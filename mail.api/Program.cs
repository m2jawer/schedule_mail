using mail.api.Common;
using mail.api.DAL;
using mail.api.Model;
using mail.api.Pages;
using Microsoft.EntityFrameworkCore;

namespace mail.api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseAuthentication();

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var appset = config.GetSection("AppSettings");

            ScheduleDb db = BaseController.Db;
            if (!db.Database.EnsureCreatedAsync().Result)
            {
                db.Database.MigrateAsync().Wait();
            }
            else
            {
                //创建基本用户
                Users users = new Users();
                users.Id = "admin";
                users.Pwd = "aa123456";
                users.LastLogin = DateTime.MinValue;

                db.Add(users);

                if (appset != null)
                {
                    SysSettings sysSettings = new SysSettings();
                    sysSettings.Mail = appset["Mail_Account"];
                    sysSettings.MailPwd = appset["Mail_Secret"];
                    sysSettings.MailPop3 = appset["Smtp"];
                    sysSettings.MailPort = Convert.ToInt32(appset["Smtp_Port"]);
                    db.SysSettings.Add(sysSettings);
                }

                db.SaveChangesAsync().Wait();
            }

            CancellationTokenSource cts = new CancellationTokenSource();

            Task.Factory.StartNew(() => {
                Utility.StartTickingScheduleMail(db, cts.Token).Wait();
            });

            app.UseStaticFiles();

            app.UseRouting();
            app.MapControllers();

            app.Run();

            cts.Cancel();
        }
    }
}
