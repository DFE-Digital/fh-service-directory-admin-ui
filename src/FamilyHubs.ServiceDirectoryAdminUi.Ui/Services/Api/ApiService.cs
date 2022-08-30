namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;

public class ApiService : IApiService
{
    protected readonly HttpClient _client;

    public ApiService(HttpClient client)
    {
        _client = client;
    }
}
