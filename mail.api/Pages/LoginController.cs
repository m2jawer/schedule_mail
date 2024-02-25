using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace mail.api.Pages
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class LoginController : BaseController
    {
        [HttpPost]
        public async Task<IActionResult> Account(JsonElement post)
        {
            var loginObj = new JObject();
            string account = GetJsonValue(post, "username", "");
            string password = GetJsonValue(post, "password", "");

            var result = Db.Users.Where(x => x.Id == account && x.Pwd == password);

            if (result.Count() > 0)
            {
                loginObj["status"] = "ok";
                loginObj["type"] = "account";
                loginObj["currentAuthority"] = "admin";

                string token = Guid.NewGuid().ToString();
                loginObj["token"] = token;

                result.First().LastLogin = DateTime.Now;
                result.First().Token = token;

                await Db.SaveChangesAsync();
            }
            else 
            {
                loginObj["status"] = "error";
                loginObj["type"] = "account";
                loginObj["currentAuthority"] = "guest";
            }

            return Ok(loginObj.ToString(Newtonsoft.Json.Formatting.None));
        }

        [HttpGet]
        public IActionResult CurrentUser()
        {
            string? token = Request.Headers["Authorization"];

            var respObj = new JObject();
            respObj["success"] = true;

            if (!string.IsNullOrEmpty(token))
            {
                var result = Db.Users.Where(x => x.Token == token);

                if (result.Count() > 0)
                {
                    var userItem = result.First();
                    var userObj = new JObject();
                    userObj["name"] = userItem.Id;
                    userObj["userid"] = userItem.Id;
                    userObj["access"] = userItem.Id;
                    respObj["data"] = userObj;
                    return Ok(respObj.ToString(Formatting.None));
                }
            }

            respObj["data"] = new JObject();
            respObj["data"]!["isLogin"] = false;
            return Unauthorized(respObj.ToString(Formatting.None));
        }

        [HttpPost]
        public IActionResult OutLogin()
        {
            string? token = Request.Headers["Authorization"];

            var respObj = new JObject();
            respObj["success"] = true;

            var result =  Db.Users.Where(x => x.Token == token);
            if (result.Count() > 0)
            {
                result.First().Token = string.Empty;

                var _ = Db.SaveChangesAsync();
            }

            return Ok(respObj.ToString(Formatting.None));
        }

        [HttpPost]
        public IActionResult EditPwd(JsonElement post)
        {
            string account = GetJsonValue(post, "username", "");
            string pwd_old = GetJsonValue(post, "pwd_old", "");
            string pwd = GetJsonValue(post, "pwd", "");
            string pwd_confirm = GetJsonValue(post, "pwd2", "");

            var result = Db.Users.Where(x => x.Id == account && x.Pwd == pwd_old);
            var editResp = new JObject();
            editResp["success"] = false;

            if (result.Count() == 0)
            {
                editResp["msg"] = "旧密码错误";
            }
            else
            {
                if (pwd == pwd_confirm)
                {
                    result.First().Pwd = pwd;

                    Db.SaveChangesAsync();

                    editResp["success"] = true;
                }
                else
                {
                    editResp["msg"] = "两次密码不相同";
                }
            }

            return Ok(editResp.ToString(Newtonsoft.Json.Formatting.None));
        }
    }
}
