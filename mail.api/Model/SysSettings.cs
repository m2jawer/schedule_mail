using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mail.api.Model
{
    [Table("sys.settings")]
    public class SysSettings
    {
        [Key]
        public int Id { get; set; } = 1;
        public string? Mail { get; set; }
        public string? MailPwd { get; set; }
        public string? MailPop3 { get; set; }
        public int MailPort { get; set; }
    }
}
