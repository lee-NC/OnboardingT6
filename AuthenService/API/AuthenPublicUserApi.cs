using System.Diagnostics;
using System.Security.Claims;
using AuthenService.Store;
using Demo.ApiGateway.DTOs;
using Demo.ApiGateway.DTOs.Response;
using Demo.Services.AuthenService.Helper.ApiResponse;
using Demo.Services.UserService.Entity.Api.Entities;
using Demo.Services.UserService.Entity.Api.Helper;
using LogService.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Services.AuthenService.API;

public static class AuthenPublicUserApi
{
    public static RouteGroupBuilder MapPublicUserEndpoints(this RouteGroupBuilder groups)
    {
        groups.MapPost("sign-in", SignIn)
            .AddEndpointFilter(EndPointFilter);

        groups.MapGet("sign-out", SignOut)
            .RequireAuthorization("required_user_scheme")
            .AddEndpointFilter(EndPointFilter);

        return groups;
    }

    private static async Task<IResult> SignOut(ILogger<IUserEntityStore> logger,
        IUserEntityStore userEntityStore, ILogger<ITransactionStore> logger2,
        ITransactionStore transactionStore, HttpContext context)
    {
        var userId = context.User.Claims.FirstOrDefault(c => c.Type == "userId").Value;
        try
        {
            var userEntity = await userEntityStore.FindByField("_id", userId);
            if (userEntity == null)
            {
                transactionStore.SaveTransaction(
                    new TransactionUser() { Username = userEntity.UserPass.Username, Id = userEntity.Id.ToString() },
                    new byte[] { }, "Token invalid");
                return TypedResults.Ok(new ApiResponse
                (ResponseCode.SERVER_INTERNAL_ERROR, "Token invalid"
                ));
            }
                
            userEntityStore.SignOut(userEntity.UserPass.Username);
            transactionStore.SaveTransaction(
                new TransactionUser() { Username = userEntity.UserPass.Username, Id = userEntity.Id.ToString() },
                new byte[] { });

            return TypedResults.Ok(new ApiResponse(ResponseCode.SUCCESS, ""));
        }
        catch (Exception e)
        {
            logger.LogInformation(e.Message);
            logger2.LogInformation(e.Message);
            throw;
        }
    }

    private static async Task<IResult> SignIn(ILogger<IUserEntityStore> logger, ILogger<ITransactionStore> logger2,
        ITransactionStore transactionStore, IUserEntityStore userEntityStore, [FromBody] SignInRequest param,
        HttpRequest request, IConfiguration config)
    {
        try
        {
            var clientIpAddr = request.Headers["ClientIP"].ToString() ?? "127.0.0.1";
            var userEntity = await userEntityStore.SignOn(param.Username, param.Password, clientIpAddr);
            if (userEntity == null)
            {
                transactionStore.SaveTransaction(
                    new TransactionUser() { },
                    new byte[]{}, "Username or password invalid");
                return TypedResults.Ok(new ApiResponse(ResponseCode.CUSTOMER_NOT_FOUND, "Username or password invalid"
                ));
            }

            var entity = userEntity as UserEntity;

            var customerRsp = TokenResponse.MapFromModel(entity.UserPass.Username, entity.Id.ToString(),
                GenerateToken.Generate(entity, config));
            ;
            transactionStore.SaveTransaction(
                new TransactionUser() { Username = entity.UserPass.Username, Id = entity.Id.ToString() },
                param);

            return TypedResults.Ok(new ApiResponse(ResponseCode.SUCCESS, "", customerRsp)
            );
        }
        catch (Exception e)
        {
            logger.LogInformation(e.Message);
            logger2.LogInformation(e.Message);
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