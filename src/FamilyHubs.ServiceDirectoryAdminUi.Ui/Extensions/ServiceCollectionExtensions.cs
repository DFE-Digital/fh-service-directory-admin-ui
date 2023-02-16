using FamilyHubs.ServiceDirectoryAdminUi.Ui.Infrastructure.Configuration;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using Microsoft.Extensions.Options;
using SFA.DAS.Http;

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
        serviceCollection.AddHttpClient<T>();

        _ = serviceCollection.AddScoped(s =>
        {
            var client = s.GetService<HttpClient<T>>();
            ArgumentNullException.ThrowIfNull(client, nameof(client));
            return instance.Invoke(client.Instance, s);
        });

        return serviceCollection;
    }

    private static void AddHttpClient<T>(this IServiceCollection serviceCollection)
    {

        _ = serviceCollection.AddSingleton(serviceProvider =>
        {
            var srv = serviceProvider.GetService<IOptions<ApiOptions>>();
            ArgumentNullException.ThrowIfNull(srv, nameof(srv));
            var settings = srv.Value;
            ArgumentNullException.ThrowIfNull(settings, nameof(settings));

            var clientBuilder = new HttpClientBuilder()
                .WithDefaultHeaders()
                .WithApimAuthorisationHeader(settings)
                .WithLogging(serviceProvider.GetService<ILoggerFactory>());

            var httpClient = clientBuilder.Build();

            if (!settings.ApiBaseUrl.EndsWith("/"))
            {
                settings.ApiBaseUrl += "/";
            }
            httpClient.BaseAddress = new Uri(settings.ApiBaseUrl);

            return new HttpClient<T>(httpClient);
        });
    }
}

public class HttpClient<T>
{
    public HttpClient(HttpClient httpClient)
    {
        Instance = httpClient;
    }

    public HttpClient Instance { get; private set; }
}