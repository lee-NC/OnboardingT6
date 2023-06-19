using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Demo.Common.DBBase.Context;

/// <summary>
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public class MongoDbRepositoryBase<TEntity> : IMongoDbRepositoryBase<TEntity> where TEntity : class
{
    protected readonly IMongoDbContext _mongoContext;
    protected IMongoCollection<TEntity> _dbCollection;

    protected MongoDbRepositoryBase(IMongoDbContext context)
    {
        _mongoContext = context;
        _dbCollection = _mongoContext.GetCollection<TEntity>(typeof(TEntity).Name);
    }

    /// <summary>
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public async Task Create(TEntity obj)
    {
        if (obj == null) throw new ArgumentNullException(typeof(TEntity).Name + " object is null");
        await _dbCollection.InsertOneAsync(obj);
    }

    /// <summary>
    /// </summary>
    /// <param name="id"></param>
    public void Delete(string id)
    {
        try
        {
            var idObjectId = new ObjectId(id);
            Delete(idObjectId);
        }
        catch (Exception)
        {
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<TEntity> FindAsync(ObjectId id)
    {
        var filter = Builders<TEntity>.Filter.Eq("_id", id.ToString());
        return await _dbCollection.FindAsync(filter).Result.FirstOrDefaultAsync();
    }

    /// <summary>
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Task<TEntity> FindAsync(string id)
    {
        try
        {
            var idObjectId = new ObjectId(id);
            return FindAsync(idObjectId);
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<TEntity>> GetAll()
    {
        try
        {
            var all = await _dbCollection.FindAsync(Builders<TEntity>.Filter.Empty);
            return await all.ToListAsync();
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="id"></param>
    public async Task Update(TEntity obj, ObjectId id)
    {
        await _dbCollection.ReplaceOneAsync(Builders<TEntity>.Filter.Eq("_id", id.ToString()), obj);
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    public async Task<IClientSessionHandle> BeginSessionAsync()
    {
        return await _mongoContext.StartSessionAsync();
    }

    /// <summary>
    /// </summary>
    /// <param name="session"></param>
    /// <returns></returns>
    public async Task CommitTransactionAsync(IClientSessionHandle session)
    {
        await session.CommitTransactionAsync();
    }

    /// <summary>
    /// </summary>
    /// <param name="session"></param>
    /// <returns></returns>
    public async Task AbortTransactionAsync(IClientSessionHandle session)
    {
        await session.AbortTransactionAsync();
    }

    /// <summary>
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public async Task CreateRange(IEnumerable<TEntity> entities)
    {
        if (entities == null) throw new ArgumentNullException(typeof(TEntity).Name + " object is null");
        await _dbCollection.InsertManyAsync(entities);
    }

    /// <summary>
    /// </summary>
    /// <param name="id"></param>
    public void Delete(ObjectId id)
    {
        _dbCollection.DeleteOneAsync(Builders<TEntity>.Filter.Eq("_id", id.ToString()));
    }

    /// <summary>
    /// </summary>
    /// <param name="queryString"></param>
    /// <param name="collection"></param>
    /// <returns></returns>
    public async Task<string> AggregateRaw(string queryString, string collection)
    {
        var query = BsonSerializer.Deserialize<BsonDocument[]>(queryString).ToList();

        List<BsonDocument> list;
        using (var cursor = await _mongoContext.GetCollection<dynamic>(collection).AggregateAsync<BsonDocument>(query))
        {
            list = cursor.ToList();
        }

        if (list == null)
            return null;
        return list.ToJson();
    }
}