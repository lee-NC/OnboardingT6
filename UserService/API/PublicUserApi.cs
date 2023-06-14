using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Demo.ApiGateway.DTOs;
using Demo.ApiGateway.DTOs.Response;
using Demo.Common.Utils;
using Demo.Common.Utils.Crypto;
using Demo.Portal.Helper;
using Demo.Services.Entities;
using Demo.Services.UserService.Model;
using Demo.Services.UserService.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Services.UserService.Model.Response;

namespace Demo.Services.UserService.API;

public static class PublicUserApi
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

    private static async Task<IResult> AddUser(HttpContext context, [FromBody] AddUserRequest request,
        ILogger<IUserEntityRepository> _logger, IUserEntityRepository _userEntityRepository)
    {
        var rolesClaim = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role).Value;
        var username = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
        // _logger.LogInformation("Get list users by {@roleType}:  {@param}", rolesClaim, username);
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
            }

            // if (userEntity.Role == RoleEnum.COMPANY_ADMIN)
            // {
            entity.CompanyId = userEntity.CompanyId;
            entity.Role = RoleEnum.SELLER;
            // }
            // else
            // {
            //     entity.CompanyId = request.CompanyId;
            //     entity.Role = request.Role;
            // }

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

    private static async Task<IResult> SignOut(ILogger<IUserEntityRepository> _logger,
        IUserEntityRepository _userEntityRepository, HttpContext context)
    {
        var username = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
        try
        {
            var userEntity = await _userEntityRepository.FindByUsername(username.ToString());
            if (userEntity == null)
            {
                return TypedResults.Ok(new ApiResponse
                {
                    Code = (int)ResponseCode.INTERNAL_SERVICE_ERROR,
                    CodeDesc = ResponseCode.CLIENT_INPUT_INVALID.ToString(),
                    Message = "Token invalid"
                });
            }
            UserEntity entity = userEntity as UserEntity;
            entity.Status = UserEntity.Statuses.INACTIVE;
            entity.StatusDesc = UserEntity.Statuses.INACTIVE.ToString();
            _userEntityRepository.Update(entity, entity.Id);
            
            return TypedResults.Ok(new ApiResponse
            {
                Code = (int)ResponseCode.SUCCESS,
                CodeDesc = ResponseCode.SUCCESS.ToString(),
                Message = "",
                Content = ""
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private static async Task<IResult> SignIn(ILogger<IUserEntityRepository> _logger,
        IUserEntityRepository _userEntityRepository, [FromBody] SignInRequest param,
        HttpRequest request, IConfiguration _config)
    {
        var clientIPAddr = request.Headers["ClientIP"].ToString() ?? "127.0.0.1";
        var userEntity = await _userEntityRepository.SignOn(param.Username, param.Password, clientIPAddr);
        if (userEntity == null)
        {
            return TypedResults.Ok(new ApiResponse
            {
                Code = (int)ResponseCode.CUSTOMER_NOT_FOUND,
                CodeDesc = ResponseCode.CUSTOMER_NOT_FOUND.ToString(),
                Message = "Username or password invalid"
            });
        }

        UserEntity entity = userEntity as UserEntity;
        entity.Status = UserEntity.Statuses.ACTIVE;
        entity.StatusDesc = UserEntity.Statuses.ACTIVE.ToString();
        _userEntityRepository.Update(entity, entity.Id);

        var customerRsp = TokenResponse.MapFromModel(entity.UserPass.Username.ToString(), entity.Id.ToString(),
            Generate(entity, _config));
        ;

        return TypedResults.Ok(new ApiResponse
        {
            Code = (int)ResponseCode.SUCCESS,
            CodeDesc = ResponseCode.SUCCESS.ToString(),
            Message = "",
            Content = customerRsp
        });
    }
    
    private static string Generate(UserEntity user, IConfiguration _config)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserPass.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(_config["Jwt:Issuer"],
            _config["Jwt:Audience"],
            claims,
            expires: DateTime.Now.AddMinutes(15),
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
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