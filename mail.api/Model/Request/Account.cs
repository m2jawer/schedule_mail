using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace mail.api.Model.Request
{
    public class Account
    {
        [SwaggerParameter("账号")]
        [DefaultValue("admin")]
        [Required]
        public string username
        {
            get;
            set;
        } = "";

        [SwaggerParameter("密码")]
        [DefaultValue("aa123456")]
        [Required]
        public string password
        {
            get;
            set;
        } = "";
    }
}
