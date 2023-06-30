using System.Diagnostics;
using System.Security.Claims;
using CompanyService.Entity.Api.Entities;
using Demo.Services.UserService.Entity.Api.Entities;
using Demo.Services.UserService.Store;
using Demo.ApiGateway.DTOs;
using Demo.Common.Utils;
using Demo.Portal.Helper;
using Microsoft.AspNetCore.Mvc;
using Demo.Services.UserService.Model.Response;

namespace Demo.Services.UserService.API;

public static class CompanyApi
{
    public static RouteGroupBuilder MapCompanyEndpoints(this RouteGroupBuilder groups)
    {
        groups.MapPost("", AddCompany)
            .RequireAuthorization("manager")
            .AddEndpointFilter(EndPointFilter);

        groups.MapPut("", UpdateCompany)
            .RequireAuthorization("admin")
            .AddEndpointFilter(EndPointFilter);

        groups.MapDelete("", DeleteCompany)
            .RequireAuthorization("manager")
            .AddEndpointFilter(EndPointFilter);

        groups.MapGet("", GetAllCompany)
            .RequireAuthorization("manager")
            .AddEndpointFilter(EndPointFilter);

        groups.MapGet("", GetCompanyById)
            .RequireAuthorization("manager")
            .AddEndpointFilter(EndPointFilter);

        return groups;
    }

    private static async Task<IResult> GetCompanyById(HttpContext context, string id,ILogger<ICompanyEntityStore> logger,
        ICompanyEntityStore companyEntityStore)
    {
        try
        {
            var comEntity = companyEntityStore.FindByField("_id", id);
            if (comEntity is null)
            {
                return TypedResults.Ok(new ApiClient.ApiResponse
                {
                    Code = (int)ResponseCode.COMPANY_NOT_FOUND,
                    CodeDesc = ResponseCode.COMPANY_NOT_FOUND.ToString(),
                    Message = ResponseCode.COMPANY_NOT_FOUND.ToString(),
                    Content = "Khong tim thay cong ty"
                });
            }
            return TypedResults.Ok(new ApiClient.ApiResponse
            {
                Code = (int)ResponseCode.SUCCESS,
                CodeDesc = ResponseCode.SUCCESS.ToString(),
                Message = ResponseCode.SUCCESS.ToString(),
                Content = comEntity
            });
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            throw;
        }
    }

    private static async Task<IResult> GetAllCompany(HttpContext context,ILogger<ICompanyEntityStore> logger,
        ICompanyEntityStore companyEntityStore)
    {
        try
        {
            var comEntity = companyEntityStore.GetAll();
            if (comEntity is null)
            {
                return TypedResults.Ok(new ApiClient.ApiResponse
                {
                    Code = (int)ResponseCode.COMPANY_NOT_FOUND,
                    CodeDesc = ResponseCode.COMPANY_NOT_FOUND.ToString(),
                    Message = ResponseCode.COMPANY_NOT_FOUND.ToString(),
                    Content = "Khong tim thay du lieu"
                });
            }
            return TypedResults.Ok(new ApiClient.ApiResponse
            {
                Code = (int)ResponseCode.SUCCESS,
                CodeDesc = ResponseCode.SUCCESS.ToString(),
                Message = ResponseCode.SUCCESS.ToString(),
                Content = comEntity
            });
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            throw;
        }
    }

    private static async Task<IResult> DeleteCompany(HttpContext context,string id, ILogger<ICompanyEntityStore> logger,
        ICompanyEntityStore companyEntityStore)
    {
        try
        {
            var comEntity = companyEntityStore.FindByField("_id", id.Trim());
            if (comEntity is null)
            {
                return TypedResults.Ok(new ApiClient.ApiResponse
                {
                    Code = (int)ResponseCode.COMPANY_NOT_FOUND,
                    CodeDesc = ResponseCode.COMPANY_NOT_FOUND.ToString(),
                    Message = ResponseCode.COMPANY_NOT_FOUND.ToString(),
                    Content = "Khong tim thay cong ty"
                });
            }

            await companyEntityStore.Delete(id.Trim());
            return TypedResults.Ok(new ApiClient.ApiResponse
            {
                Code = (int)ResponseCode.SUCCESS,
                CodeDesc = ResponseCode.SUCCESS.ToString(),
                Message = ResponseCode.SUCCESS.ToString(),
                Content = comEntity
            });
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            throw;
        }
    }

    private static async Task<IResult> UpdateCompany(HttpContext context, UpdateCompanyRequest request, ILogger<ICompanyEntityStore> logger,
        ICompanyEntityStore companyEntityStore)
    {
        try
        {
            var comEntity = companyEntityStore.FindByField("_id", request.Id.Trim());
            if (comEntity is null)
            {
                return TypedResults.Ok(new ApiClient.ApiResponse
                {
                    Code = (int)ResponseCode.COMPANY_NOT_FOUND,
                    CodeDesc = ResponseCode.COMPANY_NOT_FOUND.ToString(),
                    Message = ResponseCode.COMPANY_NOT_FOUND.ToString(),
                    Content = "Khong tim thay cong ty"
                });
            }

            var entity = comEntity.Result;
            if (string.IsNullOrWhiteSpace(request.Description))
            {
                entity.Description = request.Description;
            }
            if (request.BusinessAreas != null)
            {
                entity.BusinessAreas = request.BusinessAreas;
            }

            await companyEntityStore.Update(entity);
            return TypedResults.Ok(new ApiClient.ApiResponse
            {
                Code = (int)ResponseCode.SUCCESS,
                CodeDesc = ResponseCode.SUCCESS.ToString(),
                Message = ResponseCode.SUCCESS.ToString(),
                Content = comEntity
            });
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            throw;
        }
    }

    private static async Task<IResult> AddCompany(HttpContext context, [FromBody] AddCompanyRequest request,
        ILogger<ICompanyEntityStore> logger,
        ICompanyEntityStore companyEntityStore)
    {
        try
        {
            var entity = new CompanyEntity
            {
                Name = request.Name,
                Description = request.Description,
                BusinessAreas = request.BusinessAreas
            };

            if (request.Name != null && !string.IsNullOrWhiteSpace(request.Name))
            {
                var existCus = await companyEntityStore
                    .FindByField("name", request.Name);
                if (existCus != null)
                {
                    logger.LogWarning("CreateCompany: Exist name {@p0}", request.Name);
                    return TypedResults.Ok(new ApiClient.ApiResponse
                    {
                        Code = (int)ResponseCode.CLIENT_INPUT_INVALID,
                        CodeDesc = ResponseCode.CLIENT_INPUT_INVALID.ToString(),
                        Message = "Tồn tại công ty với tên này"
                    });
                }
            }

            await companyEntityStore.Create(entity);
            return TypedResults.Ok(new ApiClient.ApiResponse
            {
                Code = (int)ResponseCode.SUCCESS,
                CodeDesc = ResponseCode.SUCCESS.ToString(),
                Message = ResponseCode.SUCCESS.ToString(),
                Content = ""
            });
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            throw;
        }
    }

    public static async ValueTask<object?> EndPointFilter(EndpointFilterInvocationContext efiContext,
        EndpointFilterDelegate next)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = await next(efiContext);
        stopwatch.Stop();
        var elapsed = stopwatch.ElapsedMilliseconds;
        var response = efiContext.HttpContext.Response;
        response.Headers.Add("X-Response-Time", $"{elapsed} milliseconds");
        return result;
    }
}