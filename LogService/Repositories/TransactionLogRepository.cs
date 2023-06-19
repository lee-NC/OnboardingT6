using Demo.Common.DBBase.Context;
using LogService.Entities;
using MongoDB.Driver;

namespace LogService.Repositories;

public class TransactionLogRepository : MongoDbRepositoryBase<TransactionLog>, ITransactionLogRepository
{
    public TransactionLogRepository(IMongoDbContext context) : base(context)
    {
    }

    public Task<(int TotalPages, long TotalCount, IReadOnlyList<TransactionLog> Items)> FindAllWaitingTrans(
        string identityId, int page, int pageSize)
    {
        throw new NotImplementedException();
    }

    public Task<(int TotalPages, long TotalCount, IReadOnlyList<TransactionLog> Items)> FindAllByStatus(
        string customerId, int status, int page, int pageSize)
    {
        throw new NotImplementedException();
    }

    public Task<(int TotalPages, long TotalCount, IReadOnlyList<TransactionLog> Items)> FindAllByStatus(
        string customerId, int status, int page, int pageSize, string orderBy, bool isDesc)
    {
        throw new NotImplementedException();
    }

    public Task<(int TotalPages, long TotalCount, IReadOnlyList<TransactionLog> Items)> FindAllByStatus(
        string customerId, int status, DateTime startDate, DateTime endDate, string orderBy, bool isDesc)
    {
        throw new NotImplementedException();
    }

    public Task<(int TotalPages, long TotalCount, IReadOnlyList<TransactionLog> Items)> FindAll(string customerId,
        int status, int page, int pageSize, string orderBy, bool isDesc, DateTime startDate,
        DateTime endDate)
    {
        throw new NotImplementedException();
    }

    public Task<(int TotalPages, long TotalCount, IReadOnlyList<TransactionLog> Items)> FindAllBySearch(
        string textSearch, int status, int page, int pageSize, string orderBy, bool isDesc,
        DateTime startDate, DateTime endDate)
    {
        throw new NotImplementedException();
    }

    public async Task<List<TransactionLog>> FindByExistFields(Dictionary<string, object> param)
    {
        if (param is null || param.Count < 1) return null;

        var key = param.Keys.First();

        var filter = Builders<TransactionLog>.Filter.Eq(key, param[key]);
        param.Remove(key);

        foreach (var k in param.Keys) filter |= Builders<TransactionLog>.Filter.Eq(k, param[k]);

        var f = Builders<TransactionLog>.Filter.And(filter);
        return await _dbCollection.FindAsync(f).Result.ToListAsync();
    }

    public Task<(int PageIndex, int PageSize, int TotalPages, long TotalCount, IReadOnlyList<TransactionLog> Items)>
        FindByCustomerId(string customerId, int page, int pageSize)
    {
        throw new NotImplementedException();
    }

    public Task<long> CountTranByteDate(DateTime? fromDate, DateTime? endDate)
    {
        throw new NotImplementedException();
    }
}