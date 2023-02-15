using System.Text;
using System.Text.Json;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.SharedKernel;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;

public interface IOrganisationAdminClientService
{
    Task<PaginatedList<TaxonomyDto>> GetTaxonomyList(int pageNumber = 1, int pageSize = 10, TaxonomyType taxonomyType = TaxonomyType.NotSet);
    Task<List<OrganisationDto>> GetListOrganisations();
    Task<OrganisationWithServicesDto> GetOrganisationById(string id);
    Task<string> CreateOrganisation(OrganisationWithServicesDto organisation);
    Task<string> UpdateOrganisation(OrganisationWithServicesDto organisation);
    Task<string> CreateService(ServiceDto service);
    Task<string> UpdateService(ServiceDto service);
}

public class OrganisationAdminClientService : ApiService, IOrganisationAdminClientService
{
    public OrganisationAdminClientService(HttpClient client)
    : base(client)
    {

    }

    public async Task<PaginatedList<TaxonomyDto>> GetTaxonomyList(int pageNumber = 1, int pageSize = 10, TaxonomyType taxonomyType = TaxonomyType.ServiceCategory)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(_client.BaseAddress + $"api/taxonomies?pageNumber={pageNumber}&pageSize={pageSize}&taxonomyType={taxonomyType}"),
        };

        using var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        return await JsonSerializer.DeserializeAsync<PaginatedList<TaxonomyDto>>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new PaginatedList<TaxonomyDto>();

    }

    public async Task<List<OrganisationDto>> GetListOrganisations()
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(_client.BaseAddress + "api/organizations"),

        };

        using var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        return await JsonSerializer.DeserializeAsync<List<OrganisationDto>>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<OrganisationDto>();

    }

    public async Task<OrganisationWithServicesDto> GetOrganisationById(string id)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(_client.BaseAddress + $"api/organizations/{id}"),

        };

        using var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();


        return await JsonSerializer.DeserializeAsync<OrganisationWithServicesDto>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new OrganisationWithServicesDto(
            Guid.NewGuid().ToString(),
            default!
            , ""
            );
    }

    public async Task<string> CreateOrganisation(OrganisationWithServicesDto organisation)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(_client.BaseAddress + "api/organizations"),
            Content = new StringContent(JsonConvert.SerializeObject(organisation), Encoding.UTF8, "application/json"),
        };

        using var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var stringResult = await response.Content.ReadAsStringAsync();
        return stringResult;
    }

    public async Task<string> UpdateOrganisation(OrganisationWithServicesDto organisation)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Put,
            RequestUri = new Uri(_client.BaseAddress + $"api/organizations/{organisation.Id}"),
            Content = new StringContent(JsonConvert.SerializeObject(organisation), Encoding.UTF8, "application/json"),
        };

        using var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var stringResult = await response.Content.ReadAsStringAsync();
        return stringResult;
    }

    public async Task<string> CreateService(ServiceDto service)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(_client.BaseAddress + "api/services"),
            Content = new StringContent(JsonConvert.SerializeObject(service), Encoding.UTF8, "application/json"),
        };

        using var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var stringResult = await response.Content.ReadAsStringAsync();
        return stringResult;
    }

    public async Task<string> UpdateService(ServiceDto service)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Put,
            RequestUri = new Uri(_client.BaseAddress + $"api/services/{service.Id}"),
            Content = new StringContent(JsonConvert.SerializeObject(service), Encoding.UTF8, "application/json"),
        };

        using var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var stringResult = await response.Content.ReadAsStringAsync();
        return stringResult;
    }
}
