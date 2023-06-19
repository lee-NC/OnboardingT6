using Demo.Common.Utils;
using Demo.MessageQueue.Response;
using Demo.Services.UserService.Entity.Api.Entities;
using Demo.Workflow.MessageQueue.Request;
using MassTransit;
using UserService.Store;
using LogContext = Serilog.Context.LogContext;

namespace Demo.Services.Helper.Masstransit;

public class AuthenticationConsumer : IConsumer<AuthenRequest>
{
    private readonly ILogger<AuthenticationConsumer> _logger;
    private readonly IUserEntityStore _userEntityStore;

    public AuthenticationConsumer(
        IUserEntityStore userEntityStore,
        ILogger<AuthenticationConsumer> logger)
    {
        _userEntityStore = userEntityStore;
        _logger = logger;
    }

    /// <summary>
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task Consume(ConsumeContext<AuthenRequest> context)
    {
        var authType = "";
        string failureInfo;
        UserEntity userEntity;
        var authRequest = context.Message;
        LogContext.PushProperty("AuthenRequest", authRequest);
        if (authRequest is null)
        {
            _logger.LogWarning("No authen request input");
            await context.RespondAsync<AuthenResponse>(new
            {
                context.Message.RequestId,
                IsAuthenticated = false,
                FailureInfo = "No authen request input"
            });
        }

        if (authRequest!.UserPass is null)
        {
            _logger.LogWarning("Basic authorization info (username/password) is required");
            await context.RespondAsync<AuthenResponse>(new
            {
                context.Message.RequestId,
                IsAuthenticated = false,
                FailureInfo = "Basic authorization info (username/password) is required"
            });
        }

        userEntity = await _userEntityStore.FindByField(SignInEnum.USERNAME_FIELD, authRequest.UserPass!.Username);
        if (userEntity == null)
        {
            _logger.LogWarning("Customer not found");
            var res = new AuthenResponse
            {
                RequestId = context.Message.RequestId,
                IsAuthenticated = false,
                FailureInfo = $"No customer found match username {authRequest.UserPass!.Username}"
            };
            await context.RespondAsync(res);

            return;
        }

        if (userEntity.Status != UserEntity.Statuses.ACTIVE)
        {
            _logger.LogWarning("Customer status is {@CusStatus}", userEntity.Status);
            var res = new AuthenResponse
            {
                RequestId = context.Message.RequestId,
                IsAuthenticated = false,
                FailureInfo = $"Customer status is {userEntity.Status}"
            };
            await context.RespondAsync(res);

            return;
        }

        var clientId = userEntity.Id.ToString();
        // user pass check
        if (userEntity.UserPass != null && userEntity.UserPass.Status == BaseCredential.Statuses.ENABLE)
        {
            _logger.LogInformation("Authenticate with UserPass");
            authType += "Userpass";

            string? errMsg;
            //Tài khoản bị khóa do đăng nhập sai 5 lần liên tiếp
            if (userEntity.UserPass.IsLocking)
            {
                errMsg = "your_account_is_locked_try_again_later";
                var res = new AuthenResponse
                {
                    RequestId = context.Message.RequestId,
                    IsAuthenticated = false,
                    AuthType = authType,
                    FailureInfo = errMsg
                };
                await context.RespondAsync(res);
                return;
            }

            userEntity.UserPass.IsLocked = false;

            if (!UserPassCheck(userEntity.UserPass, authRequest.UserPass, clientId, out failureInfo))
            {
                var res = new AuthenResponse
                {
                    RequestId = context.Message.RequestId,
                    IsAuthenticated = false,
                    AuthType = authType,
                    FailureInfo = failureInfo
                };
                _logger.LogWarning("UserPass authenticate failed with reason {@FailureInfo}", failureInfo);
                if (userEntity.UserPass.AccessFailedCount >= 4)
                {
                    userEntity.UserPass.IsLocked = true;
                    userEntity.UserPass.LockedDate = DateTime.UtcNow;
                    userEntity.UserPass.AccessFailedCount = 0;
                    userEntity.Status = UserEntity.Statuses.SUSPENDED;
                    errMsg = "invalid_attempts_exceeded_your_account_has_been_locked";
                }
                else
                {
                    userEntity.UserPass.AccessFailedCount += 1;
                    errMsg = failureInfo;
                }

                await _userEntityStore.Update(userEntity, userEntity.Id);

                await context.RespondAsync(res);
                return;
            }

            // Rest số lần sai pass liên tiếp về 0
            if (userEntity.UserPass.AccessFailedCount > 0) userEntity.UserPass.AccessFailedCount = 0;
            await _userEntityStore.Update(userEntity, userEntity.Id);
        }

        //client ip check
        if (userEntity.ClientIP != null && userEntity.ClientIP.Status == BaseCredential.Statuses.ENABLE)
        {
            authType += "ClientIP";
            _logger.LogInformation("Authenticate with ClientIP");
            if (!ClientIpCheck(userEntity.ClientIP, authRequest.ClientIP!, clientId, out failureInfo))
            {
                var res = new AuthenResponse
                {
                    RequestId = context.Message.RequestId,
                    IsAuthenticated = false,
                    AuthType = authType,
                    FailureInfo = failureInfo
                };
                _logger.LogWarning("ClientIP authenticate failed with reason {@FailureInfo}", failureInfo);
                await context.RespondAsync(res);
                return;
            }
        }

        _logger.LogInformation("Authenticate success");
        await context.RespondAsync<AuthenResponse>(new
        {
            context.Message.RequestId,
            IsAuthenticated = true,
            AuthType = authType,
            CustomerId = userEntity.Id.ToString(),
            CustomerName = userEntity.FirstName.Trim() + " " + userEntity.LastName.Trim()
        });
    }

    private bool ClientIpCheck(ClientIpCredential credential, ClientIP param, string? clientId, out string error)
    {
        error = "";
        if (param == null)
        {
            _logger.LogError("Client IP authen error: Param not found. ClientId: {@p0}", clientId);
            error = "Client IP authen error: Param not found.";
            return false;
        }

        try
        {
            if (param.IP != credential.IP)
            {
                _logger.LogError("Client IP Authen Error: IP not match. ClientId: {@p0}", clientId);
                error = "Client IP authen error: IP not match.";
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Client IP Authen Error. ClientId: {@p0}", clientId);
            error = "Client IP authen error: Internal error.";
            return false;
        }

        return true;
    }

    private bool UserPassCheck(UserPassCredential credential, UserPass param, string? clientId, out string error)
    {
        _logger.LogInformation("Userpass check {@p}", credential.Username);
        error = "";
        if (param == null)
        {
            _logger.LogError("UserPass Authen Error: Param not found. ClientId: {@p0}", clientId);
            error = "UserPass Authen Error: Param not found.";
            return false;
        }

        if (credential.Username != param.Username)
        {
            _logger.LogError("UserPass Authen Error: User not match. ClientId {@p0}", clientId);
            error = "UserPass Authen Error: User not match.";

            return false;
        }

        var result = EntityUtils.VerifyPassword(
            credential.PasswordHash, param.Password, credential.PasswordSalt);
        if (!result)
        {
            _logger.LogError("UserPass Authen Error: Password not match. ClientId {@p0}", clientId);
            error = "UserPass Authen Error: Password not match.";
            return false;
        }

        return true;
    }
}