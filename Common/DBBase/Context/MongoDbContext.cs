using Demo.Common.DBBase.Config;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Demo.Common.DBBase.Context;

public class MongoDbContext : IMongoDbContext
{
    //public MongoDbContext(IMongoDbSettings setting)
    //{
    //    _mongoClient = new MongoClient(setting.ConnectionString);
    //    _db = _mongoClient.GetDatabase(setting.DatabaseName);

    //    // Implement class
    //    //Products = database.GetCollection<Product>(settings.CollectionName);
    //    //CatalogContextSeed.SeedData(Products);
    //}

    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        var setting = settings.Value;
        _mongoClient = new MongoClient(setting.ConnectionString);
        _db = _mongoClient.GetDatabase(setting.DatabaseName);
    }

    protected IMongoDatabase _db { get; set; }
    protected MongoClient _mongoClient { get; set; }
    protected IClientSessionHandle Session { get; set; }

    public IMongoCollection<T> GetCollection<T>(string name)
    {
        return _db.GetCollection<T>(name);
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    public async Task<IClientSessionHandle> StartSessionAsync()
    {
        return await _mongoClient.StartSessionAsync();
    }
}