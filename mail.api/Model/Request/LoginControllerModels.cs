using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace mail.api.Model.Request
{
    /// <summary>
    /// 登入提交
    /// </summary>
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

    /// <summary>
    /// 密码修改
    /// </summary>
    public class PwdEdit
    {
        [SwaggerParameter("原密码")]
        [DefaultValue("aa1233456")]
        public string pwd_old
        {
            get;
            set;
        } = "";

        [SwaggerParameter("新密码")]
        [DefaultValue("xxxxxx")]
        public string pwd
        {
            get;
            set;
        } = "";

        [SwaggerParameter("确认密码")]
        [DefaultValue("xxxxxx")]
        public string pwd2
        {
            get;
            set;
        } = "";
    }
}
