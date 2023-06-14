using System.Diagnostics;
using System.Security.Claims;
using Demo.ApiGateway.DTOs;
using Demo.Common.Utils;
using Demo.Common.Utils.Crypto;
using Demo.Portal.Helper;
using Demo.Services.Entities;
using Demo.Services.UserService.Model;
using Demo.Services.UserService.Repositories;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Services.UserService.Model.Response;

namespace Demo.Services.UserService.API;

public static class UserApi
{
    public static RouteGroupBuilder MapUserEndpoints(this RouteGroupBuilder groups)
    {
        groups.MapPost("add", AddUser)
            .RequireAuthorization("admin")
            .AddEndpointFilter(EndPointFilter);

        groups.MapPut("update", UpdateUser)
            .RequireAuthorization("required_user_scheme")
            .AddEndpointFilter(EndPointFilter);

        groups.MapDelete("", DeleteUser)
            .RequireAuthorization("admin")
            .AddEndpointFilter(EndPointFilter);

        groups.MapGet("all", GetAllUser)
            .RequireAuthorization("admin")
            .AddEndpointFilter(EndPointFilter);

        groups.MapGet("info", GetUserInfo)
            .RequireAuthorization("required_user_scheme")
            .AddEndpointFilter(EndPointFilter);

        groups.MapGet("", GetUserById)
            .RequireAuthorization("admin")
            .AddEndpointFilter(EndPointFilter);

        return groups;
    }

    private static async Task<IResult> DeleteUser(HttpContext context, string id,
        ILogger<IUserEntityRepository> _logger,
        IUserEntityRepository _userEntityRepository)
    {
        var rolesClaim = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role).Value;
        var username = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
        _logger.LogInformation("Delete user by {@roleType}:  {@param}", rolesClaim, username);
        try
        {
            UserEntity userEntity =
                await _userEntityRepository.FindByUsername(username.ToString());
            UserEntity entity =
                await _userEntityRepository.FindByField("_id", id.ToString());
            if (entity is null)
            {
                return TypedResults.Ok(new ApiClient.ApiResponse
                {
                    Code = (int)ResponseCode.CLIENT_INPUT_INVALID,
                    CodeDesc = ResponseCode.CLIENT_INPUT_INVALID.ToString(),
                    Message = ResponseCode.CLIENT_INPUT_INVALID.ToString(),
                    Content = "User not found"
                });
            }

            if (userEntity.Role == RoleEnum.COMPANY_ADMIN && !userEntity.CompanyId.Equals(entity.CompanyId))
            {
                return TypedResults.Ok(new ApiClient.ApiResponse
                {
                    Code = (int)ResponseCode.CLIENT_INPUT_INVALID,
                    CodeDesc = ResponseCode.CLIENT_INPUT_INVALID.ToString(),
                    Message = ResponseCode.CLIENT_INPUT_INVALID.ToString(),
                    Content = "Du lieu khong phu hop"
                });
            }

            _userEntityRepository.Delete(id.ToString());

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
            Console.WriteLine(e);
            throw;
        }
    }

    private static async Task<IResult> UpdateUser(HttpContext context, [FromBody] UpdateUserRequest request,
        ILogger<IUserEntityRepository> _logger,
        IUserEntityRepository _userEntityRepository)
    {
        var rolesClaim = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role).Value;
        var username = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
        _logger.LogInformation("Update user by {@roleType}:  {@param}", rolesClaim, username);
        UserEntity userEntity =
            await _userEntityRepository.FindByUsername(username.ToString());
        UserEntity entity =
            await _userEntityRepository.FindByField("_id", request.UserId);
        if (entity is null)
        {
            return TypedResults.Ok(new ApiClient.ApiResponse
            {
                Code = (int)ResponseCode.CLIENT_INPUT_INVALID,
                CodeDesc = ResponseCode.CLIENT_INPUT_INVALID.ToString(),
                Message = ResponseCode.CLIENT_INPUT_INVALID.ToString(),
                Content = "User not found"
            });
        }

        if ((userEntity.Role == RoleEnum.COMPANY_ADMIN && !userEntity.CompanyId.Equals(entity.CompanyId)) ||
            !userEntity.Id.Equals(entity.Id))
        {
            return TypedResults.Ok(new ApiClient.ApiResponse
            {
                Code = (int)ResponseCode.CLIENT_INPUT_INVALID,
                CodeDesc = ResponseCode.CLIENT_INPUT_INVALID.ToString(),
                Message = ResponseCode.CLIENT_INPUT_INVALID.ToString(),
                Content = "Du lieu khong phu hop"
            });
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            entity.Email = request.Email;
        }

        if (!string.IsNullOrWhiteSpace(request.FirstName))
        {
            entity.FirstName = request.FirstName;
        }

        if (!string.IsNullOrWhiteSpace(request.LastName))
        {
            entity.LastName = request.LastName;
        }

        if (request.Sex is not null)
        {
            entity.Sex = request.Sex;
        }

        if (request.DateOfBirth is not null)
        {
            entity.DateOfBirth = request.DateOfBirth;
        }

        _userEntityRepository.Update(entity, entity.Id);

        UserResponse response = UserResponse.MapFromModel(entity);
        return TypedResults.Ok(new ApiClient.ApiResponse
        {
            Code = (int)ResponseCode.SUCCESS,
            CodeDesc = ResponseCode.SUCCESS.ToString(),
            Message = ResponseCode.SUCCESS.ToString(),
            Content = response
        });
    }

    private static async Task<IResult> GetUserById(string id, HttpContext context,
        ILogger<IUserEntityRepository> _logger,
        IUserEntityRepository _userEntityRepository)
    {
        var rolesClaim = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role).Value;
        var username = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
        _logger.LogInformation("Get user by id by {@roleType}:  {@param}", rolesClaim, username);
        UserEntity userEntity =
            await _userEntityRepository.FindByUsername(username.ToString());
        UserEntity entity =
            await _userEntityRepository.FindByField("_id", id.ToString());
        if (entity is null)
        {
            return TypedResults.Ok(new ApiClient.ApiResponse
            {
                Code = (int)ResponseCode.CLIENT_INPUT_INVALID,
                CodeDesc = ResponseCode.CLIENT_INPUT_INVALID.ToString(),
                Message = ResponseCode.CLIENT_INPUT_INVALID.ToString(),
                Content = "User not found"
            });
        }

        if (userEntity.Role == RoleEnum.COMPANY_ADMIN && !userEntity.CompanyId.Equals(entity.CompanyId))
        {
            return TypedResults.Ok(new ApiClient.ApiResponse
            {
                Code = (int)ResponseCode.CLIENT_INPUT_INVALID,
                CodeDesc = ResponseCode.CLIENT_INPUT_INVALID.ToString(),
                Message = ResponseCode.CLIENT_INPUT_INVALID.ToString(),
                Content = "Du lieu khong phu hop"
            });
        }

        UserResponse response = UserResponse.MapFromModel(entity);
        return TypedResults.Ok(new ApiClient.ApiResponse
        {
            Code = (int)ResponseCode.SUCCESS,
            CodeDesc = ResponseCode.SUCCESS.ToString(),
            Message = ResponseCode.SUCCESS.ToString(),
            Content = response
        });
    }

    private static async Task<IResult> GetAllUser(HttpContext context, ILogger<IUserEntityRepository> _logger,
        IUserEntityRepository _userEntityRepository)
    {
        var rolesClaim = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role).Value;
        var username = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
        _logger.LogInformation("Get list users by {@roleType}:  {@param}", rolesClaim, username);
        UserEntity userEntity =
            await _userEntityRepository.FindByUsername(username.ToString());
        var items = new List<UserEntity>();
        if (userEntity.Role == RoleEnum.ADMIN)
        {
            items = await _userEntityRepository.GetAll() as List<UserEntity>;
        }
        else
        {
            var companyId = userEntity.CompanyId;
            items = await _userEntityRepository.GetAllByCompanyId(companyId) as List<UserEntity>;
        }

        IEnumerable<UserResponse> enDtos = items
            .Select(c => UserResponse.MapFromModel(c)).ToList();


        return TypedResults.Ok(new ApiClient.ApiResponse
        {
            Code = (int)ResponseCode.SUCCESS,
            CodeDesc = ResponseCode.SUCCESS.ToString(),
            Message = ResponseCode.SUCCESS.ToString(),
            Content = enDtos
        });
    }

    private static async Task<IResult> AddUser(HttpContext context, [FromBody] AddUserRequest request,
        ILogger<IUserEntityRepository> _logger, IUserEntityRepository _userEntityRepository)
    {
        var rolesClaim = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role).Value;
        var username = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
        _logger.LogInformation("Get add user by {@roleType}:  {@param}", rolesClaim, username);
        try
        {
            var userEntity =
                await _userEntityRepository.FindByUsername(username.ToString());

            var role = request.Role;
            var companyId = request.CompanyId;
            UserEntity entity = new UserEntity
            {
                UserPass = new UserPassCredential
                {
                    Username = request.Username,
                },
                Sex = request.Sex,
                LastName = request.LastName,
                FirstName = request.FirstName,
                DateOfBirth = request.DateOfBirth,
                Email = request.Email,
                Role = request.Role
            };
            if (request.Username != null && !string.IsNullOrWhiteSpace(request.Username))
            {
                var existCus = await _userEntityRepository
                    .FindByUsername(request.Username);
                if (existCus != null)
                {
                    _logger.LogWarning("CreateCustomer: Exist username {@p0}", request.Username);
                    return TypedResults.Ok(new ApiResponse
                    {
                        Code = (int)ResponseCode.CLIENT_INPUT_INVALID,
                        CodeDesc = ResponseCode.CLIENT_INPUT_INVALID.ToString(),
                        Message = "Tồn tại khách hàng với tên đăng nhập này"
                    });
                }

                var includeLowercase = true;
                var includeUppercase = true;
                var includeNumeric = true;
                var includeSpecial = true;
                var includeSpace = false;
                var password = request.Password;

                if (!PasswordGenerator.PasswordIsValid(includeLowercase, includeUppercase, includeNumeric,
                        includeSpecial, includeSpace, password) || password.Length < 8)
                {
                    _logger.LogWarning("Your password not match password policy");
                    return TypedResults.Ok(new ApiResponse
                    {
                        Code = (int)ResponseCode.CUSTOMER_NOT_FOUND,
                        CodeDesc = ResponseCode.CUSTOMER_NOT_FOUND.ToString(),
                        Message = "Mật khẩu phải có chữ hoa, chữ thường, số và ký tự đặc biệt và dài ít nhất 8 ký tự"
                    });
                }

                var passwordSalt = EntityUtils.GenerateRandomBytes(PasswordGenerator.PasswordSaltLength);
                var passwordHash = EntityUtils.GetInputPasswordHash(password, passwordSalt);

                var userPassCre = new UserPassCredential
                {
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    Username = request.Username,
                    Status = BaseCredential.Statuses.ENABLE,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt
                };
                entity.UserPass = userPassCre;
                entity.Status = UserEntity.Statuses.NEW;
                entity.StatusDesc = UserEntity.Statuses.NEW.ToString();
            }

            if (userEntity.Role == RoleEnum.COMPANY_ADMIN)
            {
                entity.CompanyId = userEntity.CompanyId;
                entity.Role = RoleEnum.SELLER;
            }
            else
            {
                entity.CompanyId = request.CompanyId;
                entity.Role = request.Role;
            }

            await _userEntityRepository.Create(entity);
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
            Console.WriteLine(e);
            throw;
        }
    }

    private static async Task<IResult> GetUserInfo(ILogger<IUserEntityRepository> _logger,
        IUserEntityRepository _userEntityRepository, HttpContext context)
    {
        var rolesClaim = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role).Value;
        var username = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
        try
        {
            var userEntity = await _userEntityRepository.FindByUsername(username.ToString());
            UserResponse response = UserResponse.MapFromModel(userEntity);
            return TypedResults.Ok(new ApiResponse
            {
                Code = (int)ResponseCode.SUCCESS,
                CodeDesc = ResponseCode.SUCCESS.ToString(),
                Message = "Thong tin nguoi dung",
                Content = response
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
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