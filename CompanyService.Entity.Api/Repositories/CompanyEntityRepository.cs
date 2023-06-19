using CompanyService.Entity.Api.Entities;
using Demo.Common.DBBase.Context;
using MongoDB.Driver;

namespace CompanyService.Entity.Api.Repositories;

public class CompanyEntityRepository : MongoDbRepositoryBase<CompanyEntity>, ICompanyEntityRepository
{

    public CompanyEntityRepository(IMongoDbContext context) : base(context)
    {
    }

    public async Task<CompanyEntity> FindByField(string fieldName, string value)
    {
        var filter = Builders<CompanyEntity>.Filter.Eq(fieldName, value);

        return await _dbCollection.FindAsync(filter).Result.FirstOrDefaultAsync();
    }
}