using Demo.Services.UserService.Entity.Api.Entities;
using Demo.Services.UserService.Entity.Api.Repositories;

namespace AuthenService.Store;

public interface IUserEntityStore
{
    Task<object> SignOn(string paramUsername, string paramPassword, string clientIpAddr);

    Task<UserEntity> FindByUsername(string value);
    
    Task<UserEntity> FindByField(string fieldName, string value);

    Task SignOut(string username);
}

public class UserEntityStore : IUserEntityStore
{
    private readonly IUserEntityRepository _userRepo;

    public UserEntityStore(IUserEntityRepository userRepo)
    {
        _userRepo = userRepo;
    }

    public async Task<object> SignOn(string username, string password, string ip)
    {
        return await _userRepo.SignOn(username, password, ip);
    }

    public async Task<UserEntity> FindByUsername(string value)
    {
        return await _userRepo.FindByUsername(value);
    }

    public async Task<UserEntity> FindByField(string fieldName, string value)
    {
        return await _userRepo.FindByField(fieldName, value);
    }

    public async Task SignOut(string username)
    {
        await _userRepo.SignOut(username);
    }
}