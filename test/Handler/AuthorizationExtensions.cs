using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using test.Policy;

namespace test.Handler;

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
            .AddScheme<AuthenticationSchemeOptions, T>("admin_scheme", _ => { })
            .AddScheme<AuthenticationSchemeOptions, T>("seller_scheme", _ => { });

        // Register authorization policy with authentication scheme registered before
        services.AddAuthorization(options =>
        {
            var allPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddAuthenticationSchemes("admin_scheme", "seller_scheme")
                .Build();

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
                        return roleList.Contains(Role.AdminClaimValue);
                    }));

            options.AddPolicy("seller", policy =>
                policy
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes("seller_scheme")
                    .RequireAssertion(context =>
                    {
                        var rolesClaim = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
                        if (rolesClaim is null) return false;
                        var roleString = rolesClaim.Value;
                        if (string.IsNullOrEmpty(roleString)) return false;

                        var roleList = roleString.Split(",");
                        return roleList.Contains(Role.SellerClaimValue);
                    }));
            options.AddPolicy("all", allPolicy);

            // Default authorization policy
            options.DefaultPolicy = options.GetPolicy("AllPolicies")!;
        });
    }
}