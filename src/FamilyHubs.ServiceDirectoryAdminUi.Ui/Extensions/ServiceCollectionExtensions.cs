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
        serviceCollection.AddClient<IOpenReferralOrganisationAdminClientService>((c, s) => new OpenReferralOrganisationAdminClientService(c));
        serviceCollection.AddClient<IUICacheService>((c, s) => new UICacheService(c));
        serviceCollection.AddClient<ISessionService>((c, s) => new SessionService());
        serviceCollection.AddClient<ITaxonomyService>((c, s) => new TaxonomyService(c));
        return serviceCollection;
    }

    private static IServiceCollection AddClient<T>(
        this IServiceCollection serviceCollection,
        Func<HttpClient, IServiceProvider, T> instance) where T : class
    {
        _ = serviceCollection.AddTransient(s =>
        {
            var srv = s.GetService<IOptions<ApiOptions>>();
            ArgumentNullException.ThrowIfNull(srv, nameof(srv));
            ApiOptions settings = srv.Value;
            ArgumentNullException.ThrowIfNull(settings, nameof(settings));

            var clientBuilder = new HttpClientBuilder()
                .WithDefaultHeaders()
                .WithApimAuthorisationHeader(settings)
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
