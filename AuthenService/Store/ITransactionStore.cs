using LogService.Entities;
using LogService.Repositories;

namespace AuthenService.Store;

public interface ITransactionStore
{

    Task<TransactionLog> SaveTransaction(
        TransactionCustomer cus,
        byte[]? request);


    Task SaveTransaction(
        TransactionCustomer cus,
        byte[]? request,
        string failureReason);


    Task<TransactionLog> SaveAuthenticodeTransaction(
        TransactionCustomer cus,
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
        
        public async Task<TransactionLog> SaveTransaction(
            TransactionCustomer cus,
            DateTime reqTime,
            DateTime finishTime,
            string refTranId,
            byte[]? request)
        {
            TransactionLog tran = new()
            {
                Customer = cus,
                Request = request,
            };
            await _tranLogRepo.Create(tran);

            return tran;
        }

        public Task<TransactionLog> SaveTransaction(TransactionCustomer cus, byte[]? request)
        {
            throw new NotImplementedException();
        }

        public async Task SaveTransaction(
            TransactionCustomer cus,
            byte[]? request,
            string failureReason)
        {
            var tran = new TransactionLog
            {
                Customer = cus,
                Request = request
            };
            await _tranLogRepo.Create(tran);
        }
        
        public async Task<TransactionLog> SaveAuthenticodeTransaction(
            TransactionCustomer cus,
            byte[]? tsResponse,
            byte[]? request,
            string failureReason)
        {
            var tran = new TransactionLog
            {
                Customer = cus,
                Request = request,
                Response = tsResponse
            };
            await _tranLogRepo.Create(tran);

            return tran;
        }
    }