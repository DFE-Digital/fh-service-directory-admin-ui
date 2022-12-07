using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralOrganisations;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServices;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralTaxonomys;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.SharedKernel;
using System.Text;
using System.Text.Json;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;

public interface IOpenReferralOrganisationAdminClientService
{
    Task<PaginatedList<OpenReferralTaxonomyDto>> GetTaxonomyList(int pageNumber = 1, int pageSize = 10);
    Task<List<OpenReferralOrganisationDto>> GetListOpenReferralOrganisations();
    Task<OpenReferralOrganisationWithServicesDto> GetOpenReferralOrganisationById(string id);
    Task<string> CreateOrganisation(OpenReferralOrganisationWithServicesDto organisation);
    Task<string> UpdateOrganisation(OpenReferralOrganisationWithServicesDto organisation);
    Task<string> CreateService(OpenReferralServiceDto service);
    Task<string> UpdateService(OpenReferralServiceDto service);
}

public class OpenReferralOrganisationAdminClientService : ApiService, IOpenReferralOrganisationAdminClientService
{
    public OpenReferralOrganisationAdminClientService(HttpClient client)
    : base(client)
    {

    }

    public async Task<PaginatedList<OpenReferralTaxonomyDto>> GetTaxonomyList(int pageNumber = 1, int pageSize = 10)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(_client.BaseAddress + $"api/taxonomies?pageNumber={pageNumber}&pageSize={pageSize}"),
        };

        using var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        return await JsonSerializer.DeserializeAsync<PaginatedList<OpenReferralTaxonomyDto>>(await response.Content.ReadAsStreamAsync(), options: new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new PaginatedList<OpenReferralTaxonomyDto>();

    }

    public async Task<List<OpenReferralOrganisationDto>> GetListOpenReferralOrganisations()
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(_client.BaseAddress + "api/organizations"),

        };

        using var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        return await JsonSerializer.DeserializeAsync<List<OpenReferralOrganisationDto>>(await response.Content.ReadAsStreamAsync(), options: new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<OpenReferralOrganisationDto>();

    }

    public async Task<OpenReferralOrganisationWithServicesDto> GetOpenReferralOrganisationById(string id)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(_client.BaseAddress + $"api/organizations/{id}"),

        };

        using var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();


        return await JsonSerializer.DeserializeAsync<OpenReferralOrganisationWithServicesDto>(await response.Content.ReadAsStreamAsync(), options: new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new OpenReferralOrganisationWithServicesDto(
            Guid.NewGuid().ToString(),
            default!
            , ""
            , null
            , null
            , null
            , null
            , null
            );
    }

    public async Task<string> CreateOrganisation(OpenReferralOrganisationWithServicesDto organisation)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(_client.BaseAddress + "api/organizations"),
            Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(organisation), Encoding.UTF8, "application/json"),
        };

        using var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var stringResult = await response.Content.ReadAsStringAsync();
        return stringResult;
    }

    public async Task<string> UpdateOrganisation(OpenReferralOrganisationWithServicesDto organisation)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Put,
            RequestUri = new Uri(_client.BaseAddress + $"api/organizations/{organisation.Id}"),
            Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(organisation), Encoding.UTF8, "application/json"),
        };

        using var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var stringResult = await response.Content.ReadAsStringAsync();
        return stringResult;
    }

    public async Task<string> CreateService(OpenReferralServiceDto service)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(_client.BaseAddress + "api/services"),
            Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(service), Encoding.UTF8, "application/json"),
        };

        using var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var stringResult = await response.Content.ReadAsStringAsync();
        return stringResult;
    }

    public async Task<string> UpdateService(OpenReferralServiceDto service)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Put,
            RequestUri = new Uri(_client.BaseAddress + $"api/services/{service.Id}"),
            Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(service), Encoding.UTF8, "application/json"),
        };

        using var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var stringResult = await response.Content.ReadAsStringAsync();
        return stringResult;
    }
}
