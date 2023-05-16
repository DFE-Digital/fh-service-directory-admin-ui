using FamilyHubs.SharedKernel.GovLogin.Configuration;
using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.SharedKernel.Identity.Authentication.Gov;
using FamilyHubs.SharedKernel.Identity.Authentication.Stub;
using FamilyHubs.SharedKernel.Identity.Authorisation;
using FamilyHubs.SharedKernel.Identity.Authorisation.FamilyHubs;
using FamilyHubs.SharedKernel.Identity.Authorisation.Stub;
using FamilyHubs.SharedKernel.Identity.Exceptions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FamilyHubs.SharedKernel.GovLogin.AppStart
{
    public static class ServiceCollectionExtension
    {
        /// <summary>
        /// Configures UI to authenticate using Gov Login
        /// </summary>
        public static void AddAndConfigureGovUkAuthentication(
            this IServiceCollection services, ConfigurationManager configuration)
        {
            services.AddHttpContextAccessor();

            var useStubAuthentication = configuration.GetValue<bool>("GovUkOidcConfiguration:StubAuthentication:UseStubAuthentication");
            if (useStubAuthentication)
            {
                configuration.AddJsonFile("stubUsers.json", true);
            }

            services.Configure<GovUkOidcConfiguration>(configuration.GetSection(nameof(GovUkOidcConfiguration)));

            var config = configuration.GetGovUkOidcConfiguration(); 
            if(config == null)
            {
                throw new AuthConfigurationException("Could not get Section GovUkOidcConfiguration from configuration");
            }

            services.AddOptions();
            services.AddSingleton(c => c.GetService<IOptions<GovUkOidcConfiguration>>()!.Value); 

            if (config.StubAuthentication.UseStubAuthentication)
            {
                services.AddStubAuthentication(config);
            }
            else
            {
                services.AddTransient<IAzureIdentityService, AzureIdentityService>();
                services.AddTransient<IJwtSecurityTokenService, JwtSecurityTokenService>();
                services.AddSingleton<IAuthorizationHandler, AuthorizationHandler>();
                services.AddHttpClient<IOidcService, OidcService>();
                services.AddGovUkAuthentication(configuration);
            }

            if (config.StubAuthentication.UseStubClaims)
            {
                services.AddTransient<ICustomClaims, StubClaims>();
            }
            else
            {
                if (string.IsNullOrEmpty(config.IdamsApiBaseUrl))
                    throw new AuthConfigurationException("IdamsApiBaseUrl is not configured, if testing locally and custom claims not required set StubAuthentication.UseStubClaims:true");

                services.AddHttpClient(nameof(FamilyHubsClaims), (serviceProvider, httpClient) =>
                {
                    httpClient.BaseAddress = new Uri(config.IdamsApiBaseUrl);
                });

                services.AddTransient<ICustomClaims, FamilyHubsClaims>();
            }

        }

        /// <summary>
        /// Adds a httpclient with user bearer token
        /// </summary>
        /// <param name="name">httpClient Name</param>
        /// <param name="configureClient">For further httpclient configuration</param>
        public static IServiceCollection AddSecureHttpClient(this IServiceCollection serviceCollection, string name, Action<IServiceProvider, HttpClient> configureClient)
        {
            serviceCollection.AddHttpClient(name).ConfigureHttpClient((serviceProvider, httpClient) =>
            {
                configureClient(serviceProvider, httpClient);
                var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
                if (httpContextAccessor == null)
                    throw new Exception($"IHttpContextAccessor required for {nameof(AddSecureHttpClient)}");

                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {httpContextAccessor.HttpContext!.GetBearerToken()}");
            });


            return serviceCollection;
        }

        /// <summary>
        /// For use in API. Endpoints with [Authorize] attribute with authorize using bearer tokens
        /// </summary>
        public static void AddBearerAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var config = configuration.GetGovUkOidcConfiguration();

            var privateKey = config.BearerTokenSigningKey;
            if (string.IsNullOrEmpty(privateKey))
                throw new AuthConfigurationException("BearerTokenSigningKey must be configured for AddBearerAuthentication");

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
              .AddJwtBearer(options => {
                  options.RequireHttpsMetadata = false;

                  options.TokenValidationParameters = new TokenValidationParameters
                  {
                      ValidateIssuerSigningKey = true,
                      IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(privateKey)),
                      ValidateLifetime = true,
                      ValidateAudience = false,
                      ValidateIssuer = false
                  };
              });
        }

        public static void AddAuthenticationCookie(this AuthenticationBuilder services, string cookieName, GovUkOidcConfiguration config)
        {

            services.AddCookie(options =>
            {
                options.AccessDeniedPath = new PathString("/error/403");
                options.ExpireTimeSpan = TimeSpan.FromMinutes(config.ExpiryInMinutes);
                options.Cookie.Name = cookieName;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.SlidingExpiration = true;
                options.Cookie.SameSite = SameSiteMode.None;
                options.CookieManager = new ChunkingCookieManager { ChunkSize = 3000 };
                options.LogoutPath = "/account/signout";
            });
        }
    }
}
