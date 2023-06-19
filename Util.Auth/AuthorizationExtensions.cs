using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using static Demo.Common.Utils.RoleEnum;

namespace Services.Util.Auth;

public static class AuthorizationExtensions
{
    public static void AddAuthenticationScheme<T>(this IServiceCollection services, IConfiguration conf)
        where T : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        HttpClientHandler handler = new()
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        services.AddAuthentication()
            .AddScheme<AuthenticationSchemeOptions, T>("admin_scheme", o => { })
            .AddScheme<AuthenticationSchemeOptions, T>("user_scheme", o => { });

        // Register authorization policy with authentication scheme registered before
        services.AddAuthorization(options =>
        {
            var allPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddAuthenticationSchemes("admin_scheme", "user_scheme")
                .Build();

            options.AddPolicy("required_user_scheme",
                policy =>
                {
                    policy.RequireAuthenticatedUser().AddAuthenticationSchemes("user_scheme")
                        .RequireAssertion(context =>
                        {
                            var rolesClaim = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
                            if (rolesClaim is null) return false;
                            var roleString = rolesClaim.Value;
                            if (string.IsNullOrEmpty(roleString)) return false;

                            var roleList = roleString.Split(",");
                            return roleList.Contains(ADMIN.ToString()) ||
                                   roleList.Contains(COMPANY_ADMIN.ToString()) ||
                                   roleList.Contains(COMPANY_ADMIN.ToString());
                        });
                });

            options.AddPolicy("admin", policy =>
                policy
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes("admin_scheme")
                    .RequireAssertion(context =>
                    {
                        var rolesClaim = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
                        if (rolesClaim is null) return false;
                        var roleString = rolesClaim.Value;
                        if (string.IsNullOrEmpty(roleString)) return false;

                        var roleList = roleString.Split(",");
                        return roleList.Contains(ADMIN.ToString()) || roleList.Contains(COMPANY_ADMIN.ToString());
                    }));

            options.AddPolicy("manager", policy =>
                policy
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes("admin_scheme")
                    .RequireAssertion(context =>
                    {
                        var rolesClaim = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
                        if (rolesClaim is null) return false;
                        var roleString = rolesClaim.Value;
                        if (string.IsNullOrEmpty(roleString)) return false;

                        var roleList = roleString.Split(",");
                        return roleList.Contains(ADMIN.ToString());
                    }));
            options.AddPolicy("all", allPolicy);

            // Default authorization policy
            options.DefaultPolicy = options.GetPolicy("all")!;
        });
    }
}