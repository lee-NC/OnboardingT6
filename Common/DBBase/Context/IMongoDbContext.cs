using MongoDB.Driver;

namespace Demo.Common.DBBase.Context
{
    public interface IMongoDbContext
    {
        IMongoCollection<T> GetCollection<T>(string name);

        Task<IClientSessionHandle> StartSessionAsync();
    }
}
