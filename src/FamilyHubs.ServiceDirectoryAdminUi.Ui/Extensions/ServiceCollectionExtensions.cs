using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Extensions;

public static class ServiceCollectionExtensions
{
    public static WebApplicationBuilder AddClientServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddTransient<ISessionService, SessionService>();

        builder.Services.AddHttpClient<IApiService, ApiService>(client =>
        {
            client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("ApplicationServiceApi:ApiBaseUrl"));
        });

        builder.Services.AddHttpClient<IPostcodeLocationClientService, PostcodeLocationClientService>(client =>
        {
            client.BaseAddress = new Uri("http://api.postcodes.io");
        });

        builder.Services.AddHttpClient<IOpenReferralOrganisationAdminClientService, OpenReferralOrganisationAdminClientService>(client =>
        {
            string rt = builder.Configuration.GetValue<string>("ApplicationServiceApi:ApiBaseUrl");
            client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("ApplicationServiceApi:ApiBaseUrl"));
        }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();


        builder.Services.AddHttpClient<ILocalOfferClientService, LocalOfferClientService>(client =>
        {
            string rt = builder.Configuration.GetValue<string>("ApplicationServiceApi:ApiBaseUrl");
            client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("ApplicationServiceApi:ApiBaseUrl"));
        }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();


        builder.Services.AddHttpClient<IUICacheService, UICacheService>(client =>
        {
            client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("ApplicationServiceApi:ApiBaseUrl"));
        }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();

        
        builder.Services.AddHttpClient<ITaxonomyService, TaxonomyService>(client =>
        {
            client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("ApplicationServiceApi:ApiBaseUrl"));
        }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();

        builder.Services.AddHttpClient<IAuthService, AuthService>(client =>
        {
            client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("AuthServiceUrl"));
        });

        return builder;
    }
}
