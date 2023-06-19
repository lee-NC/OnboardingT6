using System.IdentityModel.Tokens.Jwt;
using Demo.ApiGateway.DTOs;
using Demo.Portal.Helper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace Demo.ApiGateway;

public class Start
{
    public Start(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

        HttpClientHandler handler = new()
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Cookies";
                options.DefaultChallengeScheme = "user";
            })
            .AddCookie("Cookies", options =>
            {
                // Configure the client application to use sliding sessions
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
                options.AccessDeniedPath = "/auth/forbidden/";
                options.LoginPath = "/sign-in";
                options.LogoutPath = "/sign-out";
            });


        services.AddSingleton<JsonLoadSettings>(sp =>
        {
            return new JsonLoadSettings
            {
                DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error
            };
        });
        services.AddSession();

        // services.AddDistributedRedisCache(o =>
        // {
        //     o.Configuration = Configuration.GetConnectionString("Redis");
        // });

        services.AddSingleton<SessionFilter>();
        services.AddHealthChecks();
        services.AddMvc();


        var setting = Configuration.GetSection(nameof(CoreApiSetting)).Get<CoreApiSetting>();
        services.Configure<CoreApiSetting>(Configuration.GetSection(nameof(CoreApiSetting)));
        services.AddSingleton<ICoreClient, CoreClient>();
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        var logPath = Environment.ExpandEnvironmentVariables(@"%ALLUSERSPROFILE%\OnboardingT6\Portal\UI.Portal");
        services.AddDataProtection()
            .SetApplicationName("Demo Portal")
            .PersistKeysToFileSystem(new DirectoryInfo(logPath));
        //.DisableAutomaticKeyGeneration();
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

        app.UseSession();

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        if (!env.IsDevelopment())
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), @"Files")),
                RequestPath = new PathString("/files"),
                ServeUnknownFileTypes = true,
                DefaultContentType = "application/octet-stream"
            });

        app.UseRequestLocalization(app.ApplicationServices.GetRequiredService<IOptions<RequestLocalizationOptions>>()
            .Value);

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints => { endpoints.MapDefaultControllerRoute(); });
    }
}