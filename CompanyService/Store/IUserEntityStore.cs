using Demo.Services.UserService.Entity.Api.Entities;
using Demo.Services.UserService.Entity.Api.Repositories;

namespace CompanyService.Store;

public interface IUserEntityStore
{
    Task<UserEntity> FindByField(string fieldName, string value);
}
public class UserEntityStore : IUserEntityStore
{
    private readonly IUserEntityRepository _userRepo;

    public UserEntityStore(IUserEntityRepository userRepo)
    {
        _userRepo = userRepo;
    }
    
    public async Task<UserEntity> FindByField(string fieldName, string value)
    {
        return await _userRepo.FindByField(fieldName, value);
    }
}