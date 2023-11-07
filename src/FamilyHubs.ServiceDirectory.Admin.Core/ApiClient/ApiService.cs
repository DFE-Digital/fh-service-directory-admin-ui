using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;

//todo: ??
public interface IApiService
{

}

public class ApiService<TApiService> : IApiService
{
    protected readonly HttpClient Client;
    protected readonly ILogger<TApiService> Logger;
    private readonly JsonSerializerOptions _caseInsensitive;

    protected ApiService(HttpClient client, ILogger<TApiService> logger)
    {
        Client = client;
        Logger = logger;
        _caseInsensitive = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    //todo: better error handling and common client
    protected async Task<T?> DeserializeResponse<T>(HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        try
        {
            return await JsonSerializer.DeserializeAsync<T>(await response.Content.ReadAsStreamAsync(cancellationToken), _caseInsensitive, cancellationToken);
        }
        catch(Exception ex)
        {
            Logger.LogError($"Failed to DeserializeResponse StatusCode:{response.StatusCode} Error:{ex.Message}");
            throw;
        }
    }
}
