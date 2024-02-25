using ClosedXML.Excel;
using mail.api.DAL;
using mail.api.Model;
using System.Data;
using System.Net;
using System.Net.Mail;

namespace mail.api.Common
{
    public static class Utility
    {
        private static readonly string[] CONST_HEADER = ["批次", "索引", "邮箱", "发送时间", "昵称", "类别", "标题", "内容"];

        public static async Task<bool> SendMail(string user, string password, string smtp, string port, string mailTo, string subject, string mailContent)
        {
            try
            {
                var smtpClient = new SmtpClient(smtp)
                {
                    Port = Convert.ToInt32(port),
                    Credentials = new NetworkCredential(user, password),
                    EnableSsl = true,
                };

                var msg = new MailMessage(user, mailTo, subject, mailContent);
                msg.IsBodyHtml = true;
                await smtpClient.SendMailAsync(msg);
            }
            catch (Exception ex)
            {
                ex.ToString();
                return false;
            }

            return true;
        }

        public static async Task<string> SaveUploadFile(Stream stream, string fileName)
        {
            string directory = AppDomain.CurrentDomain.BaseDirectory + "Upload\\";

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string uploadPath = directory + fileName;

            using (FileStream fs = new FileStream(uploadPath, FileMode.OpenOrCreate))
            {
                if (stream.CanSeek)
                {
                    stream.Seek(0, SeekOrigin.Begin);
                }
                await stream.CopyToAsync(fs);
            }

            return uploadPath;
        }

        public static DataTable LoadFromXls(string filePath, int sheetIndex = 1)
        {
            DataTable dt = new DataTable();
            using (XLWorkbook wb = new XLWorkbook(filePath))
            {
                var workSheet = wb.Worksheet(sheetIndex);

                int rows = workSheet.Rows().Count();
                int cols = workSheet.Columns().Count();

                for (int j = 1; j <= cols; j++)
                {
                    dt.Columns.Add(workSheet.Cell(1, j).Value.ToString(), typeof(string));
                }

                // Loop through rows
                for (int i = 2; i <= rows; i++)
                {
                    var row = dt.NewRow();

                    // Loop through each column in selected row
                    for (int j = 1; j <= cols; j++)
                    {
                        row[j - 1] = workSheet.Cell(i, j).Value.ToString();
                    }

                    dt.Rows.Add(row);
                }
            }

            File.Delete(filePath);

            return dt;
        }

        public static Stream GeneralExcelStream(ScheduleMail[] mails)
        {
            string directory = AppDomain.CurrentDomain.BaseDirectory + "Temp\\";

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string dlFile = directory + "dl_" + DateTime.Now.ToString("ffffff") + ".xlsx";

            using (XLWorkbook workbook = new XLWorkbook())
            {
                var sheet = workbook.Worksheets.Add("Mails");

                for (int i = 0; i < CONST_HEADER.Length; i++)
                {
                    sheet.Cell(1, i + 1).Value = CONST_HEADER[i];
                }

                for (int i = 2; i <= mails.Length + 1; i++)
                {
                    sheet.Cell(i, 1).Value = mails[i - 2].BatchId;
                    sheet.Cell(i, 2).Value = mails[i - 2].Index;
                    sheet.Cell(i, 3).Value = mails[i - 2].Mail;
                    sheet.Cell(i, 4).Value = mails[i - 2].ScheduleTime;
                    sheet.Cell(i, 5).Value = mails[i - 2].NickName;
                    sheet.Cell(i, 6).Value = mails[i - 2].MailType == 1 ? "公告" : "生日";
                    sheet.Cell(i, 7).Value = mails[i - 2].Subject;
                    sheet.Cell(i, 8).Value = mails[i - 2].MailBody;
                }
                workbook.SaveAs(dlFile);
            }

            MemoryStream ms = new MemoryStream();

            using (var fileStream = File.OpenRead(dlFile))
            {
                fileStream.CopyTo(ms);
            }

            File.Delete(dlFile);
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        public static async Task StartTickingScheduleMail(ScheduleDb db, CancellationToken token)
        {
            int loop = 1;
            ParallelOptions parallelOptions = new();
            parallelOptions.MaxDegreeOfParallelism = 4;

            BeginSchedule:

            SysSettings? lastSet = null;
            while (!token.IsCancellationRequested)
            {
                if (db.SysSettings.Count() > 0)
                {
                    lastSet = db.SysSettings.First();

                    break;
                }

                Thread.Sleep(10000);
            }

            while (!token.IsCancellationRequested)
            {
                //更新邮件配置信息
                lastSet = db.SysSettings.First();

                var now = DateTime.Now;
                var targetMails = db.ScheduleMail.Where(x => x.IsSend == false && x.ScheduleTime < now);

                if (targetMails.Count() > 0)
                {
                    await Parallel.ForEachAsync(targetMails.ToArray(), parallelOptions, async (item, value) =>
                    {
                        if (await SendMail(lastSet!.Mail!, lastSet.MailPwd!, lastSet.MailPop3!, lastSet.MailPort.ToString(), item.Mail!, item.Subject!, item.MailBody!))
                        {
                            item.IsSend = true;
                            item.LastSend = DateTime.Now;
                        }
                    });

                    await db.SaveChangesAsync();
                }
                //一分钟轮训一次邮件
                Thread.Sleep(60000);

                loop++;

                //30分钟更新一次正常日志
                if (loop > 30)
                {
                    Console.WriteLine("[" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "] Mail schedule working healthy...");
                    loop = 1;

                    if (db.SysSettings.Count() == 0)
                    {
                        goto BeginSchedule;
                    }
                    lastSet = db.SysSettings.First();
                }
            }
        }
    }
}
