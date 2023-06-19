using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using Demo.Common.Utils;
using Demo.Services.UserService.Entity.Api.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using UserService.Store;

namespace Demo.Services.Helper;

/// <summary>
/// </summary>
public partial class UserAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly ILogger<UserAuthenticationHandler> _logger;
    private readonly IUserEntityStore _userStore;

    public UserAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IUserEntityStore userStore,
        ILogger<UserAuthenticationHandler> logger1
    ) : base(options, logger, encoder, clock)
    {
        _userStore = userStore;
        _logger = logger1;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userId = "";

        if (Request.Headers.ContainsKey("Authorization"))
        {
            var authorizationHeader = Request.Headers["Authorization"].ToString();
            var authHeaderRegex = MyRegex();

            if (authHeaderRegex.IsMatch(authorizationHeader))
            {
                var authBase64 = authHeaderRegex.Replace(authorizationHeader, "$1");
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadJwtToken(authBase64).Claims;

                userId = jsonToken.First(claim => claim.Type == "userId").Value;
            }
        }
        else
        {
            Response.Headers["WWW-Authenticate"] = "Basic realm=\"Authentication required\", charset=\"UTF-8\"";
            return await Task.FromResult(AuthenticateResult.Fail(new Exception("Authenticate required")));
        }

        var customer = await _userStore.FindByField("_id", userId);
        string errMsg;
        if (customer == null)
        {
            _logger.LogWarning("User {@userId} not found", userId);
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

            customer.UserPass.IsLocked = false;
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, customer.FirstName.Trim() + " " + customer.LastName.Trim()),
            new(ClaimTypes.Role, customer.Role.ToString()),
            new("userId", customer.Id.ToString())
        };

        var identity = new ClaimsIdentity(claims, "password");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        return await Task.FromResult(
            AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, Scheme.Name)));
    }

    private static bool UserPassCheck(UserPassCredential credential, string? username, string? password,
        out string error)
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