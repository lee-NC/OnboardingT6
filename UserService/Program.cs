using System.Reflection;
using Demo.Common.Logging;
using Demo.Services.CompanyService.Entity.Api;
using Demo.Services.Helper;
using Demo.Services.Helper.Masstransit;
using Demo.Services.UserService.API;
using Demo.Services.UserService.Store;
using Demo.Workflow.MessageQueue;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using Services.Util.Auth;
using StackExchange.Redis;
using UserService.Helper;
using UserService.Helper.Telegram;
using UserService.Store;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

#region [1. Serilog with ELK Configuration]

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
configuration
    .AddJsonFile($"appsettings.{environment}.json", false, true)
    .AddJsonFile($"serilog.{environment}.json", false, true);
var serilog = configuration["Serilog"];

/**
 * ModifyConnectionSettings currently not support in appsettings.json
 * References:
 * https://github.com/serilog-contrib/serilog-sinks-elasticsearch/issues/144
 */
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .WriteTo.Async(c => c.Elasticsearch(
        new ElasticsearchSinkOptions(new Uri(configuration["Serilog:ElasticConfiguration:Uri"] ??
                                             "http://localhost:9200"))
        {
            IndexFormat = $"{Assembly.GetExecutingAssembly().GetName().Name?.ToLower()}-{DateTime.UtcNow:yyyy-MM}",
            AutoRegisterTemplate = true,
            OverwriteTemplate = true,
            TemplateName = "Services.UserService",
            AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
            TypeName = null,
            BatchAction = ElasticOpType.Create,
            ModifyConnectionSettings = c => c
                .ConnectionLimit(-1)
                .ServerCertificateValidationCallback((o, certificate, arg3, arg4) => { return true; })
                .BasicAuthentication(configuration["Serilog:ElasticConfiguration:Username"],
                    configuration["Serilog:ElasticConfiguration:Password"])
        }))
    .CreateLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog();
builder.Host.UseSerilog();

#endregion

builder.Services.AddProblemDetails(o => o.CustomizeProblemDetails = ctx =>
{
    var problemCorrelationId = Guid.NewGuid().ToString("N");
    ctx.ProblemDetails.Instance = problemCorrelationId;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});
builder.Services.AddResponseCompression(options => { options.EnableForHttps = true; });
builder.Services.Configure<RabbitMQSetting>(configuration.GetSection("RabbitMQSetting"));

/**
 * Authenticationhandler, currently support challenge scheme 
 */
builder.Services.AddAuthenticationScheme<UserAuthenticationHandler>(configuration);

// Telegram bot for notification
builder.Services.AddTelegramBot(configuration);

// Register database repositories
builder.Services.AddMongoDb(configuration);
builder.Services.AddScoped<IUserEntityStore, UserEntityStore>();
builder.Services.AddScoped<ICompanyEntityStore, CompanyEntityStore>();
builder.Services.AddHangfireService(configuration.GetSection("IMongoDbSettings:Host").Value ?? "", $"telegram_bot");


var redisConfig = ConfigurationOptions.Parse(
    string.Format("{0}:{1},password={2}",
        configuration["Cache:Redis:Host"],
        configuration["Cache:Redis:Port"],
        configuration["Cache:Redis:Password"]));
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.ConfigurationOptions = redisConfig;
    options.InstanceName = "Services.UserService";
});

// Register Masstransit client Request/Response
var rabbitSetting = configuration
    .GetSection("RabbitMQSetting")
    .Get<RabbitMQSetting>();
if (rabbitSetting is null)
    Log.Error("RabbitMQSetting section missing.");
else
    builder.Services.AddMasstransitConsumer(rabbitSetting);
builder.Services.AddHealthChecks();
builder.Services.AddMvc();

#region [Buiding app]

var app = builder.Build();
if (string.Equals("Enable", configuration["OpenApiSwagger"], StringComparison.OrdinalIgnoreCase))
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.OAuthClientId("4dc9-637696319279545777.apps.smartcaapi.com");
        c.OAuthClientSecret("NjA0MzU2ZmE-OGRhNy00ZGM5");
        c.OAuthUsername("173844193");
        c.OAuthUsePkce();
    });
}

app.MapGroup("/api/v1/user/")
    .MapUserEndpoints()
    .WithTags("User Api")
    .WithOpenApi()
    .WithMetadata();

app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = EnrichDiagnosticContext.EnrichFromRequest;
});

app.UseResponseCompression();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

#endregion

app.Run();