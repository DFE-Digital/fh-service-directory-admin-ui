using FamilyHubs.SharedKernel.Razor.Health;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FamilyHubs.ServiceDirectory.Admin.Core.Health;

public static class HealthCheck
{
    public static IServiceCollection AddSiteHealthChecks(
        this IServiceCollection services,
        IConfiguration config)
    {
        var oneLoginUrl = config.GetValue<string>("GovUkOidcConfiguration:Oidc:BaseUrl");

        // we handle API failures as Degraded, so that App Services doesn't remove or replace the instance (all instances!) due to an API being down
        var healthCheckBuilder = services.AddFamilyHubsHealthChecks(config)
            .AddIdentityServer(new Uri(oneLoginUrl!), name: "One Login", failureStatus: HealthStatus.Degraded, tags: new[] { "ExternalAPI" });

        var notificationApiUrl = config.GetValue<string>("Notification:Endpoint");
        if (!string.IsNullOrEmpty(notificationApiUrl))
        {
            // special case as Url contains path
            //todo: change notifications client to use host and append path
            notificationApiUrl = notificationApiUrl.Replace("/api/notify", "/api/info");
            healthCheckBuilder.AddUrlGroup(new Uri(notificationApiUrl), "Notification API", HealthStatus.Degraded,
                new[] { FhHealthChecksBuilder.UrlType.InternalApi.ToString() });
        }

        return services;
    }
}