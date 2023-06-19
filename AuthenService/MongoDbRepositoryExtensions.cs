using Demo.Common.DBBase.Config;
using Demo.Common.DBBase.Context;
using Demo.Services.UserService.Entity.Api.Repositories;
using LogService.Repositories;

namespace Demo.Services.AuthenService;

/// <summary>
/// </summary>
public static class MongoDbRepositoryExtensions
{
    /// <summary>
    /// </summary>
    /// <param name="services"></param>
    /// <param name="Configuration"></param>
    public static void AddMongoDb(this IServiceCollection services, IConfiguration Configuration)
    {
        services.Configure<MongoDbSettings>(Configuration.GetSection(typeof(MongoDbSettings).Name));
        services.AddScoped<IMongoDbContext, MongoDbContext>();
        services.AddScoped<ITransactionLogRepository, TransactionLogRepository>();
        services.AddScoped<IUserEntityRepository, UserEntityRepository>();
    }
}