
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

namespace Demo.ApiGateway;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        // services.AddControllersWithViews().AddRazorRuntimeCompilation();
        // JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
        //
        // HttpClientHandler handler = new()
        // {
        //     ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        // };
        //
        // services.AddAuthentication(options =>
        //     {
        //         options.DefaultScheme = "Cookies";
        //         options.DefaultChallengeScheme = "user";
        //     })
        //     .AddCookie("Cookies", options =>
        //     {
        //         options.SlidingExpiration = true;
        //         options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
        //     });
        //
        // // var x = Configuration.GetSection("SslClientSetting");
        // // services.Configure<SslClientSetting>(Configuration.GetSection("SslClientSetting"));
        //
        // services.AddSession(options =>
        // {
        //     options.IdleTimeout = TimeSpan.FromHours(4);
        //     options.Cookie.HttpOnly = false; // correct initialization
        // });
        //
        // services.AddDistributedRedisCache(o => { o.Configuration = Configuration.GetConnectionString("Redis"); });
        //
        // services.AddSingleton<SessionFilter>();
        //
        //
        // CoreApiSetting? setting = Configuration.GetSection(nameof(CoreApiSetting)).Get<CoreApiSetting>();
        // services.Configure<CoreApiSetting>(Configuration.GetSection(nameof(CoreApiSetting)));
        // services.AddSingleton<ICoreClient, CoreClient>();
        // services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        //
        // var logPath = Environment.ExpandEnvironmentVariables(@"%ALLUSERSPROFILE%\OnboardingT6\Portal\UI.Portal");
        // services.AddDataProtection()
        //     .SetApplicationName("Demo")
        //     .PersistKeysToFileSystem(new DirectoryInfo(logPath));
        // //.DisableAutomaticKeyGeneration();
        services
            .AddOcelot(Configuration);
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        // app.UseSession();

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        if (!env.IsDevelopment())
        {
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), @"Files")),
                RequestPath = new PathString("/files"),
                ServeUnknownFileTypes = true,
                DefaultContentType = "application/octet-stream"
            });
        }


        app.UseRequestLocalization(app.ApplicationServices.GetRequiredService<IOptions<RequestLocalizationOptions>>()
            .Value);

        app.UseRouting();

        // app.UseAuthentication();
        // app.UseAuthorization();

        app.UseEndpoints(endpoints => { endpoints.MapDefaultControllerRoute(); });
        app.UseOcelot();
    }
}