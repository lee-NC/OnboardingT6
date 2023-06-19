using Autofac.Extensions.DependencyInjection;
using Demo.ApiGateway;
using Serilog;
using Serilog.Events;

public class Program
{
    public static void Main(string[] args)
    {
        var configuration = GetConfiguration(args, out var env);

        var logPath = configuration["Serilog:SinkFile:Path"];
        Console.WriteLine($"Logpath: {logPath}");
        if (string.IsNullOrEmpty(logPath)) logPath = @"%ALLUSERSPROFILE%\OnboardingT6\Portal\Portal.log";
        logPath = Environment.ExpandEnvironmentVariables(logPath);

        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Environment", env)
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .WriteTo.Async(c =>
                c.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"))
            .WriteTo.PersistentFile(logPath, persistentFileRollingInterval: PersistentFileRollingInterval.Day,
                preserveLogFilename: true)
            .CreateLogger();
        Log.Information("Starting Portal...");
        Log.Information($"Env: {env}");

        CreateHostBuilder(args).Build().Run();
        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = false
        };

        var client = new HttpClient(handler);
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .UseServiceProviderFactory(new AutofacServiceProviderFactory())
            .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Start>(); })
            .UseSerilog();
    }

    private static IConfiguration GetConfiguration(string[] args, out string environment)
    {
        environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (string.IsNullOrEmpty(environment)) environment = "Production";
        Console.WriteLine(environment);

        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile($"appsettings.{environment}.json", true, true);

        configurationBuilder.AddCommandLine(args);
        configurationBuilder.AddEnvironmentVariables();

        return configurationBuilder.Build();
    }
}