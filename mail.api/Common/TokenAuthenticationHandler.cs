using mail.api.Pages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Encodings.Web;

namespace mail.api.Common
{
    public class TokenAuthenticationHandler : AuthenticationHandler<TokenAuthenticationOptions>
    {
        public TokenAuthenticationHandler(IOptionsMonitor<TokenAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder) : base(options, logger, encoder)
        { }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string? token = Request.Headers["X-Authorization"];

            if (!string.IsNullOrEmpty(token))
            {
                var result = BaseController.Db.Users.Where(x => x.Token == token);

                if (result.Count() > 0)
                {
                    //验证通过
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, result.First().Id!),
                    };
                    var identity = new ClaimsIdentity(claims, Scheme.Name);
                    var principal = new GenericPrincipal(identity, null);
                    AuthenticationTicket ticket = new AuthenticationTicket(principal, Scheme.Name);
                    return Task.FromResult(AuthenticateResult.Success(ticket));
                }
            }

            Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.FromResult(AuthenticateResult.Fail("错误的Token信息！"));
        }
    }

    public class TokenAuthenticationOptions : AuthenticationSchemeOptions
    {
    }
}
