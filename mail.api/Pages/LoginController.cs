using mail.api.Model.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace mail.api.Pages
{
#if DEBUG
    [Route("mail/api/[controller]/[action]")]
#endif
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Produces("application/json")]
    [Authorize(AuthenticationSchemes = "Basic")]
    public class LoginController : BaseController
    {
        /// <summary>
        /// 登入验证，并获取账号token标识
        /// </summary>
        /// <param name="post">提交参数格式参考样例</param>
        /// <remarks>验证通过返回token和status=ok，否则返回status为error</remarks>
        /// <response code="200">
        /// {
        ///     "status":"ok",
        ///     "type":"account",
        ///     "currentAuthority":"admin",
        ///     "token":"xxxx-xxxx-xxxx-xxxx"
        /// }
        /// </response>t
        /// <response code="other">
        /// {
        ///     "status":"error",
        ///     "type":"account",
        ///     "currentAuthority":"guest"
        /// }
        /// </response>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Account([FromBody]Account post)
        {
            var loginObj = new JObject();

            var result = Db.Users.Where(x => x.Id == post.username && x.Pwd == post.password);

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

                ////验证通过
                //var claims = new List<Claim>
                //    {
                //        new Claim(ClaimTypes.Name, result.First().Id!),
                //    };
                //var identity = new ClaimsIdentity(claims, "Token");
                //var principal = new GenericPrincipal(identity, null);
                //SignIn(principal);
            }
            else 
            {
                loginObj["status"] = "error";
                loginObj["type"] = "account";
                loginObj["currentAuthority"] = "guest";
            }

            return Ok(loginObj);
        }

        /// <summary>
        /// 登入后获取当前账号信息
        /// </summary>
        /// <remarks>登入后才允许访问，需要传入Header键值为Authorization进行登入验证识别</remarks>
        /// <response code="200">
        /// {
        ///     "success": true,
        ///     "data"; {
        ///         "status":"ok",
        ///         "type":"account",
        ///         "currentAuthority":"admin",
        ///         "token":"xxxx-xxxx-xxxx-xxxx"
        ///     }
        /// }
        /// </response>
        /// <response code="other">
        /// {
        ///     "success": false,
        ///     "data"; {
        ///         "isLogin":"false"
        ///     }
        /// }
        /// </response>
        [HttpGet]
        public IActionResult CurrentUser()
        {
            //string? token = Request.Headers["X-Authorization"];

            var respObj = new JObject();
            respObj["success"] = true;

            //if (!string.IsNullOrEmpty(token))
            //{
            //    var result = Db.Users.Where(x => x.Token == token);

            //    if (result.Count() > 0)
            //    {
            //        var userItem = result.First();
            //        var userObj = new JObject();
            //        userObj["name"] = userItem.Id;
            //        userObj["userid"] = userItem.Id;
            //        userObj["access"] = userItem.Id;
            //        respObj["data"] = userObj;
            //        return Ok(respObj);
            //    }
            //}

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userObj = new JObject();
                userObj["name"] = User.Identity.Name;
                userObj["userid"] = User.Identity.Name;
                userObj["access"] = User.Identity.Name;
                respObj["data"] = userObj;
                return Ok(respObj);
            }

            respObj["data"] = new JObject();
            respObj["data"]!["isLogin"] = false;
            return Unauthorized(respObj);
        }

        /// <summary>
        /// 登出注销登入凭证
        /// </summary>
        /// <remarks>登入后访问才有效</remarks>
        /// <response code="200">
        /// {
        ///     "success":"true"
        /// }
        /// </response>
        /// <response code="other">
        /// {
        ///     "success":"true"
        /// }
        /// </response>
        [HttpPost]
        public IActionResult OutLogin()
        {
            //string? token = Request.Headers["X-Authorization"];

            //var respObj = new JObject();
            //respObj["success"] = true;

            //var result =  Db.Users.Where(x => x.Token == token);
            //if (result.Count() > 0)
            //{
            //    result.First().Token = string.Empty;

            //    var _ = Db.SaveChangesAsync();
            //}

            var respObj = new JObject();
            respObj["success"] = true;

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var result = Db.Users.Where(x => x.Id == User.Identity.Name);
                if (result.Count() > 0)
                {
                    result.First().Token = string.Empty;

                    var _ = Db.SaveChangesAsync();
                }
            }

            return Ok(respObj);
        }

        /// <summary>
        /// 更改用户密码
        /// </summary>
        /// <param name="post">修改密码需要原密码，新密码和确认密码等基本信息</param>
        /// <remarks>登入后访问才有效</remarks>
        /// <response code="200">
        /// {
        ///     "success":"true"
        /// }
        /// </response>
        /// <response code="other">
        /// {
        ///     "success":"true"
        /// }
        /// </response>
        [HttpPost]
        public IActionResult EditPwd([FromBody]PwdEdit post)
        {
            //string account = GetJsonValue(post, "username", "");
            //string pwd_old = GetJsonValue(post, "pwd_old", "");
            //string pwd = GetJsonValue(post, "pwd", "");
            //string pwd_confirm = GetJsonValue(post, "pwd2", "");

            string account = User.Identity!.Name!;
            string pwd_old = post.pwd_old;
            string pwd = post.pwd;
            string pwd_confirm = post.pwd2;

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

            return Ok(editResp);
        }
    }
}
