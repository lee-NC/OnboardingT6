using Demo.Services.UserService.Entity.Api.Entities;
using Demo.Services.UserService.Entity.Api.Repositories;
using MongoDB.Bson;

namespace UserService.Store;

public interface IUserEntityStore
{
    Task<UserEntity> FindByField(string fieldName, string value);

    Task<List<UserEntity>> GetAllByCompanyId(string? companyId);

    Task<UserEntity> FindByUsername(string value);
    Task Create(UserEntity entity);

    Task Delete(string toString);

    Task Update(UserEntity entity, ObjectId entityId);
    Task<List<UserEntity>> GetAll();
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

    public async Task<List<UserEntity>> GetAllByCompanyId(string? companyId)
    {
        return await _userRepo.GetAllByCompanyId(companyId);
    }

    public async Task<UserEntity> FindByUsername(string value)
    {
        return await _userRepo.FindByUsername(value);
    }

    public async Task Create(UserEntity entity)
    {
        await _userRepo.Create(entity);
    }

    public async Task Delete(string id)
    {
        _userRepo.Delete(id);
        return;
    }

    public async Task Update(UserEntity entity, ObjectId entityId)
    {
        await _userRepo.Update(entity, entityId);
    }

    public async Task<List<UserEntity>> GetAll()
    {
        return await _userRepo.GetAll() as List<UserEntity>;
    }
}