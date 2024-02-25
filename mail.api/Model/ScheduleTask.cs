using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace mail.api.Model
{
    [PrimaryKey("Name")]
    [Table("sys.scheduletask")]
    public class ScheduleTask
    {
        /// <summary>
        /// 任务名称
        /// </summary>
        public string Name { get; set; } = "";
        /// <summary>
        /// 任务数
        /// </summary>
        public int TaskCount { get; set; }
        /// <summary>
        /// 已发送任务数
        /// </summary>
        public int SendCount { get; set; }
        /// <summary>
        /// 未发送任务数
        /// </summary>
        public int UnSendCount { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateAt { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateAt { get; set; }
        /// <summary>
        /// 是否全部完成
        /// </summary>
        public bool IsFinish { get; set; }
    }
}
