using Demo.ApiGateway.DTOs;
using LogService.Entities;
using LogService.Repositories;

namespace AuthenService.Store;

public interface ITransactionStore
{
    Task SaveTransaction(TransactionUser cus,
        object? request);


    Task SaveTransaction(
        TransactionUser cus,
        byte[]? request,
        string failureReason);


    Task<TransactionLog> SaveAuthenticodeTransaction(
        TransactionUser cus,
        byte[]? tsResponse,
        byte[]? request,
        string failureReason);
}

public class TransactionStore : ITransactionStore
{
    private readonly ITransactionLogRepository _tranLogRepo;

    public TransactionStore(
        ITransactionLogRepository tranLogRepo)
    {
        _tranLogRepo = tranLogRepo;
    }

    public async Task SaveTransaction(TransactionUser cus, object? request)
    {
        var tran = new TransactionLog
        {
            User = cus,
            Request = request as byte[]
        };
        await _tranLogRepo.Create(tran);
    }

    public async Task SaveTransaction(
        TransactionUser cus,
        byte[]? request,
        string failureReason)
    {
        var tran = new TransactionLog
        {
            User = cus,
            Request = request
        };
        await _tranLogRepo.Create(tran);
    }

    public async Task<TransactionLog> SaveAuthenticodeTransaction(
        TransactionUser cus,
        byte[]? tsResponse,
        byte[]? request,
        string failureReason)
    {
        var tran = new TransactionLog
        {
            User = cus,
            Request = request,
            Response = tsResponse
        };
        await _tranLogRepo.Create(tran);

        return tran;
    }

    public async Task<TransactionLog> SaveTransaction(
        TransactionUser cus,
        DateTime reqTime,
        DateTime finishTime,
        string refTranId,
        byte[]? request)
    {
        TransactionLog tran = new()
        {
            User = cus,
            Request = request
        };
        await _tranLogRepo.Create(tran);

        return tran;
    }
}