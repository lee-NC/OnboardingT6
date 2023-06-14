using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using Demo.Common.Utils;
using Demo.Services.Entities;
using Demo.Services.UserService.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Demo.Services.Helper
{
    /// <summary>
    /// 
    /// </summary>
    public partial class UserAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IUserEntityRepository _userRepo;
        private readonly ILogger<UserAuthenticationHandler> _logger;
        
        public UserAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IUserEntityRepository userRepo,
            ILogger<UserAuthenticationHandler> logger1
            ) : base(options, logger, encoder, clock)
        {
            _userRepo = userRepo;
            _logger = logger1;
        }
        
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var username = "";
            var password = "";

            if (Request.Headers.ContainsKey("Authorization"))
            {
                var authorizationHeader = Request.Headers["Authorization"].ToString();
                var authHeaderRegex = MyRegex();

                if (authHeaderRegex.IsMatch(authorizationHeader))
                {
                    var authBase64 = authHeaderRegex.Replace(authorizationHeader, "$1");
                    var handler = new JwtSecurityTokenHandler();
                    var jsonToken = handler.ReadJwtToken(authBase64).Claims;

                    username = jsonToken.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
                }
            }
            else
            {
                Response.Headers["WWW-Authenticate"] = "Basic realm=\"Authentication required\", charset=\"UTF-8\"";
                return await Task.FromResult(AuthenticateResult.Fail(new Exception("Authenticate required")));
            }

            var customer = await _userRepo.FindByUsername( username);
            string errMsg;
            if (customer == null)
            {
                _logger.LogWarning("User {@username} not found", username);
                errMsg = "user_not_found";
                Response.Headers["WWW-Authenticate"] = $"Basic realm=\"{errMsg}\", charset=\"UTF-8\"";
                return await Task.FromResult(AuthenticateResult.Fail(new Exception("Authenticate failed")));
            }

            if (customer.Status != UserEntity.Statuses.ACTIVE)
            {
                errMsg = "user_not_actived";
                Response.Headers["WWW-Authenticate"] = $"Basic realm=\"{errMsg}\", charset=\"UTF-8\"";
                return await Task.FromResult(AuthenticateResult.Fail(new Exception("Authenticate failed")));
            }

            if (customer.UserPass != null && customer.UserPass.Status == BaseCredential.Statuses.ENABLE)
            {
                //Tài khoản bị khóa do đăng nhập sai 5 lần liên tiếp
                if (customer.UserPass.IsLocking)
                {
                    errMsg = "your_account_is_locked_try_again_later";
                    Response.Headers["WWW-Authenticate"] = $"Basic realm=\"{errMsg}\", charset=\"UTF-8\"";
                    return await Task.FromResult(AuthenticateResult.Fail(new Exception("Authenticate failed")));
                }
                else
                {
                    customer.UserPass.IsLocked = false;
                }

                // if (!UserPassCheck(customer.UserPass, username, password, out errMsg))
                // {
                //     if (customer.UserPass.AccessFailedCount >= 4)
                //     {
                //         customer.UserPass.IsLocked = true;
                //         customer.UserPass.LockedDate = DateTime.UtcNow;
                //         customer.UserPass.AccessFailedCount = 0;
                //         customer.Status = UserEntity.Statuses.SUSPENDED;
                //         errMsg = "invalid_attempts_exceeded_your_account_has_been_locked";
                //     }
                //     else
                //     {
                //         customer.UserPass.AccessFailedCount += 1;
                //     }
                //
                //     await _userRepo.Update(customer, customer.UserId);
                //
                //     Response.Headers["WWW-Authenticate"] = $"Basic realm=\"{errMsg}\", charset=\"UTF-8\"";
                //     return await Task.FromResult(AuthenticateResult.Fail(new Exception("Authenticate failed")));
                // }
                // else
                // {
                //     // Rest số lần sai pass liên tiếp về 0
                //     if (customer.UserPass.AccessFailedCount > 0)
                //     {
                //         customer.UserPass.AccessFailedCount = 0;
                //     }
                //     await _userRepo.Update(customer, customer.UserId);
                // }
            }

            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, "" + customer.UserPass.Username),
                    new Claim(ClaimTypes.Role, "" + customer.Role.ToString()),
                };

            var identity = new ClaimsIdentity(claims, "password");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            return await Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, Scheme.Name)));
        }
        
        private static bool UserPassCheck(UserPassCredential credential, string? username, string? password, out string error)
        {
            error = "";
            if (credential.Username != username)
            {
                error = "UserPass Authen Error: User not match.";
                return false;
            }

            var result = EntityUtils.VerifyPassword(
            credential.PasswordHash, password, credential.PasswordSalt);
            if (!result)
            {
                error = "UserPass Authen Error: Password not match.";
                return false;
            }

            return true;
        }

        [GeneratedRegex("Bearer (.*)")]
        private static partial Regex MyRegex();
    }
}
