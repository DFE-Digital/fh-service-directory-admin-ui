using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;

namespace FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;

public static class AiClientServiceCollectionExtension
{
    public static void AddAiClient(this IServiceCollection services, IConfiguration configuration)
    {
        var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(10);

        var delay = Backoff.DecorrelatedJitterBackoffV2(
            medianFirstRetryDelay: TimeSpan.FromSeconds(1),
            retryCount: 2);

        services.AddHttpClient(AiClient.HttpClientName, client =>
        {
            client.BaseAddress = new Uri(AiClient.GetEndpoint(configuration));
        })
            .AddPolicyHandler((callbackServices, request) => HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(delay, (result, timespan, retryAttempt, context) =>
                {
                    callbackServices.GetService<ILogger<AiClient>>()?
                        .LogWarning("Delaying for {Timespan}, then making retry {RetryAttempt}.",
                            timespan, retryAttempt);
                }))
            .AddPolicyHandler(timeoutPolicy);

        services.AddTransient<IAiClient, AiClient>();
    }
}