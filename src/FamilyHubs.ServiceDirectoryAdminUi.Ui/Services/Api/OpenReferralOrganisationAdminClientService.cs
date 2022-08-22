using Application.Common.Models;
using LAHub.Domain.RecordEntities;
using SFA.DAS.HashingService;
using System.Text;
using System.Text.Json;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;

public interface IOpenReferralOrganisationAdminClientService
{
    Task<PaginatedList<OpenReferralTaxonomyRecord>> GetTaxonomyList(int pageNumber = 1, int pageSize = 10);
    Task<List<OpenReferralOrganisationRecord>> GetListOpenReferralOrganisations();
    Task<OpenReferralOrganisationWithServicesRecord> GetOpenReferralOrganisationById(string id);
    Task<string> CreateOrganisation(OpenReferralOrganisationWithServicesRecord organisation);
    Task<string> UpdateOrganisation(OpenReferralOrganisationWithServicesRecord organisation);
}

public class OpenReferralOrganisationAdminClientService : ApiService, IOpenReferralOrganisationAdminClientService
{
    public OpenReferralOrganisationAdminClientService(HttpClient client, IHashingService hashingService)
    : base(client, hashingService)
    {

    }

    public async Task<PaginatedList<OpenReferralTaxonomyRecord>> GetTaxonomyList(int pageNumber = 1, int pageSize = 10)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(_client.BaseAddress + $"api/taxonomies?pageNumber={pageNumber}&pageSize={pageSize}"),
        };

        using var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        return await JsonSerializer.DeserializeAsync<PaginatedList<OpenReferralTaxonomyRecord>>(await response.Content.ReadAsStreamAsync(), options: new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new PaginatedList<OpenReferralTaxonomyRecord>();

    }

    public async Task<List<OpenReferralOrganisationRecord>> GetListOpenReferralOrganisations()
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(_client.BaseAddress + "api/organizations"),

        };

        using var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        return await JsonSerializer.DeserializeAsync<List<OpenReferralOrganisationRecord>>(await response.Content.ReadAsStreamAsync(), options: new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<OpenReferralOrganisationRecord>();

    }

    public async Task<OpenReferralOrganisationWithServicesRecord> GetOpenReferralOrganisationById(string id)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(_client.BaseAddress + $"api/organizations/{id}"),

        };

        using var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();


        return await JsonSerializer.DeserializeAsync<OpenReferralOrganisationWithServicesRecord>(await response.Content.ReadAsStreamAsync(), options: new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new OpenReferralOrganisationWithServicesRecord(
            Guid.NewGuid().ToString()
            , ""
            , null
            , null
            , null
            , null
            , null
            );
    }

    public async Task<string> CreateOrganisation(OpenReferralOrganisationWithServicesRecord organisation)
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

    public async Task<string> UpdateOrganisation(OpenReferralOrganisationWithServicesRecord organisation)
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
}
