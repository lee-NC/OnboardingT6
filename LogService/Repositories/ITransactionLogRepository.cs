using Demo.Common.DBBase.Context;
using LogService.Entities;

namespace LogService.Repositories;

public interface ITransactionLogRepository : IMongoDbRepositoryBase<TransactionLog>
{
    Task<(int TotalPages, long TotalCount, IReadOnlyList<TransactionLog> Items)> FindAllWaitingTrans(
        string identityId,
        int page,
        int pageSize);

    Task<(int TotalPages, long TotalCount, IReadOnlyList<TransactionLog> Items)> FindAllByStatus(
        string customerId,
        int status,
        int page,
        int pageSize);

    Task<(int TotalPages, long TotalCount, IReadOnlyList<TransactionLog> Items)> FindAllByStatus(
        string customerId,
        int status,
        int page,
        int pageSize,
        string orderBy,
        bool isDesc);

    Task<(int TotalPages, long TotalCount, IReadOnlyList<TransactionLog> Items)> FindAllByStatus(
        string customerId,
        int status,
        DateTime startDate,
        DateTime endDate,
        string orderBy,
        bool isDesc);

    Task<(int TotalPages, long TotalCount, IReadOnlyList<TransactionLog> Items)>
        FindAll(string customerId, int status, int page, int pageSize, string orderBy, bool isDesc, DateTime startDate,
            DateTime endDate);

    Task<(int TotalPages, long TotalCount, IReadOnlyList<TransactionLog> Items)>
        FindAllBySearch(string textSearch, int status, int page, int pageSize, string orderBy, bool isDesc,
            DateTime startDate, DateTime endDate);

    Task<List<TransactionLog>> FindByExistFields(Dictionary<string, object> param);

    Task<(int PageIndex, int PageSize, int TotalPages, long TotalCount, IReadOnlyList<TransactionLog> Items)>
        FindByCustomerId(string customerId, int page, int pageSize);

    Task<long> CountTranByteDate(DateTime? fromDate, DateTime? endDate);
}