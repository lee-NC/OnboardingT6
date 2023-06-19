using Demo.ApiGateway.DTOs;
using Demo.Portal.Helper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using RestSharp;

namespace Demo.Portal.Controllers;

[Route("api/v1/company/")]
public class CompanyController : BaseController
{
    private readonly ICoreClient _coreClient;
    private readonly ILogger<CompanyController> _log;

    public CompanyController(
        ILogger<CompanyController> logger,
        ICoreClient coreClient
    )
    {
        _log = logger;
        _coreClient = coreClient;
    }

    [Route("")]
    [HttpGet]
    [ServiceFilter(typeof(SessionFilter))]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var resp = await SendToServices("/api/v1/company/", new { }, Method.GET);
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
            var resp = await SendToServices("/api/v1/company/", new { id }, Method.GET);
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
    [HttpPost]
    [ServiceFilter(typeof(SessionFilter))]
    public async Task<IActionResult> AddCompany(AddCompanyRequest request)
    {
        try
        {
            // continue business logic
            var resp = await SendToServices("/api/v1/company", request, Method.POST);
            if (resp != null && resp.Code == 0)
                return Ok(new
                {
                    code = resp.Code,
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
    public async Task<IActionResult> RemoveCompany(string id)
    {
        try
        {
            var resp = await SendToServices("/api/v1/company", new { id }, Method.DELETE);
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
    public async Task<IActionResult> UpdateCompany(UpdateCompanyRequest request)
    {
        try
        {
            var resp = await SendToServices("/api/v1/company", request, Method.PUT);
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
        var accessToken = await HttpContext.GetTokenAsync("access_token");
        return _coreClient.Query(accessToken, path, request, out var mesg, method);
    }
}