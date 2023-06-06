using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Core.Services.DataUpload;
using FamilyHubs.ServiceDirectory.Admin.Web.Middleware;
using FamilyHubs.SharedKernel.GovLogin.AppStart;
using FamilyHubs.SharedKernel.Security;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Serilog;
using Serilog.Events;
using StackExchange.Redis;

namespace FamilyHubs.ServiceDirectory.Admin.Web;

public static class StartupExtensions
{
    public static void ConfigureHost(this WebApplicationBuilder builder)
    {
        // ApplicationInsights
        builder.Host.UseSerilog((_, services, loggerConfiguration) =>
        {
            var logLevelString = builder.Configuration["LogLevel"];

            var parsed = Enum.TryParse<LogEventLevel>(logLevelString, out var logLevel);

            loggerConfiguration.WriteTo.ApplicationInsights(
                services.GetRequiredService<TelemetryConfiguration>(),
                TelemetryConverter.Traces,
                parsed ? logLevel : LogEventLevel.Warning);

            loggerConfiguration.WriteTo.Console(parsed ? logLevel : LogEventLevel.Warning);
        });
    }

    public static void ConfigureServices(this IServiceCollection services, ConfigurationManager configuration)
    {
        services.AddApplicationInsightsTelemetry();

        // Add services to the container.
        services
        .AddClientServices(configuration)
            .AddWebUiServices(configuration);

        services.AddAndConfigureGovUkAuthentication(configuration);
        services.AddTransient<IViewModelToApiModelHelper, ViewModelToApiModelHelper>();

        services.AddSingleton<ICacheService, CacheService>();
        services.AddTransient<IExcelReader, ExcelReader>();
        services.AddTransient<IDataUploadService, DataUploadService>();
        services.AddScoped<ICorrelationService, CorrelationService>();

        // Add services to the container.
        services.AddRazorPages(options =>
        {
            options.Conventions.AuthorizeFolder("/OrganisationAdmin");
            options.Conventions.AuthorizeFolder("/AccountAdmin");
        });

        services.AddAuthorization(options => options.AddPolicy("DfeAdmin", policy =>
            policy.RequireAssertion(context =>
                context.User.HasClaim(claim => claim.Value == "Admin") ||
                context.User.HasClaim(claim => claim.Value == "DFE")
            )));
        
        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(configuration.GetValue<int>("SessionTimeOutMinutes"));
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });
        
        // Add Session middleware
        services.AddDistributedCache(configuration);
    }
    
    public static IServiceCollection AddDistributedCache(this IServiceCollection services, ConfigurationManager configuration)
    {
        var cacheConnection = configuration.GetValue<string>("CacheConnection");

        if (string.IsNullOrWhiteSpace(cacheConnection))
        {
            services.AddDistributedMemoryCache();
        }
        else
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = cacheConnection;
                options.InstanceName = "AdminWeb";
                
                // var configurationOptions = ConfigurationOptions.Parse(cacheConnection);
                // configurationOptions.ReconnectRetryPolicy = new ExponentialRetry(configurationOptions.ConnectTimeout / 2);
                // configurationOptions.SocketManager = SocketManager.ThreadPool;
                //
                // options.ConfigurationOptions = configurationOptions;
            });
        }

        services.AddTransient<ICacheService, CacheService>();
        
        services.AddTransient<ICacheKeys, CacheKeys>();

        // there's currently only one, so this should be fine
        services.AddSingleton(new DistributedCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(configuration.GetValue<int>("SessionTimeOutMinutes"))
        });
        
        return services;
    }

    public static void AddWebUiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        // Customise default API behaviour
        services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);
    }

    public static IServiceCollection AddClientServices(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        var erviceDirectoryApiBaseUrl = configuration.GetValue<string?>("ServiceDirectoryApiBaseUrl");

        serviceCollection.AddPostCodeClient((c, _) => new PostcodeLocationClientService(c));
        serviceCollection.AddClient<IServiceDirectoryClient>(configuration, "ServiceDirectoryApiBaseUrl", (c, _) => new ServiceDirectoryClient(c));
        serviceCollection.AddClient<ITaxonomyService>(configuration, "ServiceDirectoryApiBaseUrl", (c, _) => new TaxonomyService(c));
        serviceCollection.AddClient<IIdamClient>(configuration, "IdamApi", (c, _) => new IdamClient(c));

        return serviceCollection;
    }

    private static void AddClient<T>(this IServiceCollection services, IConfiguration config, string baseUrlKey, Func<HttpClient, IServiceProvider, T> instance) where T : class
    {
        var name = typeof(T).Name;

        services.AddSecureHttpClient(name, (serviceProvider, httpClient) =>
        {
            var baseUrl = config.GetValue<string?>(baseUrlKey);
            ArgumentNullException.ThrowIfNull(baseUrl, $"appsettings.{baseUrlKey}");

            httpClient.BaseAddress = new Uri(baseUrl);
        });

        services.AddScoped<T>(s =>
        {
            var clientFactory = s.GetService<IHttpClientFactory>();
            var correlationService = s.GetService<ICorrelationService>();

            var httpClient = clientFactory?.CreateClient(name);

            ArgumentNullException.ThrowIfNull(httpClient);
            ArgumentNullException.ThrowIfNull(correlationService);

            httpClient.DefaultRequestHeaders.Add("X-Correlation-ID", correlationService.CorrelationId);
            return instance.Invoke(httpClient, s);
        });
    }

    private static void AddPostCodeClient(this IServiceCollection serviceCollection, Func<HttpClient, IServiceProvider, PostcodeLocationClientService> instance)
    {
        const string Name = nameof(PostcodeLocationClientService);
        serviceCollection.AddHttpClient(Name).ConfigureHttpClient((_, httpClient) =>
        {
            httpClient.BaseAddress = new Uri("http://api.postcodes.io");
        });

        serviceCollection.AddScoped<IPostcodeLocationClientService>(s =>
        {
            var clientFactory = s.GetService<IHttpClientFactory>();
            var httpClient = clientFactory?.CreateClient(Name);
            ArgumentNullException.ThrowIfNull(httpClient);
            return instance.Invoke(httpClient, s);
        });
    }

    public static void ConfigureWebApplication(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        app.UseMiddleware<CorrelationMiddleware>();

        app.UseAppSecurityHeaders();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        app.UseStatusCodePagesWithReExecute("/Error/{0}");

#if use_https
        app.UseHttpsRedirection();
#endif
        
        app.UseStaticFiles();

        app.UseRouting();

        app.UseGovLoginAuthentication();

        app.UseSession();

        app.MapRazorPages();
    }
}