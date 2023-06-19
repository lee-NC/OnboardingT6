using MongoDB.Bson;
using MongoDB.Driver;

namespace Demo.Common.DBBase.Context;

public interface IMongoDbRepositoryBase<TEntity> where TEntity : class
{
    Task Create(TEntity obj);
    Task Update(TEntity obj, ObjectId id);
    void Delete(string id);
    Task<TEntity> FindAsync(string id);
    Task<TEntity> FindAsync(ObjectId id);
    Task<IEnumerable<TEntity>> GetAll();
    Task<IClientSessionHandle> BeginSessionAsync();
    Task CommitTransactionAsync(IClientSessionHandle session);
    Task AbortTransactionAsync(IClientSessionHandle session);
}