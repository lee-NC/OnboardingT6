using CompanyService.Entity.Api.Entities;
using Demo.Common.DBBase.Context;

namespace CompanyService.Entity.Api.Repositories;

public interface ICompanyEntityRepository : IMongoDbRepositoryBase<CompanyEntity>
{
    Task<CompanyEntity> FindByField(string fieldName, string value);
}