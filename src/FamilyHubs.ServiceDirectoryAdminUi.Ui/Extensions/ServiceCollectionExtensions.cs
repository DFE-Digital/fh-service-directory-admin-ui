using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddClientServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<ISessionService, SessionService>();

        services.AddHttpClient<IApiService, ApiService>(client =>
        {
            client.BaseAddress = new Uri(configuration.GetValue<string>("ApplicationServiceApi:ApiBaseUrl"));
        });

        services.AddHttpClient<IPostcodeLocationClientService, PostcodeLocationClientService>(client =>
        {
            client.BaseAddress = new Uri("http://api.postcodes.io");
        });

        services.AddHttpClient<IOpenReferralOrganisationAdminClientService, OpenReferralOrganisationAdminClientService>(client =>
        {
            client.BaseAddress = new Uri(configuration.GetValue<string>("ApplicationServiceApi:ApiBaseUrl"));
        }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();


        services.AddHttpClient<ILocalOfferClientService, LocalOfferClientService>(client =>
        {
            client.BaseAddress = new Uri(configuration.GetValue<string>("ApplicationServiceApi:ApiBaseUrl"));
        }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();


        services.AddHttpClient<IUICacheService, UICacheService>(client =>
        {
            client.BaseAddress = new Uri(configuration.GetValue<string>("ApplicationServiceApi:ApiBaseUrl"));
        }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();

        
        services.AddHttpClient<ITaxonomyService, TaxonomyService>(client =>
        {
            client.BaseAddress = new Uri(configuration.GetValue<string>("ApplicationServiceApi:ApiBaseUrl"));
        }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();

        services.AddHttpClient<IAuthService, AuthService>(client =>
        {
            client.BaseAddress = new Uri(configuration.GetValue<string>("AuthServiceUrl"));
        });
    }
}
