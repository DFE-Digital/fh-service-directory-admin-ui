using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;

public class ApiService
{
    protected readonly HttpClient Client;
    private readonly JsonSerializerOptions _caseInsensitive;

    protected ApiService(HttpClient client)
    {
        Client = client;
        _caseInsensitive = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    //todo: better error handling and common client
    protected async Task<T?> DeserializeResponse<T>(HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        return await JsonSerializer.DeserializeAsync<T>(await response.Content.ReadAsStreamAsync(cancellationToken), _caseInsensitive, cancellationToken);
    }
}
