using Demo.Common.DBBase.Context;
using Demo.Services.UserService.Entity.Api.Entities;

namespace Demo.Services.UserService.Entity.Api.Repositories;

public interface IUserEntityRepository : IMongoDbRepositoryBase<UserEntity>
{
    Task<UserEntity> FindByField(string fieldName, string value);

    Task<List<UserEntity>> GetAllByCompanyId(string? companyId);

    Task<object> SignOn(string paramUsername, string paramPassword, string clientIpAddr);

    Task<UserEntity> FindByUsername(string value);

    Task<UserEntity> SignOut(string username);
}