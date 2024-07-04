using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace mail.api.Model.Request
{
    /// <summary>
    /// 代理邮箱配置
    /// </summary>
    public class SmtpSetting
    {
        [SwaggerParameter("代理邮件账号")]
        [DefaultValue("example@xx.com")]
        [Required]
        public string mail
        {
            get;
            set;
        } = "";

        [SwaggerParameter("Smtp代理授权码")]
        [DefaultValue("xxxxxxx")]
        [Required]
        public string mail_pwd
        {
            get;
            set;
        } = "";

        [SwaggerParameter("Smtp服务器地址")]
        [DefaultValue("smtp.xx.com")]
        [Required]
        public string mail_pop3
        {
            get;
            set;
        } = "";

        [SwaggerParameter("Smtp服务器端口")]
        [DefaultValue("465")]
        [Required]
        public int mail_port
        {
            get;
            set;
        }
    }

    /// <summary>
    /// 更新特定邮件内容
    /// </summary>
    public class UpdateMailBody
    {
        [SwaggerParameter("任务Id")]
        [DefaultValue("1")]
        public string batch_id
        {
            get;
            set;
        } = "";

        [SwaggerParameter("更改的邮件索引")]
        [DefaultValue("1")]
        public string index
        {
            get;
            set;
        } = "";

        [SwaggerParameter("新的邮件内容，支持HTML格式")]
        [DefaultValue("<h1 style\"color:red\">内容</h1>")]
        public string mail_body
        {
            get;
            set;
        } = "";
    }

    /// <summary>
    /// 删除特定邮件计划任务
    /// </summary>
    public class RemoveSchedulSet
    {
        [SwaggerParameter("任务Id")]
        [DefaultValue("")]
        public string batch_id
        {
            get;
            set;
        } = "";

        [SwaggerParameter("[非批量]移除的计划邮件索引")]
        [DefaultValue("1")]
        public string index
        {
            get;
            set;
        } = "";

        [SwaggerParameter("[批量]移除的计划邮件索引值")]
        [DefaultValue(new string[0])]
        public string[]? batch_ids
        {
            get;
            set;
        }
    }

    /// <summary>
    /// 任务明细查询
    /// </summary>
    public class QryTaskDetail
    {
        [SwaggerParameter("任务Id")]
        [DefaultValue("")]
        public string batch_id
        {
            get;
            set;
        } = "";

        [SwaggerParameter("页面索引")]
        [DefaultValue("1")]
        public int current
        {
            get;
            set;
        }

        [SwaggerParameter("每页数据显示笔数")]
        [DefaultValue("20")]
        public int pageSize
        {
            get;
            set;
        }
    }

    /// <summary>
    /// 任务移除数据体
    /// </summary>
    public class TaskRemoveSet
    {
        [SwaggerParameter("指定删除的任务ID")]
        [DefaultValue("")]
        public string batch_id
        {
            get;
            set;
        } = "";

        [SwaggerParameter("批量删除的任务信息和索引")]
        [DefaultValue(new string[0])]
        public string[]? batch_ids
        {
            get;
            set;
        }
    }

    /// <summary>
    /// 下载任务明细表数据体
    /// </summary>
    public class DownloadMailSet
    {
        [SwaggerParameter("任务名称")]
        [DefaultValue("")]
        public string batch_id
        {
            get;
            set;
        } = "";
    }

    /// <summary>
    /// 任务列表查询数据体
    /// </summary>
    public class QryTask
    {
        [SwaggerParameter("任务名称")]
        [DefaultValue("")]
        public string batch_id
        {
            get;
            set;
        } = "";

        [SwaggerParameter("页面索引")]
        [DefaultValue("1")]
        public int current
        {
            get;
            set;
        }

        [SwaggerParameter("每页数据显示笔数")]
        [DefaultValue("20")]
        public int pageSize
        {
            get;
            set;
        }
    }
}
