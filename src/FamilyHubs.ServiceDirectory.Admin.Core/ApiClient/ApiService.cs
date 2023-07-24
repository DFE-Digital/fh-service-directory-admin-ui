using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;

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

    protected async Task<T?> DeserializeResponse<T>(HttpResponseMessage response, CancellationToken? cancellationToken = null)
    {
        try
        {
            T? result;

            if(cancellationToken != null)
            {
                result = await JsonSerializer.DeserializeAsync<T>(await response.Content.ReadAsStreamAsync(cancellationToken.Value), _caseInsensitive, cancellationToken.Value);
            }
            else
            {
                result = await JsonSerializer.DeserializeAsync<T>(await response.Content.ReadAsStreamAsync(), _caseInsensitive);
            }
            
            return result;
        }
        catch(Exception ex)
        {
            Logger.LogError($"Failed to DeserializeResponse StatusCode:{response.StatusCode} Error:{ex.Message}");
            throw;
        }
    }
}
