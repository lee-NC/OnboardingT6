using System.Diagnostics;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using Demo.Common.Utils.Cache;
using Demo.MessageQueue.Response;
using Demo.Workflow.MessageQueue.Request;
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Demo.Services.AuthenService.Helper
{
    public partial class DemoAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly ILogger<DemoAuthenticationHandler> _logger;
        private readonly IRequestClient<AuthenRequest> _authClient;
        private readonly IDistributedCache _cache;
        private readonly bool _enableCache;
        private readonly bool _allowAnnonymous;

        public DemoAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IRequestClient<AuthenRequest> client,
            IDistributedCache cache,
            IConfiguration configuration)
            : base(options, logger, encoder, clock)
        {
            _logger = logger.CreateLogger<DemoAuthenticationHandler>();
            _authClient = client;
            _enableCache = "true".Equals(configuration["Cache:Enable"], StringComparison.OrdinalIgnoreCase);
            _cache = cache;
            _allowAnnonymous = "true".Equals(configuration["ServerConfig:AllowAnnonymous"],
                StringComparison.OrdinalIgnoreCase);
        }
        

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            AuthenRequest authRequest = new();
            var cachedKey = "";
            /**
             * If username/password exist
             * 
             **/
            if (Request.Headers.ContainsKey("Authorization"))
            {
                var authorizationHeader = Request.Headers["Authorization"].ToString();
                var authHeaderRegex = MyRegex();

                if (authHeaderRegex.IsMatch(authorizationHeader))
                {
                    var authBase64 =
                        Encoding.UTF8.GetString(
                            Convert.FromBase64String(authHeaderRegex.Replace(authorizationHeader, "$1")));
                    var authSplit = authBase64.Split(Convert.ToChar(":"), 2);
                    cachedKey = $"allow_" + authSplit[0];
                    authRequest.UserPass = new UserPass
                    {
                        Username = authSplit[0],
                        Password = authSplit.Length > 1 ? authSplit[1] : null
                    };
                }
            }
            else
            {
                Response.Headers["WWW-Authenticate"] = "Basic realm=\"Authentication required\", charset=\"UTF-8\"";
                return await Task.FromResult(AuthenticateResult.Fail(new Exception("Authenticate required")));
            }

            Serilog.Context.LogContext.PushProperty("AuthenticatedUsername", authRequest.UserPass.Username);

            AuthenResponse? authResp = null;

            if (_allowAnnonymous)
            {
                authResp = new AuthenResponse
                {
                    AuthType = "UserPass",
                    IsAuthenticated = true,
                    CustomerName = authRequest.UserPass?.Username,
                    CustomerId = authRequest.UserPass?.Username
                };
            }
            else
            {
                try
                {
                    var value = Convert.ToHexString(SHA1.HashData(JsonSerializer.SerializeToUtf8Bytes(authRequest)));
                    if (_enableCache)
                    {
                        authResp = await _cache.GetAsync<AuthenResponse>(cachedKey);
                    }

                    if (authResp == null)
                    {
                        authResp = _authClient.GetResponse<AuthenResponse>(authRequest!)?.Result?.Message;
                        if (_enableCache && authResp!.IsAuthenticated)
                        {
                            await _cache.SetAsync(cachedKey, authResp, TimeSpan.FromMinutes(30));
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Authenticate not success. Get exception {p0}", ex.Message);
                    Response.Headers["WWW-Authenticate"] = "Basic realm=\"Authentication failed\", charset=\"UTF-8\"";
                    return await Task.FromResult(AuthenticateResult.Fail(ex));
                }

                if (authResp is null)
                {
                    _logger.LogError("Authenticate not success. Get response NULL");
                    Response.Headers["WWW-Authenticate"] = "Basic realm=\"Authentication failed\", charset=\"UTF-8\"";
                    return await Task.FromResult(
                        AuthenticateResult.Fail(new Exception("Authenticate not success. Get response NULL")));
                }
            }

            if (authResp.IsAuthenticated)
            {
                var authenticatedUser = new AuthenticatedUser
                {
                    AuthenticationType = authResp.AuthType,
                    Name = authResp.CustomerName,
                    IsAuthenticated = authResp.IsAuthenticated,
                    UserId = authResp.CustomerId
                };
                if (string.IsNullOrEmpty(authenticatedUser.AuthenticationType))
                {
                    authenticatedUser.AuthenticationType = "UserPass";
                }
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, "" + authenticatedUser.Name),
                    new Claim("UserId", "" + authenticatedUser.UserId),
                    new Claim("AuthType", "" + authenticatedUser.AuthenticationType),
                    new Claim("ServicePack", "" + authenticatedUser.ServicePack)
                };

                var identity = new ClaimsIdentity(claims, authenticatedUser.AuthenticationType);
                var claimsPrincipal = new ClaimsPrincipal(identity);

                stopwatch.Stop();
                _logger.LogInformation("User successful authenticated in {@Elapsed:0.000} ms",
                    stopwatch.ElapsedMilliseconds);

                return await Task.FromResult(
                    AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, Scheme.Name)));
            }
            else
            {
                _logger.LogWarning("Authenticate fail, @{p0}", authResp.FailureInfo);
            }

            Response.Headers["WWW-Authenticate"] = $"Basic realm=\"{authResp.FailureInfo}\", charset=\"UTF-8\"";
            return await Task.FromResult(AuthenticateResult.Fail(new Exception("Authenticate failed")));
        }

        public class AuthenticatedUser : IIdentity
        {
            public AuthenticatedUser()
            {
            }

            public AuthenticatedUser(string authenticationType, bool isAuthenticated, string name)
            {
                AuthenticationType = authenticationType;
                IsAuthenticated = isAuthenticated;
                Name = name;
            }

            public AuthenticatedUser(AuthenResponse authResp)
            {
                AuthenticationType = authResp.AuthType;
                IsAuthenticated = authResp.IsAuthenticated;
                Name = authResp.CustomerName;
            }

            public string? AuthenticationType { get; set; }

            public bool IsAuthenticated { get; set; }

            public string? Name { get; set; }
            public string? UserId { get; set; }

            public string? ServicePack { get; set; }
        }

        [GeneratedRegex("Bearer (.*)")]
        private static partial Regex MyRegex();
    }
}