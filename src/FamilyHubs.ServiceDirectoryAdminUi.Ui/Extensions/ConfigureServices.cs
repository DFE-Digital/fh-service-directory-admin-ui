using AutoMapper;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Infrastructure.Configuration;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using fh_service_directory_api.core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
//using NSwag;
//using NSwag.Generation.Processors.Security;
//using Microsoft.Extensions.DependencyInjection;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Extensions;

public static class ConfigureServices
{
    public static IServiceCollection AddWebUIServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ApiOptions>(configuration.GetSection(ApiOptions.ApplicationServiceApi));
        //services.AddDatabaseDeveloperPageExceptionFilter();

        services.AddSingleton<ICurrentUserService, CurrentUserService>();

        services.AddHttpContextAccessor();

        //services.AddHealthChecks()
        //    .AddDbContextCheck<LAHubDbContext>();


        // Customise default API behaviour
        services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = true);

        

        return services;
    }

    public static IServiceCollection AddWebUIApplicationServices(this IServiceCollection services)
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new AutoMappingProfiles());
        });

        var mapper = config.CreateMapper();
        services.AddSingleton(mapper);

        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        return services;
    }
}
