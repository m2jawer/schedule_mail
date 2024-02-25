using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mail.api.Model
{
    [Table("sys.users")]
    public class Users
    {
        [Key]
        public string? Id { get; set; }
        public string? Pwd { get; set; }
        public DateTime LastLogin { get; set; }
        public string? Token { get; set; }
    }
}
