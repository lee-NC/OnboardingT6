using Demo.ApiGateway.DTOs;
using Demo.Portal.Helper;
using Microsoft.AspNetCore.Mvc;
using RestSharp;

namespace Demo.Portal.Controllers;

[Route("api/v1/user/")]
public class UserController : BaseController
{
    private static string _apiSubName = string.Empty;
    private static string _urlPreviewDocument = string.Empty;
    private readonly ICoreClient _coreClient;

    private readonly ILogger<UserController> _log;
    private readonly IConfiguration Configuration;

    public UserController(
        ILogger<UserController> logger,
        ICoreClient coreClient, IConfiguration configuration)
    {
        _log = logger;
        _coreClient = coreClient;
        Configuration = configuration;
        _apiSubName = Configuration.GetSection("AppSettings")["APISubName"];
        _urlPreviewDocument = Configuration.GetSection("AppSettings")["urlPreviewdocument"];
    }

    [Route("")]
    [HttpGet]
    [ServiceFilter(typeof(SessionFilter))]
    public async Task<IActionResult> GetAll()
    {
        var resp = await SendToServices("/api/v1/user/all", new { }, Method.GET);
        return Ok(resp);
    }

    [Route("info")]
    [HttpGet]
    [ServiceFilter(typeof(SessionFilter))]
    public async Task<IActionResult> GetUserInfo()
    {
        var resp = await SendToServices("/api/v1/user/info", new { }, Method.GET);
        return Ok(resp);
    }

    [Route("")]
    [HttpPost]
    [ServiceFilter(typeof(SessionFilter))]
    public async Task<IActionResult> AddUser(AddUserRequest request)
    {
        try
        {
            var resp = await SendToServices("/api/v1/user/add", request, Method.POST);
            if (resp != null && resp.Code == 0)
                return Ok(new
                {
                    code = 0,
                    Content = string.Empty
                });
        }
        catch (Exception ex)
        {
            _log.LogError(ex.Message, ex);
        }

        return Ok(new
        {
            code = -1,
            Content = string.Empty
        });
    }

    [Route("{id}")]
    [HttpGet]
    [ServiceFilter(typeof(SessionFilter))]
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            var resp = await SendToServices("/api/v1/user/{id}", new { id }, Method.GET);
            if (resp != null && resp.Code == 0)
                return Ok(new
                {
                    code = 0,
                    Content = string.Empty
                });
        }
        catch (Exception ex)
        {
            _log.LogError(ex.Message, ex);
        }

        return Ok(new
        {
            code = -1,
            Content = string.Empty
        });
    }

    [Route("")]
    [HttpPut]
    [ServiceFilter(typeof(SessionFilter))]
    public async Task<IActionResult> UpdateUserInfo(UpdateUserRequest request)
    {
        try
        {
            var resp = await SendToServices("/api/v1/user/update", request, Method.PUT);
            if (resp != null && resp.Code == 0)
                return Ok(new
                {
                    code = 0,
                    Content = string.Empty
                });
        }
        catch (Exception ex)
        {
            _log.LogError(ex.Message, ex);
        }

        return Ok(new
        {
            code = -1,
            Content = string.Empty
        });
    }


    [Route("{id}")]
    [HttpDelete]
    [ServiceFilter(typeof(SessionFilter))]
    public async Task<IActionResult> RemoveUser(string id)
    {
        try
        {
            var resp = await SendToServices("/api/v1/user/{id}", new { id }, Method.DELETE);
            if (resp != null && resp.Code == 0)
                return Ok(new
                {
                    code = 0,
                    Content = string.Empty
                });
        }
        catch (Exception ex)
        {
            _log.LogError(ex.Message, ex);
        }

        return Ok(new
        {
            code = -1,
            Content = string.Empty
        });
    }

    private async Task<ApiRespoinse> SendToServices(string path, object request, Method method)
    {
        var accessToken = Request.Headers["Authorization"].ToString();
        return _coreClient.Query(accessToken, path, request, out var mesg, method);
    }
}