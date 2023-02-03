using System.Text.Json;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.SharedKernel;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;

public interface ILocalOfferClientService
{
    Task<PaginatedList<ServiceDto>> GetLocalOffers(string serviceType, string status, int minimum_age, int maximum_age, double? latitude, double? longtitude, double? proximity, int pageNumber, int pageSize, string text, string? serviceDeliveries, bool? isPaidFor, string? taxonmyIds);
    Task<ServiceDto> GetLocalOfferById(string id);
    //Task<PaginatedList<TestItem>> GetTestCommand(double latitude, double logtitude, double meters);

    Task<List<ServiceDto>> GetServicesByOrganisationId(string id);
    Task<bool> DeleteServiceById(string id);
}

public class LocalOfferClientService : ApiService, ILocalOfferClientService
{
    public LocalOfferClientService(HttpClient client)
        : base(client)
    {

    }

    public async Task<PaginatedList<ServiceDto>> GetLocalOffers(string serviceType, string status, int minimum_age, int maximum_age, double? latitude, double? longtitude, double? proximity, int pageNumber, int pageSize, string text, string? serviceDeliveries, bool? isPaidFor, string? taxonmyIds)
    {
        if (string.IsNullOrEmpty(status))
            status = "active";

        var url = string.Empty;
        if (latitude != null && longtitude != null && proximity != null)
            url = $"api/services?serviceType={serviceType}&status={status}&minimum_age={minimum_age}&maximum_age={maximum_age}&latitude={latitude}&longtitude={longtitude}&proximity={proximity}&pageNumber={pageNumber}&pageSize={pageSize}&text={text}";
        else
            url = $"api/services?serviceType={serviceType}&status={status}&minimum_age={minimum_age}&maximum_age={maximum_age}&pageNumber={pageNumber}&pageSize={pageSize}&text={text}";

        if (serviceDeliveries != null)
        {
            url += $"&serviceDeliveries={serviceDeliveries}";
        }

        if (isPaidFor != null)
        {
            url += $"&isPaidFor={isPaidFor.Value}";
        }

        if (taxonmyIds != null)
        {
            url += $"&taxonmyIds={taxonmyIds}";
        }

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(_client.BaseAddress + url),
        };

        using var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        return await JsonSerializer.DeserializeAsync<PaginatedList<ServiceDto>>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new PaginatedList<ServiceDto>();
    }

    public async Task<ServiceDto> GetLocalOfferById(string id)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(_client.BaseAddress + $"api/services/{id}"),

        };

        using var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var retVal = await JsonSerializer.DeserializeAsync<ServiceDto>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        ArgumentNullException.ThrowIfNull(retVal, nameof(retVal));

        return retVal;
    }

    public async Task<List<ServiceDto>> GetServicesByOrganisationId(string id)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(_client.BaseAddress + $"api/organisationservices/{id}"),
        };

        using var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        return await JsonSerializer.DeserializeAsync<List<ServiceDto>>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ServiceDto>();
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

        var retVal = await JsonSerializer.DeserializeAsync<bool>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        ArgumentNullException.ThrowIfNull(retVal, nameof(retVal));

        return retVal;
    }
}
