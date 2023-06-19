namespace Demo.Common.DBBase.Config;

public class MongoDbSettings : IMongoDbSettings
{
    public string Host { get; set; }
    public string CollectionName { get; set; }
    public string ConnectionString { get; set; }
    public string DatabaseName { get; set; }
}