using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServices;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.SharedKernel;
using System.Text.Json;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;

public interface ILocalOfferClientService
{
    Task<PaginatedList<OpenReferralServiceDto>> GetLocalOffers(string status, int minimum_age, int maximum_age, double latitude, double longtitude, double proximity, int pageNumber, int pageSize, string text);
    Task<OpenReferralServiceDto> GetLocalOfferById(string id);
    //Task<PaginatedList<TestItem>> GetTestCommand(double latitude, double logtitude, double meters);

    Task<List<OpenReferralServiceDto>> GetServicesByOrganisationId(string id);
    Task<bool> DeleteServiceById(string id);
}

public class LocalOfferClientService : ApiService, ILocalOfferClientService
{
    public LocalOfferClientService(HttpClient client)
        : base(client)
    {

    }

    public async Task<PaginatedList<OpenReferralServiceDto>> GetLocalOffers(string status, int minimum_age, int maximum_age, double latitude, double longtitude, double proximity, int pageNumber, int pageSize, string text)
    {
        if (string.IsNullOrEmpty(status))
            status = "active";

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(_client.BaseAddress + $"api/services?status={status}&minimum_age={minimum_age}&maximum_age={maximum_age}&latitude={latitude}&longtitude={longtitude}&proximity={proximity}&pageNumber={pageNumber}&pageSize={pageSize}&text={text}"),
        };

        using var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        return await JsonSerializer.DeserializeAsync<PaginatedList<OpenReferralServiceDto>>(await response.Content.ReadAsStreamAsync(), options: new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new PaginatedList<OpenReferralServiceDto>();
    }

    public async Task<OpenReferralServiceDto> GetLocalOfferById(string id)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(_client.BaseAddress + $"api/services/{id}"),

        };

        using var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var retVal = await JsonSerializer.DeserializeAsync<OpenReferralServiceDto>(await response.Content.ReadAsStreamAsync(), options: new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        ArgumentNullException.ThrowIfNull(retVal, nameof(retVal));

        return retVal;
    }

    public async Task<List<OpenReferralServiceDto>> GetServicesByOrganisationId(string id)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(_client.BaseAddress + $"api/organisationservices/{id}"),
        };

        using var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        return await JsonSerializer.DeserializeAsync<List<OpenReferralServiceDto>>(await response.Content.ReadAsStreamAsync(), options: new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<OpenReferralServiceDto>();
    }

    public async Task<bool> DeleteServiceById(string id)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Delete,
            RequestUri = new Uri(_client.BaseAddress + $"api/services/{id}"),

        };

        using var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var retVal = await JsonSerializer.DeserializeAsync<bool>(await response.Content.ReadAsStreamAsync(), options: new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        ArgumentNullException.ThrowIfNull(retVal, nameof(retVal));

        return retVal;
    }
}
