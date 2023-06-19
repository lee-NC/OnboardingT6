using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Demo.ApiGateway.DTOs;
using Demo.ApiGateway.DTOs.Response;
using Demo.Common.Utils;
using Demo.Portal.Helper;
using Demo.Portal.Helper.ErrorHanlde;
using Demo.Portal.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using RestSharp;

namespace Demo.Portal.Controllers;

[Route("api/v1/auth")]
public partial class AuthController : BaseController
{
    private static string _apiSubName = string.Empty;
    private static string _urlPreviewDocument = string.Empty;
    private readonly ICoreClient _coreClient;
    private readonly ILogger<AuthController> _log;
    private readonly IConfiguration Configuration;

    public AuthController(
        ILogger<AuthController> logger,
        ICoreClient coreClient, IConfiguration configuration)
    {
        _log = logger;
        _coreClient = coreClient;
        Configuration = configuration;
        _apiSubName = Configuration.GetSection("AppSettings")["APISubName"];
        _urlPreviewDocument = Configuration.GetSection("AppSettings")["urlPreviewdocument"];
    }


    [Route("sign-in")]
    [HttpPost]
    public async Task<IActionResult> SignIn(SignInRequest request)
    {
        if (request is null) return Json("Dữ liệu không hợp lệ".ToErrorAjaxResult());

        try
        {
            var resp = _coreClient.QueryNoToken<ApiResponse<TokenResponse>>("/api/v1/pub/sign-in", request,
                out var message);


            if (resp == null) return Json(message.ToErrorAjaxResult());

            if (resp.Code != 0) return Json(resp.Message?.ToErrorAjaxResult());

            var user = resp.Content;

            HttpContext.Session.SetString(SignInEnum.USERNAME, request.Username);
            HttpContext.Session.SetString(SignInEnum.PASSWORD, request.Password);

            if (user is null) return Json("No user found");

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Username),
                new(ClaimTypes.Role, user.UserId)
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                //AllowRefresh = <bool>,
                // Refreshing the authentication session should be allowed.

                //ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
                // The time at which the authentication ticket expires. A 
                // value set here overrides the ExpireTimeSpan option of 
                // CookieAuthenticationOptions set with AddCookie.

                //IsPersistent = true,
                // Whether the authentication session is persisted across 
                // multiple requests. When used with cookies, controls
                // whether the cookie's lifetime is absolute (matching the
                // lifetime of the authentication ticket) or session-based.

                //IssuedUtc = <DateTimeOffset>,
                // The time at which the authentication ticket was issued.

                //RedirectUri = <string>
                // The full path or absolute URI to be used as an http 
                // redirect response value.
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return Json(resp.Content?.Token.ToSuccessAjaxResult());
        }
        // catch (AccessTokenTimeoutException ex)
        // {
        //     throw ex;
        // }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, "Exception LoadUserInfo");
            return Json(ex.ToExeptionAjaxResult());
        }
    }

    [Route("sign-out")]
    [HttpGet]
    public async Task<IActionResult> SignOut()
    {
        var authorizationHeader = Request.Headers["Authorization"].ToString();
        var authHeaderRegex = MyRegex();
        var username = "";

        if (authHeaderRegex.IsMatch(authorizationHeader))
        {
            var authBase64 = authHeaderRegex.Replace(authorizationHeader, "$1");
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(authBase64).Claims;

            username = jsonToken.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
        }

        if (string.IsNullOrEmpty(username)) return Json("Dữ liệu không hợp lệ".ToErrorAjaxResult());

        var accessToken = authorizationHeader;
        try
        {
            var resp = _coreClient.Query(accessToken, "/api/v1/pub/sign-out", new { username }, out var message,
                Method.GET);
            if (resp == null) return Json(message.ToErrorAjaxResult());

            if (resp.Code != 0) return Json(resp.Message?.ToErrorAjaxResult());

            var user = resp.Content;

            if (user is null) return Json("No user found");

            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return Json(resp.Content?.ToSuccessAjaxResult());
        }
        catch (AccessTokenTimeoutException ex)
        {
            throw ex;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, "Exception LoadUserInfo");
            return Json(ex.ToExeptionAjaxResult());
        }
    }

    [GeneratedRegex("Bearer (.*)")]
    private static partial Regex MyRegex();
}