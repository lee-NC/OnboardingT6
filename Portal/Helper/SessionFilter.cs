using System.Net;
using System.Security.Claims;
using Demo.Common.Utils;
using Demo.Portal.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Demo.Portal.Helper;

public class SessionFilter : IAsyncActionFilter
{
    public SessionFilter(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    private IConfiguration Configuration { get; }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var username = context.HttpContext.Session.GetString(SignInEnum.USERNAME);
        var password = context.HttpContext.Session.GetString(SignInEnum.PASSWORD);
        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
        {
            var claims = new List<Claim>
            {
                new("username", username),
                new("password", password)
            };

            var appIdentity = new ClaimsIdentity(claims);
            context.HttpContext.User.AddIdentity(appIdentity);
            await next();

            return;
        }

        var method = context.HttpContext.Request.Method;
        if (method == "GET")
            context.Result = new RedirectToActionResult(nameof(AuthController.SignIn), "Auth", null);
        else
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
    }
}