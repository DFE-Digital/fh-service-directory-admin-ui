using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FamilyHubs.ServiceDirectory.Admin.Core.Exceptions;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.ServiceDirectory.Shared.Models;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;

public interface IServiceDirectoryClient
{
    Task<PaginatedList<TaxonomyDto>> GetTaxonomyList(int pageNumber = 1, int pageSize = 10, TaxonomyType taxonomyType = TaxonomyType.NotSet);

    Task<List<OrganisationDto>> GetOrganisations(CancellationToken cancellationToken = default);
    Task<List<OrganisationDto>> GetCachedLaOrganisations(CancellationToken cancellationToken = default);
    Task<List<OrganisationDto>> GetCachedVcsOrganisations(long laOrganisationId, CancellationToken cancellationToken = default);

    Task<OrganisationWithServicesDto?> GetOrganisationById(long id);
    Task<long> CreateOrganisation(OrganisationWithServicesDto organisation);
    Task<long> CreateService(ServiceDto service);
    Task<long> UpdateService(ServiceDto service);
    Task<ServiceDto> GetServiceById(long id);
    Task<List<ServiceDto>> GetServicesByOrganisationId(long id);
    Task<bool> DeleteServiceById(long id);
}

public class ServiceDirectoryClient : ApiService, IServiceDirectoryClient
{
    private readonly ICacheService _cacheService;

    public ServiceDirectoryClient(HttpClient client, ICacheService cacheService)
        : base(client)
    {
        _cacheService = cacheService;
    }

    public async Task<PaginatedList<TaxonomyDto>> GetTaxonomyList(int pageNumber = 1, int pageSize = 10,
        TaxonomyType taxonomyType = TaxonomyType.ServiceCategory)
    {
        var request = new HttpRequestMessage();
        request.Method = HttpMethod.Get;
        request.RequestUri = new Uri(Client.BaseAddress +
                                     $"api/taxonomies?pageNumber={pageNumber}&pageSize={pageSize}&taxonomyType={taxonomyType}");

        using var response = await Client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        return await JsonSerializer.DeserializeAsync<PaginatedList<TaxonomyDto>>(
            await response.Content.ReadAsStreamAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new PaginatedList<TaxonomyDto>();
    }

    private async Task<List<OrganisationDto>> GetCachedOrganisationsInternal(CancellationToken cancellationToken = default)
    {
        var semaphore = new SemaphoreSlim(1, 1);
        var organisations = await _cacheService.GetOrganisations();
        if (organisations is not null)
            return organisations;
        try
        {
            await semaphore.WaitAsync(cancellationToken);

            // recheck to make sure it didn't populate before entering semaphore
            organisations = await _cacheService.GetOrganisations();
            if (organisations is not null)
                return organisations;

            organisations = await GetOrganisations(cancellationToken);


            await _cacheService.StoreOrganisations(organisations);

            return organisations;
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task<List<OrganisationDto>> GetOrganisations(CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage();
        request.Method = HttpMethod.Get;
        request.RequestUri = new Uri(Client.BaseAddress + "api/organisations");

        using var response = await Client.SendAsync(request, cancellationToken);

        response.EnsureSuccessStatusCode();

        var organisations = await JsonSerializer.DeserializeAsync<List<OrganisationDto>>(
                                await response.Content.ReadAsStreamAsync(cancellationToken),
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, cancellationToken) ??
                            new List<OrganisationDto>();
        
        return organisations;
    }

    public async Task<List<OrganisationDto>> GetCachedLaOrganisations(CancellationToken cancellationToken = default)
    {
        var organisations = await GetCachedOrganisationsInternal(cancellationToken);

        var laOrganisations = organisations.Where(x => x.OrganisationType == OrganisationType.LA).ToList();

        return laOrganisations;
    }

    public async Task<List<OrganisationDto>> GetCachedVcsOrganisations(long laOrganisationId, CancellationToken cancellationToken = default)
    {
        // recheck to make sure it didn't populate before entering semaphore
        var organisations = await GetCachedOrganisationsInternal(cancellationToken);

        var vcsOrganisations = organisations.Where(x => x.OrganisationType == OrganisationType.VCFS && x.AssociatedOrganisationId == laOrganisationId).ToList();

        return vcsOrganisations;
    }

    public async Task<OrganisationWithServicesDto?> GetOrganisationById(long id)
    {
        var request = new HttpRequestMessage();
        request.Method = HttpMethod.Get;
        request.RequestUri = new Uri(Client.BaseAddress + $"api/organisations/{id}");

        using var response = await Client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        return await JsonSerializer.DeserializeAsync<OrganisationWithServicesDto>(
            await response.Content.ReadAsStreamAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<long> CreateOrganisation(OrganisationWithServicesDto organisation)
    {
        var request = new HttpRequestMessage();
        request.Method = HttpMethod.Post;
        request.RequestUri = new Uri(Client.BaseAddress + "api/organisations");
        request.Content =
            new StringContent(JsonConvert.SerializeObject(organisation), Encoding.UTF8, "application/json");

        using var response = await Client.SendAsync(request);
        await _cacheService.ResetOrganisations();
        await ValidateResponse(response);

        var stringResult = await response.Content.ReadAsStringAsync();
        return long.Parse(stringResult);
    }

    public async Task<long> UpdateOrganisation(OrganisationWithServicesDto organisation)
    {
        var request = new HttpRequestMessage();
        request.Method = HttpMethod.Put;
        request.RequestUri = new Uri(Client.BaseAddress + $"api/organisations/{organisation.Id}");
        request.Content =
            new StringContent(JsonConvert.SerializeObject(organisation), Encoding.UTF8, "application/json");

        using var response = await Client.SendAsync(request);
        await _cacheService.ResetOrganisations();

        await ValidateResponse(response);

        var stringResult = await response.Content.ReadAsStringAsync();
        return long.Parse(stringResult);
    }

    public async Task<long> CreateService(ServiceDto service)
    {
        var request = new HttpRequestMessage();
        request.Method = HttpMethod.Post;
        request.RequestUri = new Uri(Client.BaseAddress + "api/services");
        request.Content = new StringContent(JsonConvert.SerializeObject(service), Encoding.UTF8, "application/json");

        using var response = await Client.SendAsync(request);

        await ValidateResponse(response);

        var stringResult = await response.Content.ReadAsStringAsync();
        return long.Parse(stringResult);
    }

    public async Task<long> UpdateService(ServiceDto service)
    {
        var request = new HttpRequestMessage();
        request.Method = HttpMethod.Put;
        request.RequestUri = new Uri(Client.BaseAddress + $"api/services/{service.Id}");
        request.Content = new StringContent(JsonConvert.SerializeObject(service), Encoding.UTF8, "application/json");

        using var response = await Client.SendAsync(request);

        await ValidateResponse(response);

        var stringResult = await response.Content.ReadAsStringAsync();
        return long.Parse(stringResult);
    }

    public async Task<ServiceDto> GetServiceById(long id)
    {
        if (id == 0) throw new ArgumentOutOfRangeException(nameof(id), id, "Service Id can not be zero");

        var request = new HttpRequestMessage();
        request.Method = HttpMethod.Get;
        request.RequestUri = new Uri(Client.BaseAddress + $"api/services/{id}");

        using var response = await Client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var result = await JsonSerializer.DeserializeAsync<ServiceDto>(await response.Content.ReadAsStreamAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        ArgumentNullException.ThrowIfNull(result);

        return result;
    }

    public async Task<List<ServiceDto>> GetServicesByOrganisationId(long id)
    {
        var request = new HttpRequestMessage();
        request.Method = HttpMethod.Get;
        request.RequestUri = new Uri(Client.BaseAddress + $"api/organisationservices/{id}");

        using var response = await Client.SendAsync(request);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return new List<ServiceDto>();

        response.EnsureSuccessStatusCode();

        return await JsonSerializer.DeserializeAsync<List<ServiceDto>>(await response.Content.ReadAsStreamAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ServiceDto>();
    }

    private static async Task ValidateResponse(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            // TODO : handle failures without throwing errors
            var failure = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
            if (failure != null)
            {
                throw new ApiException(failure);
            }

            response.EnsureSuccessStatusCode();
        }
    }

    public async Task<bool> DeleteServiceById(long id)
    {
        var request = new HttpRequestMessage();
        request.Method = HttpMethod.Delete;
        request.RequestUri = new Uri(Client.BaseAddress + $"api/services/{id}");

        using var response = await Client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var retVal = await JsonSerializer.DeserializeAsync<bool>(await response.Content.ReadAsStreamAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        ArgumentNullException.ThrowIfNull(retVal, nameof(retVal));

        return retVal;
    }
}