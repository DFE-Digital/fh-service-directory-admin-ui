using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;

namespace FamilyHubs.ServiceDirectory.Admin.Core.ServiceValidators;

public static class ServiceRenderCheckerServiceCollectionExtension
{
    //todo: not for poc, but could make this code generic by introducing a interface containing HttpClientName, GetEndpoint and timeout and generic logger type
    //todo: would we want a retry policy in the production version?
    public static void AddServiceRenderChecker(this IServiceCollection services, IConfiguration configuration)
    {
        var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(5);

        var delay = Backoff.DecorrelatedJitterBackoffV2(
            medianFirstRetryDelay: TimeSpan.FromSeconds(1),
            retryCount: 2);

        services.AddHttpClient(ServiceRenderChecker.HttpClientName, client =>
            {
                //client.BaseAddress = new Uri(AiClient.GetEndpoint(configuration));
            })
            .AddPolicyHandler((callbackServices, request) => HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(delay, (result, timespan, retryAttempt, context) =>
                {
                    callbackServices.GetService<ILogger<ServiceRenderChecker>>()?
                        .LogWarning("Delaying for {Timespan}, then making retry {RetryAttempt}.",
                            timespan, retryAttempt);
                }))
            .AddPolicyHandler(timeoutPolicy);

        services.AddTransient<IServiceRenderChecker, ServiceRenderChecker>();
    }
}