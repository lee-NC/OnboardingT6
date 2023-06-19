﻿namespace Demo.Common.DBBase.Config;

public interface IMongoDbSettings
{
    string CollectionName { get; set; }
    string ConnectionString { get; set; }
    string DatabaseName { get; set; }
}