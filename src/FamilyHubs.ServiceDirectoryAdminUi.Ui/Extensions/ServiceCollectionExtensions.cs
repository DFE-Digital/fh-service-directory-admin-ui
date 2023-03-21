using FamilyHubs.ServiceDirectoryAdminUi.Ui.Infrastructure.Configuration;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using Microsoft.Extensions.Options;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddClientServices(this IServiceCollection serviceCollection)
    {

        serviceCollection.AddPostCodeClient((c, s) => new PostcodeLocationClientService(c));
        serviceCollection.AddClient<ILocalOfferClientService>((c, s) => new LocalOfferClientService(c));
        serviceCollection.AddClient<IOrganisationAdminClientService>((c, s) => new OrganisationAdminClientService(c));
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
            var clientFactory = s.GetService<IHttpClientFactory>();
            var correlationService = s.GetService<ICorrelationService>();

            var httpClient = clientFactory?.CreateClient(name);

            ArgumentNullException.ThrowIfNull(httpClient);
            ArgumentNullException.ThrowIfNull(correlationService);

            httpClient.DefaultRequestHeaders.Add("X-Correlation-ID", correlationService.CorrelationId);
            return instance.Invoke(httpClient, s);
        });

        return serviceCollection;
    }

    private static IServiceCollection AddPostCodeClient(
        this IServiceCollection serviceCollection,
        Func<HttpClient, IServiceProvider, PostcodeLocationClientService> instance)
    {
        var name = nameof(PostcodeLocationClientService);
        serviceCollection.AddHttpClient(name).ConfigureHttpClient((serviceProvider, httpClient) =>
        {
            var srv = serviceProvider.GetService<IOptions<ApiOptions>>();
            ArgumentNullException.ThrowIfNull(srv, nameof(srv));
            var settings = srv.Value;
            ArgumentNullException.ThrowIfNull(settings, nameof(settings));

            httpClient.BaseAddress = new Uri("http://api.postcodes.io");

        });

        serviceCollection.AddScoped<IPostcodeLocationClientService>(s =>
        {
            var clientFactory = s.GetService<IHttpClientFactory>();
            var httpClient = clientFactory?.CreateClient(name);
            ArgumentNullException.ThrowIfNull(httpClient);
            return instance.Invoke(httpClient, s);
        });

        return serviceCollection;
    }
}
