using DocumentFormat.OpenXml.Wordprocessing;
using System.ComponentModel.DataAnnotations.Schema;

namespace mail.api.Model
{
    [Table("sys.schedulemail")]
    public class ScheduleMail
    {
        /// <summary>
        /// 目标
        /// </summary>
        public string? Mail { get; set; }
        /// <summary>
        /// 发送时间
        /// </summary>
        public DateTime ScheduleTime { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        public string? NickName { get; set; }
        /// <summary>
        /// 1表示公告 2表示特定事件或生日
        /// </summary>
        public int MailType { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string? Subject { get; set; }
        /// <summary>
        /// 邮件内容
        /// </summary>
        public string? MailBody { get; set; }
        /// <summary>
        /// 是否已发
        /// </summary>
        public bool IsSend { get; set; }
        /// <summary>
        /// 发送时间
        /// </summary>
        public DateTime LastSend { get; set; }
        /// <summary>
        /// 任务编号
        /// </summary>
        public string? BatchId { get; set; }
        /// <summary>
        /// 索引
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// 生日
        /// </summary>
        public string? Birthday { get; set; }
        /// <summary>
        /// 称谓
        /// </summary>
        public string? Call { get; set; }

        /// <summary>
        /// 替换邮件标题
        /// </summary>
        /// <returns></returns>
        public string GetMailSubject()
        {
            return ReplaceTemplate(Subject!);
        }

        /// <summary>
        /// 获取替换模板后的邮件内容
        /// </summary>
        /// <returns></returns>
        public string GetMailContent()
        {
            return ReplaceTemplate(MailBody!);
        }

        private string ReplaceTemplate(string template)
        {
            //名称
            template = template.Replace("@name", NickName);
            //称谓
            template = template.Replace("@call", Call);

            if (MailType == 1)//普通公告，法定节假日
            {
                //计划节假日日期
                template = template.Replace("@date", ScheduleTime.Date.ToString("yyyy年M月d日"));
            }
            else//生日问候
            {
                DateTime birthday = GetBirthdayScheduleTime();
                //生日日期
                template = template.Replace("@date", birthday.ToString("yyyy年M月d日"));
            }

            return template;
        }

        /// <summary>
        /// 获取生日邮件计划发送时间
        /// </summary>
        /// <returns></returns>
        public DateTime GetBirthdayScheduleTime()
        {
            if (MailType == 2)
            {
                DateTime birthday = Convert.ToDateTime(DateTime.Now.Year.ToString() + "/" + Birthday);

                if (birthday < DateTime.Now)
                {
                    int newYear = DateTime.Now.Year + 1;
                    birthday = Convert.ToDateTime(newYear + "/" + Birthday);
                }

                return birthday;
            }

            return ScheduleTime;
        }
    }
}
