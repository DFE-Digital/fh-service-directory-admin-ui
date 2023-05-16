using FamilyHubs.SharedKernel.GovLogin.AppStart;
using FamilyHubs.SharedKernel.GovLogin.Configuration;
using FamilyHubs.SharedKernel.Identity.Exceptions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyHubs.SharedKernel.Identity.Authentication.Stub
{
    internal static class ServiceCollectionExtension
    {

        public static void AddStubAuthentication(this IServiceCollection services, GovUkOidcConfiguration config)
        {
            if (string.IsNullOrWhiteSpace(config.CookieName))
                throw new AuthConfigurationException($"CookieName is not configured in {nameof(GovUkOidcConfiguration)} section of appsettings");

            services
                .AddAuthentication(sharedOptions =>
                {
                    sharedOptions.DefaultSignOutScheme = OpenIdConnectDefaults.AuthenticationScheme;
                    sharedOptions.DefaultChallengeScheme = config.CookieName;
                })
                .AddScheme<AuthenticationSchemeOptions, StubAuthenticationHandler>(config.CookieName, _ => { })
                .AddCookie(OpenIdConnectDefaults.AuthenticationScheme);

            services.AddAuthentication(config.CookieName).AddAuthenticationCookie(config.CookieName, config);
        }

    }
}
