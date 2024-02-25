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
        public string? Subject { get; set; }
        public string? MailBody { get; set; }
        public bool IsSend { get; set; }
        public DateTime LastSend { get; set; }
        public string? BatchId { get; set; }
        public int Index { get; set; }
    }
}
