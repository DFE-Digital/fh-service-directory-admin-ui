using Microsoft.Extensions.Logging;

namespace FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;

public interface IApiService
{

}

public class ApiService<T> : IApiService
{
    protected readonly HttpClient Client;
    protected readonly ILogger<T> Logger;

    protected ApiService(HttpClient client, ILogger<T> logger)
    {
        Client = client;
        Logger = logger;
    }


}
