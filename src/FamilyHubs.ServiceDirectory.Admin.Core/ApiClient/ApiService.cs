namespace FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;

public interface IApiService
{

}

public class ApiService : IApiService
{
    protected readonly HttpClient Client;

    protected ApiService(HttpClient client)
    {
        Client = client;
    }
}
