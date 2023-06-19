using CompanyService.Entity.Api.Repositories;
using Demo.Common.DBBase.Config;
using Demo.Common.DBBase.Context;
using Demo.Services.UserService.Entity.Api.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.Services.CompanyService.Entity.Api;

public static class MongoDbRepositoryExtensions
{
    public static void AddMongoDb(this IServiceCollection services, IConfiguration Configuration)
    {
        services.Configure<MongoDbSettings>(Configuration.GetSection(typeof(MongoDbSettings).Name));
        services.AddScoped<IMongoDbContext, MongoDbContext>();
        services.AddScoped<ICompanyEntityRepository, CompanyEntityRepository>();
        services.AddScoped<IUserEntityRepository, UserEntityRepository>();
    }
}