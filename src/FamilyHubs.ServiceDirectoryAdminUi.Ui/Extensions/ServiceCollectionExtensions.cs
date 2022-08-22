using WebUI.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using SFA.DAS.HashingService;
using SFA.DAS.Http;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddClientServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddClient<IApiService>((c, s) => new ApiService(c, s.GetRequiredService<IHashingService>()));
        serviceCollection.AddClient<IPostcodeLocationClientService>((c, s) => new PostcodeLocationClientService(c, s.GetRequiredService<IHashingService>()));
        serviceCollection.AddClient<ILocalOfferClientService>((c, s) => new LocalOfferClientService(c, s.GetRequiredService<IHashingService>()));
        serviceCollection.AddClient<IOpenReferralOrganisationAdminClientService>((c, s) => new OpenReferralOrganisationAdminClientService(c, s.GetRequiredService<IHashingService>()));
        return serviceCollection;
    }
    public static IServiceCollection AddHashingService(this IServiceCollection serviceCollection)
    {
        _ = serviceCollection.AddSingleton<IHashingService>(c =>
        {
            var settings = c.GetService<IOptions<WebConfigurationOptions>>()?.Value;
            string hashString = "CleanArchitecture";
            if (settings != null && !string.IsNullOrEmpty(settings.Hashstring))
                hashString = settings.Hashstring;
            return new HashingService("56789BCDFGHJKLMNPRSTVWXY", hashString);
        });

        return serviceCollection;
    }

    private static IServiceCollection AddClient<T>(
        this IServiceCollection serviceCollection,
        Func<HttpClient, IServiceProvider, T> instance) where T : class
    {
        _ = serviceCollection.AddTransient(s =>
        {
            var settings = s.GetService<IOptions<ApiOptions>>()?.Value;
            ArgumentNullException.ThrowIfNull(settings, nameof(settings));

            var clientBuilder = new HttpClientBuilder()
                .WithDefaultHeaders()
                //.WithApimAuthorisationHeader(settings)
                .WithLogging(s.GetService<ILoggerFactory>());

            var httpClient = clientBuilder.Build();

            if (!settings.ApiBaseUrl.EndsWith("/"))
            {
                settings.ApiBaseUrl += "/";
            }
            httpClient.BaseAddress = new Uri(settings.ApiBaseUrl);

            return instance.Invoke(httpClient, s);
        });

        return serviceCollection;
    }
}
