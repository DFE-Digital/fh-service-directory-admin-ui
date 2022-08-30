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


    //    public async Task<PaginatedList<TestItem>> GetTestCommand(double latitude, double logtitude, double meters)
    //    {
    //        GetServicesByDistanceCommand command = new(latitude, logtitude, meters);

    //        var request = new HttpRequestMessage
    //        {
    //            Method = HttpMethod.Post,
    //            RequestUri = new Uri(_client.BaseAddress + "api/GetTestCommandDepricated"),
    //            Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(command), Encoding.UTF8, "application/json"),
    //        };

    //        using var response = await _client.SendAsync(request);

    //        response.EnsureSuccessStatusCode();

    //#pragma warning disable CS8603 // Possible null reference return.
    //        return await JsonSerializer.DeserializeAsync<PaginatedList<TestItem>>(await response.Content.ReadAsStreamAsync(), options: new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    //#pragma warning restore CS8603 // Possible null reference return.

    //    }
}
