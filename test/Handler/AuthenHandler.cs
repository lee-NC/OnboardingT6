using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using test.Model;

namespace test.Handler;

public partial class AuthenHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private static readonly List<User> Users = new()
    {
        new User { Username = "admin", Password = "password", Role = "Administrator" },
        new User { Username = "seller", Password = "password", Role = "Seller" }
    };

    private readonly ILogger<AuthenHandler> _logger;

    public AuthenHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger,
        UrlEncoder encoder, ISystemClock clock, ILogger<AuthenHandler> logger1) : base(options, logger, encoder, clock)
    {
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
                // var authSplit = jsonToken.Split(Convert.ToChar(","), 2);
                // username = authSplit[0];
                // password = authSplit.Length > 1 ? authSplit[1] : null;
            }
        }
        else
        {
            Response.Headers["WWW-Authenticate"] = "Basic realm=\"Authentication required\", charset=\"UTF-8\"";
            return await Task.FromResult(AuthenticateResult.Fail(new Exception("Authenticate required")));
        }

        var customer = Users.FirstOrDefault(o =>
            o.Username.ToLower() == username.ToLower());
        string errMsg;
        if (customer == null)
        {
            _logger.LogWarning("User {@username} not found", username);
            errMsg = "user_not_found";
            Response.Headers["WWW-Authenticate"] = $"Basic realm=\"{errMsg}\", charset=\"UTF-8\"";
            return await Task.FromResult(AuthenticateResult.Fail(new Exception("Authenticate failed")));
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "" + customer.Username),
            new(ClaimTypes.Role, "" + customer.Role)
        };

        var identity = new ClaimsIdentity(claims, "password");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        return await Task.FromResult(
            AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, Scheme.Name)));
    }

    [GeneratedRegex("Bearer (.*)")]
    private static partial Regex MyRegex();
}