using mail.api.Common;
using mail.api.Model;
using mail.api.Model.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Text.Json;

namespace mail.api.Pages
{
#if DEBUG
    [Route("mail/api/[controller]/[action]")]
#endif
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Produces("application/json")]
    [Authorize(AuthenticationSchemes = "Basic")]
    public class MailController : BaseController
    {
        /// <summary>
        /// [登入后可用]获取邮箱服务代发设定信息
        /// </summary>
        /// <remarks>根据不同的SMTP服务商设定要求，展示对应的配置信息</remarks>
        /// <response code="200">
        /// {
        ///     "Id":"1",
        ///     "Mail":"xx@xx.xxx",
        ///     "MailPwd":"xxxxxxxx",
        ///     "MailPort":"465",
        ///     "MailPop3":"smtp.xx.com"
        /// }
        /// </response>
        /// <response code="other">
        /// { }
        /// </response>
        [HttpGet]
        public IActionResult GetSettings()
        {
            if (Db.SysSettings.Count() == 0)
            {
                return Ok(new { });
            }
            else
            {
                var setItem = Db.SysSettings.First();

                return Ok(setItem);
            }
        }

        /// <summary>
        /// [登入后可用]获取邮箱服务代发设定信息
        /// </summary>
        /// <remarks>根据不同的SMTP服务商设定要求，展示对应的配置信息</remarks>
        /// <param name="smtpSet">Smtp服务器设置</param>
        /// <response code="200">
        /// {
        ///     "Id":"1",
        ///     "Mail":"xx@xx.xxx",
        ///     "MailPwd":"xxxxxxxx",
        ///     "MailPort":"465",
        ///     "MailPop3":"smtp.xx.com"
        /// }
        /// </response>
        /// <response code="other">
        /// { }
        /// </response>
        [HttpPost]
        public async Task<IActionResult> UpdateSettings(SmtpSetting smtpSet)
        {
            var setting = new SysSettings();
            setting.Mail = smtpSet.mail;
            setting.MailPwd = smtpSet.mail_pwd;
            setting.MailPop3 = smtpSet.mail_pop3;
            setting.MailPort = smtpSet.mail_port;

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
            return Ok(respObj);
        }

        /// <summary>
        /// 上传邮件排程计划
        /// </summary>
        /// <remarks>上传样板字段为：序号、目标邮件、发送时间、昵称、类型、主题、内容，支持多笔批量上传</remarks>
        /// <param name="file">邮件模板文件</param>
        /// <response code="200">
        /// {
        ///     "Id":"1",
        ///     "Mail":"xx@xx.xxx",
        ///     "MailPwd":"xxxxxxxx",
        ///     "MailPort":"465",
        ///     "MailPop3":"smtp.xx.com"
        /// }
        /// </response>
        /// <response code="other">
        /// { 
        ///     "success": false,
        ///     "msg": "没有上传文件信息!"
        /// }
        /// </response>
        [HttpPost]
        public async Task<IActionResult> UploadSchedule(IFormFile file)
        {
            var respObj = new JObject();
            respObj["success"] = false;
            if (file == null)
            {
                respObj["msg"] = "没有上传文件信息!";
                return Ok(respObj);
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

                //删除计划邮件，然后重新新增上去
                var removeMails = Db.ScheduleMail.Where(x => x.BatchId == taskName);

                Db.ScheduleMail.RemoveRange(removeMails.ToArray());

                foreach (DataRow row in dt.Rows)
                {
                    //int idx = Convert.ToInt32(row[0]);

                    //var mailItem = Db.ScheduleMail.Where(x => x.Index == idx);

                    //if (mailItem.Count() == 0)
                    //{
                    var mail = new ScheduleMail();
                    mail.BatchId = taskName;
                    mail.Index = Convert.ToInt32(row[0]);
                    mail.Mail = row[1].ToString();
                    mail.NickName = row[3].ToString();
                    mail.Call = row[4].ToString();
                    mail.Birthday = row[5].ToString();
                    mail.MailType = row[6].ToString() == "公告" ? 1 : 2;
                    if (mail.MailType == 2)
                    {
                        mail.ScheduleTime = mail.GetBirthdayScheduleTime();
                    }
                    else
                    {
                        mail.ScheduleTime = Convert.ToDateTime(row[2]);
                    }
                    mail.Subject = row[7].ToString();
                    mail.MailBody = row[8].ToString();
                    mail.IsSend = false;
                    mail.LastSend = DateTime.MinValue;

                    Db.ScheduleMail.Add(mail);
                    //}
                    //else
                    //{
                    //    var mail = mailItem.First();
                    //    mail.BatchId = taskName;
                    //    mail.Index = Convert.ToInt32(row[0]);
                    //    mail.Mail = row[1].ToString();
                    //    mail.ScheduleTime = Convert.ToDateTime(row[2]);
                    //    mail.NickName = row[3].ToString();
                    //    mail.MailType = row[4].ToString() == "公告" ? 1 : 2;
                    //    mail.Subject = row[5].ToString();
                    //    mail.MailBody = row[6].ToString();
                    //}
                }

                _ = Db.SaveChangesAsync();

                respObj["success"] = true;

                return Ok(respObj);
            }
            else
            {
                respObj["msg"] = "文件内容是空的!";

                return Ok(respObj);
            }
        }

        /// <summary>
        /// 编辑单个邮件内容(未启用)
        /// </summary>
        /// <remarks>根据任务id，邮件索引编号，更改邮件内容</remarks>
        /// <param name="mailSet"></param>
        /// <response code="200">
        /// {
        ///     "success": true
        /// }
        /// </response>
        /// <response code="other">
        /// {
        ///     "success": false,
        ///     "msg": "编辑资料不存在!"
        /// }
        /// </response>
        [HttpPost]
        public async Task<IActionResult> UpdataScheduleMailContent([FromBody]UpdateMailBody mailSet)
        {
            var respObj = new JObject();
            respObj["success"] = false;

            string batchId = mailSet.batch_id;
            string index = mailSet.index;
            string mailBody = mailSet.mail_body;

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

            return Ok(respObj);
        }

        /// <summary>
        /// 移除特定计划排程邮件
        /// </summary>
        /// <remarks>移除提交的计划排程邮件,移除的邮件是明确的任务列表下的排程计划邮件资料</remarks>
        /// <response code="200">
        /// {
        ///     "success": true
        /// }
        /// </response>
        /// <response code="other">
        /// {
        ///     "success": false,
        ///     "msg": "删除的资料不存在!"
        /// }
        /// </response>
        [HttpPost]
        public async Task<IActionResult> RemoveScheduleMail(RemoveSchedulSet removeSet)
        {
            var respObj = new JObject();
            respObj["success"] = false;

            string batchId = removeSet.batch_id;
            string index = removeSet.index;

            if (string.IsNullOrEmpty(batchId))
            {
                //string batchIds = GetJsonValue(input, "batch_ids", "");
                var removeList = removeSet.batch_ids;// JArray.Parse(batchIds);

                string taskName = "";
                foreach (var item in removeList!)
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

            return Ok(respObj);
        }

        /// <summary>
        /// 下载指定任务ID的排程邮件模板
        /// </summary>
        /// <remarks>方便重新更改和编辑邮件模板内容</remarks>
        /// <param name="post">提交指定的任务名称进行下载</param>
        /// <response code="200">
        /// 直接输出邮件模板文件信息，格式为excel（xlxs）
        /// </response>
        /// <response code="other">
        /// {
        ///     "success": false,
        ///     "msg": "没有符合资料的计划邮件!"
        /// }
        /// </response>
        [HttpPost]
        public async Task<IActionResult> DownLoadScheduleMailList([FromBody]DownloadMailSet post)
        {
            var respObj = new JObject();
            respObj["success"] = false;

            string batchId = post.batch_id;// GetJsonValue(input, "batch_id", "");
            var downloadItems = Db.ScheduleMail.Where(x => x.BatchId == batchId);

            if (downloadItems.Count() > 0)
            {                    
                return File(Utility.GeneralExcelStream(await downloadItems.ToArrayAsync()), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", batchId + ".xlsx");
            }
            else
            {
                respObj["msg"] = "没有符合资料的计划邮件!";
            }

            return Ok(respObj);
        }

        /// <summary>
        /// 返回计划排程任务列表
        /// </summary>
        /// <remarks>根据条件查询邮件排程信息</remarks>
        /// <param name="post">任务列表查询参数</param>
        /// <response code="200">
        /// {
        ///     "success": true,
        ///     "data": {...},
        ///     "total": 10
        /// }
        /// </response>
        /// <response code="other">
        /// {
        ///     "success": false,
        ///     "error": "错误信息"
        /// }
        /// </response>
        [HttpPost]
        public async Task<IActionResult> GetTaskList([FromBody]QryTask post)
        {
            var respObj = new JObject();
            respObj["success"] = true;
            try
            {
                //int pageIndex = GetJsonValue(input, "page", 1);
                //int pageSize = GetJsonValue(input, "pageSize", 10);

                //string batchId = GetJsonValue(input, "batch_id", "");
                string batchId = post.batch_id;// string.IsNullOrEmpty(batch_id) ? "" : batch_id;
                int pageIndex = post.current;//string.IsNullOrEmpty(page) ? 1 : Convert.ToInt32(page);
                int pageSize = post.pageSize;//string.IsNullOrEmpty(page_size) ? 10 : Convert.ToInt32(page_size);

                if (!string.IsNullOrEmpty(batchId))
                {
                    var count = Db.ScheduleTask.Where(x => (x.Name.Contains(batchId))).Count();
                    var tasks = Db.ScheduleTask.Where(x => (x.Name.Contains(batchId))).Skip((pageIndex - 1) * pageSize).Take(pageSize).Select(
                        x => new {
                            x.Name,
                            x.TaskCount,
                            x.UpdateAt,
                            x.CreateAt,
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
                            x.Name,
                            x.TaskCount,
                            x.UpdateAt,
                            x.CreateAt,
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

            return Ok(respObj);
        }

        /// <summary>
        /// 根据任务编号查询排程邮件明细信息
        /// </summary>
        /// <remarks>展示任务明细的排程邮件列表，包括目标、发送时间、昵称、邮件类型（公告,生日）、标题、邮件内容、是否已发、发送时间、任务编号、索引</remarks>
        /// <param name="post"></param>
        /// <response code="200">
        /// {
        ///     "success": true,
        ///     "data": {...},
        ///     "total": 10
        /// }
        /// </response>
        /// <response code="other">
        /// {
        ///     "success": false,
        ///     "error": "错误信息"
        /// }
        /// </response>
        [HttpPost]
        public async Task<IActionResult> GetTaskDetails([FromBody]QryTaskDetail post)
        {
            var respObj = new JObject();
            respObj["success"] = true;
            try
            {
                int pageIndex = post.current;
                int pageSize = post.pageSize;
                string batchId = post.batch_id;
                var taskMails = Db.ScheduleMail.Where(x => x.BatchId == batchId).OrderBy(x => x.Index);
                respObj["total"] = taskMails.Count();
                //foreach (var t in taskMails)
                //{
                //    t.LastSend = DateTime.Now;
                //}
                var respItems = await taskMails.Skip(pageIndex - 1 * pageSize).Take(pageSize).ToArrayAsync();

                foreach (var item in respItems)
                { 
                    item.Subject = item.GetMailSubject();
                    item.MailBody = item.GetMailContent();
                }

                respObj["data"] = JArray.Parse(JsonConvert.SerializeObject(respItems));
            }
            catch (Exception ex)
            {
                respObj["success"] = false;

                respObj["error"] = ex.Message + ex.StackTrace;
            }

            return Ok(respObj);
        }

        /// <summary>
        /// 根据任务名称移除排程任务，并同时删除任务下的所有排程计划邮件信息
        /// </summary>
        /// <remarks>出于数据安全考量，请确认有任务备份文档或先下载任务表格，然后再执行确认删除</remarks>
        /// <param name="post">指定删除的任务信息，支持批量删除提交</param>
        /// <response code="200">
        /// {
        ///     "success": true
        /// }
        /// </response>
        /// <response code="other">
        /// {
        ///     "success": false,
        ///     "error": "错误信息"
        /// }
        /// </response>
        [HttpPost]
        public async Task<IActionResult> RemoveTaskByName([FromBody]TaskRemoveSet post)
        {
            var respObj = new JObject();
            respObj["success"] = true;
            try
            {
                string batchId = post.batch_id;

                if (string.IsNullOrEmpty(batchId))
                {
                    //批量删除
                    var removeList = post.batch_ids!;

                    var task = Db.ScheduleTask.Where(x => removeList.Contains(x.Name));

                    if (task.Count() > 0)
                    {
                        Db.ScheduleTask.RemoveRange(task.ToArray());
                    }

                    var taskMails = Db.ScheduleMail.Where(x => removeList.Contains(x.BatchId));

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

            return Ok(respObj);
        }
    }
}
