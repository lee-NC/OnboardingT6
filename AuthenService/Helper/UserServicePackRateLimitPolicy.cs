using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using StackExchange.Redis;
using System.Globalization;
using System.Threading.RateLimiting;
using RedisRateLimiting;

namespace Demo.Services.AuthenService.Helper
{
    public class UserServicePackRateLimitPolicy : IRateLimiterPolicy<string>
    {
        private readonly Func<OnRejectedContext, CancellationToken, ValueTask>? _onRejected;
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<UserServicePackRateLimitPolicy> _logger;

        private static readonly string DetailFormat =
            "The user has sent too many requests in a given amount of time (\"rate limiting\")." +
            " The maximum allowed is {0} requests per {1}. Please retry again after {2}.";

        public const string Name = "servicepack_policy";


        public UserServicePackRateLimitPolicy(
            IConnectionMultiplexer connectionMultiplexer,
            IServiceProvider serviceProvider,
            ILogger<UserServicePackRateLimitPolicy> logger)
        {
            _serviceProvider = serviceProvider;
            _connectionMultiplexer = connectionMultiplexer;
            _logger = logger;
            _onRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.RequestServices.GetService<ILoggerFactory>()?
                    .CreateLogger("Microsoft.AspNetCore.RateLimitingMiddleware")
                    .LogWarning("OnRejected: {GetUserEndPoint}", context.HttpContext.Request.Path);
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    var retryAfterSecs = ((int)retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);
                    context.HttpContext.Response.Headers.RetryAfter = retryAfterSecs;
                    await context.HttpContext.Response.WriteAsJsonAsync<ProblemDetails>(new ProblemDetails
                    {
                        Status = StatusCodes.Status429TooManyRequests,
                        Type = "https://httpstatuses.com/429",
                        Title = "Too Many Requests",
                        Detail = string.Format(DetailFormat, 1, 60, retryAfterSecs),
                        Instance = context.HttpContext.Request.Path
                    }, cancellationToken: token);
                }
                else
                {
                    await context.HttpContext.Response.WriteAsJsonAsync<ProblemDetails>(new ProblemDetails
                    {
                        Status = StatusCodes.Status429TooManyRequests,
                        Type = "https://httpstatuses.com/429",
                        Title = "Too Many Requests",
                        Detail = "Too many requests. Please try again later. " +
                                 "Read more about our rate limits at https://example.org/docs/ratelimiting.",
                        Instance = context.HttpContext.Request.Path
                    }, cancellationToken: token);
                }
            };
        }

        public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected
        {
            get => _onRejected;
        }

        public RateLimitPartition<string> GetPartition(HttpContext httpContext)
        {
            var servicePackName = "Default service pack";
            if (httpContext.User != null && httpContext.User.Claims.Where(c => c.Type == "ServicePack")
                    .Select(c => c.Value).SingleOrDefault() != null)
            {
                servicePackName = httpContext.User.Claims.Where(c => c.Type == "ServicePack").Select(c => c.Value)
                    .SingleOrDefault();
            }

            // TODO: Get ratelimit options for servicePackName from database...
            // TODO: Cache first, hit every request

            return RedisRateLimitPartition.GetTokenBucketRateLimiter(servicePackName, key =>
                new RedisTokenBucketRateLimiterOptions
                {
                    TokenLimit = 5,
                    TokensPerPeriod = 2,
                    ReplenishmentPeriod = TimeSpan.FromSeconds(5),
                    ConnectionMultiplexerFactory = () => _connectionMultiplexer,
                })!;
        }
    }
}