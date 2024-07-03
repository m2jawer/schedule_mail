using mail.api.Common;
using mail.api.DAL;
using mail.api.Model;
using mail.api.Pages;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace mail.api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

#if DEBUG
            //builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Schema Mail API", Version = "v1" });

                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename), true);//true 显示控制器注释

                c.OperationFilter<TokenHeaderParameter>();
                //c.AddSecurityDefinition("X-Authorization", new OpenApiSecurityScheme
                //{
                //    Description = "Authorization header. Example: \"xxxx-xxxx-xxxx-xxxx\"",
                //    In = ParameterLocation.Header,
                //    Name = "Authorization",
                //    Type = SecuritySchemeType.ApiKey
                //});
            });
#endif
            builder.Services.AddAuthorization(o =>
            {
                o.AddPolicy("Basic", policy => policy.RequireClaim("Basic"));
            });
            builder.Services.AddAuthentication("Basic").AddScheme<TokenAuthenticationOptions, TokenAuthenticationHandler>("Basic", null);
            builder.Services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
            });
            builder.Services.AddMvc(o => o.EnableEndpointRouting = false);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var appset = config.GetSection("AppSettings");

            #region 初始部署的时候，对SQLite简易微型数据库的数据初始化
            ScheduleDb db = BaseController.Db;
            if (!db.Database.EnsureCreatedAsync().Result)
            {
                db.Database.MigrateAsync().Wait();
            }
            else
            {
                //创建基本用户，预设密码如下
                Users users = new Users();
                users.Id = "admin";
                users.Pwd = "aa123456";
                users.LastLogin = DateTime.MinValue;

                db.Add(users);

                //初始化邮件SMTP服务器的初始信息，参考配置档appsettings中AppSettings节点内容
                if (appset != null)
                {
                    SysSettings sysSettings = new SysSettings();
                    sysSettings.Mail = appset["Mail_Account"];                  //用于自动发送邮件的源邮件，用于发送邮件的账号
                    sysSettings.MailPwd = appset["Mail_Secret"];                //SMTP平台要求的验证密钥，用于发送邮件的密码或验证授权密钥
                    sysSettings.MailPop3 = appset["Smtp"];                      //用于发送的系统SMTP地址
                    sysSettings.MailPort = Convert.ToInt32(appset["Smtp_Port"]);//SMTP服务器的端口，一般是465或587，具体根据平台要求进行配置
                    db.SysSettings.Add(sysSettings);
                }

                db.SaveChangesAsync().Wait();
            }
            #endregion

            CancellationTokenSource cts = new CancellationTokenSource();
            //启动计划邮件任务进程
            Task.Factory.StartNew(() => {
                Utility.StartTickingScheduleMail(db, cts.Token).Wait();
            });

            //app.UseStaticFiles();
            //使用系统路由
#if DEBUG
            app.UseRouting().UseAuthentication().UseAuthorization().UseEndpoints(endpoints =>
            {
                endpoints.MapSwagger();
            });
            app.UseSwagger().UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Schema Mail API");
                c.RoutePrefix = string.Empty;
            });
#else
            app.UseRouting().UseAuthentication();.UseAuthorization();
#endif
            //app.UseMiddleware<TokenAuthenticationMiddleware>();
            //使用MVC的controllers和action的映对
            app.MapControllers();

            app.Run();
            //服务销毁则停止计划邮件进程
            cts.Cancel();
        }
    }
}
