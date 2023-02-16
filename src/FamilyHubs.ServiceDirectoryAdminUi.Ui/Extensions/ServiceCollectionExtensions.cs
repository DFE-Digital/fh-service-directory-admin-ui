using FamilyHubs.ServiceDirectoryAdminUi.Ui.Infrastructure.Configuration;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using Microsoft.Extensions.Options;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddClientServices(this IServiceCollection serviceCollection)
    {

        serviceCollection.AddClient<IApiService>((c, s) => new ApiService(c));
        serviceCollection.AddClient<IPostcodeLocationClientService>((c, s) => new PostcodeLocationClientService(c));
        serviceCollection.AddClient<ILocalOfferClientService>((c, s) => new LocalOfferClientService(c));
        serviceCollection.AddClient<IOrganisationAdminClientService>((c, s) => new OrganisationAdminClientService(c));
        serviceCollection.AddClient<IUICacheService>((c, s) => new UICacheService(c));
        serviceCollection.AddClient<ISessionService>((c, s) => new SessionService());
        serviceCollection.AddClient<ITaxonomyService>((c, s) => new TaxonomyService(c));

        return serviceCollection;
    }

    private static IServiceCollection AddClient<T>(
        this IServiceCollection serviceCollection,
        Func<HttpClient, IServiceProvider, T> instance) where T : class
    {
        var name = typeof(T).Name;
        serviceCollection.AddHttpClient(name).ConfigureHttpClient((serviceProvider, httpClient) =>
        {
            var srv = serviceProvider.GetService<IOptions<ApiOptions>>();
            ArgumentNullException.ThrowIfNull(srv, nameof(srv));
            var settings = srv.Value;
            ArgumentNullException.ThrowIfNull(settings, nameof(settings));

            if (!settings.ApiBaseUrl.EndsWith("/"))
            {
                settings.ApiBaseUrl += "/";
            }
            httpClient.BaseAddress = new Uri(settings.ApiBaseUrl);
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", settings.SubscriptionKey);
            httpClient.DefaultRequestHeaders.Add("X-Version", settings.ApiVersion);
        });

        serviceCollection.AddScoped<T>(s =>
        {
            var clientFactory = s.GetService<System.Net.Http.IHttpClientFactory>();
            var correlationService = s.GetService<ICorrelationService>();

            var httpClient = clientFactory?.CreateClient(name);

            ArgumentNullException.ThrowIfNull(httpClient);
            ArgumentNullException.ThrowIfNull(correlationService);

            httpClient.DefaultRequestHeaders.Add("X-Correlation-ID", correlationService.CorrelationId);
            return instance.Invoke(httpClient, s);
        });

        return serviceCollection;
    }

}
