using mail.api.Common;
using mail.api.Model;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Text.Json;

namespace mail.api.Pages
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MailController : BaseController
    {
        [HttpGet]
        public IActionResult GetSettings()
        {
            if (Db.SysSettings.Count() == 0)
            {
                return Ok("{}");
            }
            else
            {
                var setItem = Db.SysSettings.First();

                return Ok(JsonConvert.SerializeObject(setItem));
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSettings(JsonElement input)
        {
            var setting = new SysSettings();
            setting.Mail = GetJsonValue(input, "mail", "");
            setting.MailPwd = GetJsonValue(input, "mail_pwd", "");
            setting.MailPop3 = GetJsonValue(input, "mail_pop3", "");
            setting.MailPort = GetJsonValue(input, "mail_port", 0);

            var result = Db.SysSettings.First();

            if (result == null)
            {
                Db.Add(setting);
            }
            else
            {
                result.MailPwd = setting.MailPwd;
                result.MailPop3 = setting.MailPop3;
                result.MailPort = setting.MailPort;
                result.Mail = setting.Mail;
            }
            
            await Db.SaveChangesAsync();

            var respObj = new JObject();
            respObj["success"] = true;
            return Ok(respObj.ToString(Formatting.None));
        }

        [HttpPost]
        public async Task<IActionResult> UploadSchedule(IFormFile file)
        {
            var respObj = new JObject();
            respObj["success"] = false;
            if (file == null)
            {
                respObj["msg"] = "没有上传文件信息!";
                return Ok(respObj.ToString(Formatting.None));
            }

            string saveFile = await Utility.SaveUploadFile(file.OpenReadStream(), file.FileName);

            var dt = Utility.LoadFromXls(saveFile);

            if (dt != null && dt.Rows.Count > 0)
            {
                DataView dataView = dt.DefaultView;

                var distinctDt = dataView.ToTable(true, dt.Columns[0].ColumnName);

                foreach (DataRow row in distinctDt.Rows)
                {
                    var result = Db.ScheduleMail.Where(x => x.BatchId == file.FileName);

                    if (result.Count() > 0)
                    {
                        Db.RemoveRange(result);
                    }
                }

                ScheduleTask task;

                string taskName = file.FileName.Substring(0, file.FileName.LastIndexOf('.'));
                var taskResult = Db.ScheduleTask.Where(x => x.Name == taskName);
                if (taskResult.Count() > 0)
                {
                    task = taskResult.First();

                    task.TaskCount = dt.Rows.Count;
                    task.UpdateAt = DateTime.Now;
                }
                else
                {
                    task = new ScheduleTask();

                    task.Name = taskName;
                    task.CreateAt = DateTime.Now;
                    task.TaskCount = dt.Rows.Count;
                    task.UpdateAt = DateTime.Now;

                    Db.ScheduleTask.Add(task);
                }

                foreach (DataRow row in dt.Rows)
                {
                    int idx = Convert.ToInt32(row[0]);

                    var mailItem = Db.ScheduleMail.Where(x => x.Index == idx);

                    if (mailItem.Count() == 0)
                    {
                        var mail = new ScheduleMail();
                        mail.BatchId = taskName;
                        mail.Index = Convert.ToInt32(row[0]);
                        mail.Mail = row[1].ToString();
                        mail.ScheduleTime = Convert.ToDateTime(row[2]);
                        mail.NickName = row[3].ToString();
                        mail.MailType = row[4].ToString() == "公告" ? 1 : 2;
                        mail.Subject = row[5].ToString();
                        mail.MailBody = row[6].ToString();
                        mail.IsSend = false;
                        mail.LastSend = DateTime.MinValue;

                        Db.Add(mail);
                    }
                    else
                    {
                        var mail = mailItem.First();
                        mail.BatchId = taskName;
                        mail.Index = Convert.ToInt32(row[0]);
                        mail.Mail = row[1].ToString();
                        mail.ScheduleTime = Convert.ToDateTime(row[2]);
                        mail.NickName = row[3].ToString();
                        mail.MailType = row[4].ToString() == "公告" ? 1 : 2;
                        mail.Subject = row[5].ToString();
                        mail.MailBody = row[6].ToString();
                    }
                }

                _ = Db.SaveChangesAsync();

                respObj["success"] = true;

                return Ok(respObj.ToString(Formatting.None));
            }
            else
            {
                respObj["msg"] = "文件内容是空的!";

                return Ok(respObj.ToString(Formatting.None));
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdataScheduleMailContent(JsonElement input)
        {
            var respObj = new JObject();
            respObj["success"] = false;

            string batchId = GetJsonValue(input, "batch_id", "");
            string index = GetJsonValue(input, "index", "");
            string mailBody = GetJsonValue(input, "mail_body", "");

            var editItem = Db.ScheduleMail.Where(x => x.BatchId == batchId && x.Index == Convert.ToInt32(index));

            if (editItem.Count() > 0)
            {
                editItem.First().MailBody = mailBody;

                await Db.SaveChangesAsync();

                respObj["success"] = true;
            }
            else
            {
                respObj["msg"] = "编辑资料不存在!";
            }

            return Ok(respObj.ToString(Formatting.None));
        }

        [HttpPost]
        public async Task<IActionResult> RemoveScheduleMail(JsonElement input)
        {
            var respObj = new JObject();
            respObj["success"] = false;

            string batchId = GetJsonValue(input, "batch_id", "");
            string index = GetJsonValue(input, "index", "");

            if (string.IsNullOrEmpty(batchId))
            {
                string batchIds = GetJsonValue(input, "batch_ids", "");
                var removeList = JArray.Parse(batchIds);

                string taskName = "";
                foreach (var item in removeList)
                {
                    string[] removeItem = item.ToString().Split('-');
                    var task = Db.ScheduleMail.Where(x => x.BatchId == removeItem[0] && x.Index == Convert.ToInt32(removeItem[1]));

                    if (task.Count() > 0)
                    {
                        Db.ScheduleMail.RemoveRange(task);
                    }

                    taskName = removeItem[0];
                }

                await Db.SaveChangesAsync();

                //删除后更新子任务数量
                var targetTask = Db.ScheduleTask.Where(x => x.Name == taskName);

                if (targetTask.Count() > 0)
                {
                    var taskResult = targetTask.First();

                    taskResult.TaskCount = Db.ScheduleMail.Where(x => x.BatchId == taskName).Count();
                    taskResult.SendCount = Db.ScheduleMail.Where(x => x.BatchId == taskName && x.IsSend == true).Count();
                    taskResult.UnSendCount = taskResult.TaskCount - taskResult.SendCount;
                    taskResult.IsFinish = taskResult.UnSendCount == 0;
                    taskResult.UpdateAt = DateTime.Now;
                }

                await Db.SaveChangesAsync();

                respObj["success"] = true;
            }
            else
            {
                var removeItem = Db.ScheduleMail.Where(x => x.BatchId == batchId && x.Index == Convert.ToInt32(index));

                if (removeItem.Count() > 0)
                {
                    Db.ScheduleMail.Remove(removeItem.First());

                    await Db.SaveChangesAsync();

                    respObj["success"] = true;
                }
                else
                {
                    respObj["msg"] = "删除的资料不存在!";
                }
            }

            return Ok(respObj.ToString(Formatting.None));
        }

        [HttpPost]
        public async Task<IActionResult> DownLoadScheduleMailList(JsonElement input)
        {
            var respObj = new JObject();
            respObj["success"] = false;

            string batchId = GetJsonValue(input, "batch_id", "");
            var downloadItems = Db.ScheduleMail.Where(x => x.BatchId == batchId);

            if (downloadItems.Count() > 0)
            {                    
                return File(Utility.GeneralExcelStream(await downloadItems.ToArrayAsync()), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "template_" + DateTime.Now.ToString("ffffff") + ".xlsx");
            }
            else
            {
                respObj["msg"] = "没有符合资料的计划邮件!";
            }

            return Ok(respObj.ToString(Formatting.None));
        }

        [HttpPost]
        public async Task<IActionResult> GetTaskList(JsonElement input)
        {
            var respObj = new JObject();
            respObj["success"] = true;
            try
            {
                int pageIndex = GetJsonValue(input, "page", 1);
                int pageSize = GetJsonValue(input, "pageSize", 10);

                string batchId = GetJsonValue(input, "batch_id", "");

                if (!string.IsNullOrEmpty(batchId))
                {
                    var count = Db.ScheduleTask.Where(x => (x.Name.Contains(batchId))).Count();
                    var tasks = Db.ScheduleTask.Where(x => (x.Name.Contains(batchId))).Skip((pageIndex - 1) * pageSize).Take(pageSize).Select(
                        x => new {
                            Name = x.Name,
                            TaskCount = x.TaskCount,
                            UpdateAt = x.UpdateAt,
                            CreateAt = x.CreateAt,
                            SendCount = Db.ScheduleMail.Where(y => y.BatchId == x.Name && y.IsSend).Count(),
                            UnSendCount = x.TaskCount - x.SendCount,
                            IsFinish = x.UnSendCount == 0,
                        });

                    respObj["data"] = JArray.Parse(JsonConvert.SerializeObject(await tasks.ToArrayAsync()));
                    respObj["total"] = count;
                }
                else
                {
                    var count = Db.ScheduleTask.Count();
                    var tasks = Db.ScheduleTask.Skip((pageIndex - 1) * pageSize).Take(pageSize).Select(
                        x => new {
                            Name = x.Name,
                            TaskCount = x.TaskCount,
                            UpdateAt = x.UpdateAt,
                            CreateAt = x.CreateAt,
                            SendCount = Db.ScheduleMail.Where(y => y.BatchId == x.Name && y.IsSend).Count(),
                            UnSendCount = x.TaskCount - Db.ScheduleMail.Where(y => y.BatchId == x.Name && y.IsSend).Count(),
                            IsFinish = x.TaskCount == Db.ScheduleMail.Where(y => y.BatchId == x.Name && y.IsSend).Count(),
                        });

                    respObj["data"] = JArray.Parse(JsonConvert.SerializeObject(await tasks.ToArrayAsync()));
                    respObj["total"] = count;
                }
            }
            catch (Exception ex)
            {
                respObj["success"] = false;

                respObj["error"] = ex.Message + ex.StackTrace;
            }

            return Ok(respObj.ToString(Formatting.None));
        }

        [HttpPost]
        public async Task<IActionResult> GetTaskDetails(JsonElement input)
        {
            var respObj = new JObject();
            respObj["success"] = true;
            try
            {
                int pageIndex = GetJsonValue(input, "page", 1);
                int pageSize = GetJsonValue(input, "pageSize", 10);

                string batchId = GetJsonValue(input, "batch_id", "");
                var taskMails = Db.ScheduleMail.Where(x => x.BatchId == batchId);

                respObj["total"] = taskMails.Count();

                respObj["data"] = JArray.Parse(JsonConvert.SerializeObject(await taskMails.Skip(pageIndex - 1 * pageSize).Take(pageSize).ToArrayAsync()));
            }
            catch (Exception ex)
            {
                respObj["success"] = false;

                respObj["error"] = ex.Message + ex.StackTrace;
            }

            return Ok(respObj.ToString(Formatting.None));
        }

        [HttpPost]
        public async Task<IActionResult> RemoveTaskByName(JsonElement input)
        {
            var respObj = new JObject();
            respObj["success"] = true;
            try
            {
                string batchId = GetJsonValue(input, "batch_id", "");

                if (string.IsNullOrEmpty(batchId))
                {
                    //批量删除
                    string batchIds = GetJsonValue(input, "batch_ids", "");
                    var removeList = JArray.Parse(batchIds);

                    var task = Db.ScheduleTask.Where(x => removeList.ToObject<List<string>>()!.ToArray().Contains(x.Name));

                    if (task.Count() > 0)
                    {
                        Db.ScheduleTask.RemoveRange(task.ToArray());
                    }

                    var taskMails = Db.ScheduleMail.Where(x => removeList.ToObject<List<string>>()!.ToArray().Contains(x.BatchId));

                    if (taskMails.Count() > 0)
                    {
                        Db.ScheduleMail.RemoveRange(taskMails.ToArray());
                    }

                    await Db.SaveChangesAsync();
                }
                else
                {
                    var task = Db.ScheduleTask.Where(x => x.Name == batchId);

                    if (task.Count() > 0)
                    {
                        Db.ScheduleTask.RemoveRange(task.ToArray());
                    }

                    var taskMails = Db.ScheduleMail.Where(x => x.BatchId == batchId);

                    if (taskMails.Count() > 0)
                    {
                        Db.ScheduleMail.RemoveRange(taskMails.ToArray());

                        await Db.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                respObj["success"] = false;

                respObj["error"] = ex.Message + ex.StackTrace;
            }

            return Ok(respObj.ToString(Formatting.None));
        }
    }
}
