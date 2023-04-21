using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Core.Services.DataUpload;
using FamilyHubs.ServiceDirectory.Admin.Web.Middleware;
using FamilyHubs.SharedKernel.Security;
using FamilyHubs.SharedKernel.GovLogin.AppStart;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Events;
using FamilyHubs.SharedKernel.GovLogin.Configuration;

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

    public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplicationInsightsTelemetry();

        // Add services to the container.
        services
        .AddClientServices(configuration)
            .AddWebUiServices(configuration);

        services.Configure<GovUkOidcConfiguration>(configuration.GetSection(nameof(GovUkOidcConfiguration)));
        services.AddAndConfigureGovUkAuthentication(configuration, "FamilyHubsAdminUi.Auth");
        services.AddTransient<IViewModelToApiModelHelper, ViewModelToApiModelHelper>();

        services.AddSingleton<ICacheService, CacheService>();
        services.AddTransient<IExcelReader, ExcelReader>();
        services.AddTransient<IDataUploadService, DataUploadService>();
        services.AddScoped<ICorrelationService, CorrelationService>();

        // Add services to the container.
        services.AddRazorPages(options =>
        {
            options.Conventions.AuthorizeFolder("/OrganisationAdmin");
        });

        // Add Session middleware
        services.AddDistributedMemoryCache();

        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(configuration.GetValue<int>("SessionTimeOutMinutes"));
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });
    }

    public static void AddWebUiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        // Customise default API behaviour
        services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);
    }

    public static IServiceCollection AddClientServices(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddPostCodeClient((c, _) => new PostcodeLocationClientService(c));
        serviceCollection.AddClient<IOrganisationAdminClientService>(configuration, (c, _) => new OrganisationAdminClientService(c));
        serviceCollection.AddClient<ITaxonomyService>(configuration, (c, _) => new TaxonomyService(c));

        return serviceCollection;
    }

    private static void AddClient<T>(this IServiceCollection serviceCollection, IConfiguration configuration, Func<HttpClient, IServiceProvider, T> instance) where T : class
    {
        var name = typeof(T).Name;
        serviceCollection.AddHttpClient(name).ConfigureHttpClient(httpClient =>
        {
            var serviceDirectoryApiBaseUrl = configuration.GetValue<string?>("ServiceDirectoryApiBaseUrl");
            ArgumentNullException.ThrowIfNull(serviceDirectoryApiBaseUrl);

            httpClient.BaseAddress = new Uri(serviceDirectoryApiBaseUrl);
        });

        serviceCollection.AddScoped<T>(s =>
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

    private static void AddPostCodeClient(this IServiceCollection serviceCollection,
        Func<HttpClient, IServiceProvider, PostcodeLocationClientService> instance)
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
