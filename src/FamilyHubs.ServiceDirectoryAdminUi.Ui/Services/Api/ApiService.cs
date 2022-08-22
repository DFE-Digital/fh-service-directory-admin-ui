using SFA.DAS.HashingService;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;

public class ApiService : IApiService
{
    protected readonly HttpClient _client;
    protected readonly IHashingService _hashingService;

    public ApiService(HttpClient client, IHashingService hashingService)
    {
        _client = client;
        _hashingService = hashingService;
    }
}
