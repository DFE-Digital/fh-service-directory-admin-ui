using FamilyHubs.ServiceDirectory.Admin.Core.Exceptions;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.ServiceDirectory.Shared.Models;
using FamilyHubs.SharedKernel.Exceptions;
using FamilyHubs.SharedKernel.Razor.Dashboard;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Text;
using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient.Exceptions;

namespace FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;

public interface IServiceDirectoryClient
{
    Task<PaginatedList<TaxonomyDto>> GetTaxonomyList(int pageNumber = 1, int pageSize = 10, TaxonomyType taxonomyType = TaxonomyType.ServiceCategory);

    Task<List<OrganisationDto>> GetOrganisations(CancellationToken cancellationToken = default);
    Task<List<OrganisationDto>> GetOrganisationByAssociatedOrganisation(long id);
    //todo: getting data from cache doesn't belong in the service directory client
    Task<List<OrganisationDto>> GetCachedLaOrganisations(CancellationToken cancellationToken = default);
    Task<List<OrganisationDto>> GetCachedVcsOrganisations(long laOrganisationId, CancellationToken cancellationToken = default);
    Task<OrganisationWithServicesDto> GetOrganisationById(long id, CancellationToken cancellationToken = default);
    Task<Outcome<long, ApiException>> CreateOrganisation(OrganisationWithServicesDto organisation);
    Task<long> UpdateOrganisation(OrganisationWithServicesDto organisation);
    Task<bool> DeleteOrganisation(long id);
    Task<long> CreateService(ServiceDto service, CancellationToken cancellationToken = default);
    Task<long> UpdateService(ServiceDto service, CancellationToken cancellationToken = default);
    Task<ServiceDto> GetServiceById(long id, CancellationToken cancellationToken = default);

    Task<PaginatedList<ServiceNameDto>> GetServiceSummaries(
        long? organisationId = null,
        string? serviceNameSearch = null,
        int pageNumber = 1,
        int pageSize = 10,
        SortOrder sortOrder = SortOrder.ascending,
        CancellationToken cancellationToken = default);

    Task<LocationDto> GetLocationById(long id, CancellationToken cancellationToken = default);
    Task<long> CreateLocation(LocationDto location, CancellationToken cancellationToken = default);
    Task<long> UpdateLocation(LocationDto location, CancellationToken cancellationToken = default);
    Task<PaginatedList<LocationDto>> GetLocations(bool? isAscending, string orderByColumn, string? searchName, bool? isFamilyHub, int pageNumber = 1, int pageSize = 10,  CancellationToken cancellationToken = default);
    Task<PaginatedList<LocationDto>> GetLocationsByOrganisationId(long organisationId,  bool? isAscending, string orderByColumn, string? searchName, bool? isFamilyHub, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);
}

public class ServiceDirectoryClient : ApiService<ServiceDirectoryClient>, IServiceDirectoryClient
{
    private readonly ICacheService _cacheService;

    public ServiceDirectoryClient(HttpClient client, ICacheService cacheService, ILogger<ServiceDirectoryClient> logger)
        : base(client, logger)
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

        var results = await DeserializeResponse<PaginatedList<TaxonomyDto>>(response) ?? new PaginatedList<TaxonomyDto>();

        Logger.LogInformation($"{nameof(ServiceDirectoryClient)} Returning {results.TotalCount} Taxonomies");

        return results;
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

    public async Task<List<OrganisationDto>> GetOrganisations(CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage();
        request.Method = HttpMethod.Get;
        request.RequestUri = new Uri(Client.BaseAddress + "api/organisations");

        using var response = await Client.SendAsync(request, cancellationToken);

        response.EnsureSuccessStatusCode();

        var organisations = await DeserializeResponse<List<OrganisationDto>>(response, cancellationToken) ?? new List<OrganisationDto>();

        Logger.LogInformation($"{nameof(ServiceDirectoryClient)} Returning  {organisations.Count} Organisations");

        return organisations;
    }

    public async Task<List<OrganisationDto>> GetOrganisationByAssociatedOrganisation(long id)
    {
        var request = new HttpRequestMessage();
        request.Method = HttpMethod.Get;
        request.RequestUri = new Uri(Client.BaseAddress + $"api/organisationsByAssociatedOrganisation?id={id}");

        using var response = await Client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var organisations = await DeserializeResponse<List<OrganisationDto>>(response) ?? new List<OrganisationDto>();

        Logger.LogInformation($"{nameof(ServiceDirectoryClient)} Returning  {organisations.Count} Associated Organisations");

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

    public async Task<OrganisationWithServicesDto> GetOrganisationById(long id, CancellationToken cancellationToken = default)
    {
        using var response = await Client.GetAsync($"{Client.BaseAddress}api/organisations/{id}", cancellationToken);

        return await Read<OrganisationWithServicesDto>(response, cancellationToken);
    }

    public async Task<Outcome<long, ApiException>> CreateOrganisation(OrganisationWithServicesDto organisation)
    {
        var request = new HttpRequestMessage();
        request.Method = HttpMethod.Post;
        request.RequestUri = new Uri(Client.BaseAddress + "api/organisations");
        request.Content =
            new StringContent(JsonConvert.SerializeObject(organisation), Encoding.UTF8, "application/json");

        using var response = await Client.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            await _cacheService.ResetOrganisations();
            var stringResult = await response.Content.ReadAsStringAsync();
            Logger.LogInformation($"{nameof(ServiceDirectoryClient)} Organisation Created id:{stringResult}");
            return new Outcome<long, ApiException>(long.Parse(stringResult));
        }

        var failure = await response.Content.ReadFromJsonAsync<ApiExceptionResponse<ValidationError>>();
        if (failure != null)
        {
            Logger.LogWarning("Failed to add Organisation {@apiExceptionResponse}", failure);
            return new Outcome<long, ApiException>(new ApiException(failure));
        }

        Logger.LogError("Response from api failed with an unknown response body {statusCode}", response.StatusCode);
        var unhandledException = new ApiExceptionResponse<ValidationError>
        {
            Title = "Failed to add Organisation",
            Detail = "Response from api failed with an unknown response body",
            StatusCode = (int)response.StatusCode,
            ErrorCode = ErrorCodes.UnhandledException.ParseToCodeString()
        };

        return new Outcome<long, ApiException>(new ApiException(unhandledException));
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
        Logger.LogInformation($"{nameof(ServiceDirectoryClient)} Organisation Updated id:{stringResult}");
        return long.Parse(stringResult);
    }

    public async Task<bool> DeleteOrganisation(long id)
    {
        var request = new HttpRequestMessage();
        request.Method = HttpMethod.Delete;
        request.RequestUri = new Uri(Client.BaseAddress + $"api/organisations/{id}");

        using var response = await Client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var retVal = await DeserializeResponse<bool>(response);
        ArgumentNullException.ThrowIfNull(retVal);
        Logger.LogInformation($"{nameof(ServiceDirectoryClient)} Organisation Deleted id:{id}");

        return retVal;
    }

    public async Task<long> CreateService(ServiceDto service, CancellationToken cancellationToken = default)
    {
        using var response = await Client.PostAsJsonAsync($"{Client.BaseAddress}api/services", service, cancellationToken);

        return await Read<long>(response, cancellationToken);
    }

    public async Task<long> UpdateService(ServiceDto service, CancellationToken cancellationToken = default)
    {
        using var response = await Client.PutAsJsonAsync($"{Client.BaseAddress}api/services/{service.Id}", service, cancellationToken);

        return await Read<long>(response, cancellationToken);
    }

    public async Task<ServiceDto> GetServiceById(long id, CancellationToken cancellationToken = default)
    {
        //todo:
        using var response = await Client.GetAsync($"{Client.BaseAddress}api/services-simple/{id}", cancellationToken);

        return await Read<ServiceDto>(response, cancellationToken);
    }

    public async Task<PaginatedList<ServiceNameDto>> GetServiceSummaries(
        long? organisationId = null,
        string? serviceNameSearch = null,
        int pageNumber = 1,
        int pageSize = 10, 
        SortOrder sortOrder = SortOrder.ascending,
        CancellationToken cancellationToken = default)
    {
        if (sortOrder == SortOrder.none)
            throw new ArgumentOutOfRangeException(nameof(sortOrder), sortOrder, "SortOrder can not be none");

        string endpointUrl = $"{Client.BaseAddress}api/services/summary?pageNumber={pageNumber}&pageSize={pageSize}&sortOrder={sortOrder}";
        if (organisationId != null)
        {
            endpointUrl += $"&organisationId={organisationId}";
        }

        if (serviceNameSearch != null)
        {
            endpointUrl += $"&serviceNameSearch={serviceNameSearch}";
            
        }

        using var response = await Client.GetAsync(endpointUrl, cancellationToken);

        //todo: extension method with generic type on extension (with base extension)
        return await Read<PaginatedList<ServiceNameDto>>(response, cancellationToken);
    }

    private async Task<T> Read<T>(HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        if (!response.IsSuccessStatusCode)
        {
            throw new ServiceDirectoryClientServiceException(response, await response.Content.ReadAsStringAsync(cancellationToken));
        }
        var content = await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);

        if (content is null)
        {
            // the only time it'll be null, is if the API returns "null"
            // (see https://stackoverflow.com/questions/71162382/why-are-the-return-types-of-nets-system-text-json-jsonserializer-deserialize-m)
            // unlikely, but possibly (pass new MemoryStream(Encoding.UTF8.GetBytes("null")) to see it actually return null)
            // note we hard-code passing "null", rather than messing about trying to rewind the stream, as this is such a corner case and we want to let the deserializer take advantage of the async stream (in the happy case)
            throw new ServiceDirectoryClientServiceException(response, "null");
        }

        return content;
    }

    private static async Task ValidateResponse(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            // TODO : handle failures without throwing errors
            var failure = await response.Content.ReadFromJsonAsync<ApiExceptionResponse<ValidationError>>();
            if (failure != null)
            {
                throw new ApiException(failure);
            }

            response.EnsureSuccessStatusCode();
        }
    }

    public async Task<LocationDto> GetLocationById(long id, CancellationToken cancellationToken = default)
    {
        using var response = await Client.GetAsync($"{Client.BaseAddress}api/locations/{id}", cancellationToken);

        return await Read<LocationDto>(response, cancellationToken);
    }

    public async Task<long> CreateLocation(LocationDto location, CancellationToken cancellationToken = default)
    {
        using var response = await Client.PostAsJsonAsync($"{Client.BaseAddress}api/locations", location, cancellationToken);

        return await Read<long>(response, cancellationToken);
    }

    public async Task<long> UpdateLocation(LocationDto location, CancellationToken cancellationToken = default)
    {
        using var response = await Client.PutAsJsonAsync($"{Client.BaseAddress}api/locations/{location.Id}", location, cancellationToken);

        return await Read<long>(response, cancellationToken);
    }

    public async Task<PaginatedList<LocationDto>> GetLocations(bool? isAscending, string orderByColumn, string? searchName, bool? isFamilyHub, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage();
        request.Method = HttpMethod.Get;
        request.RequestUri = new Uri(Client.BaseAddress + $"api/locations?pageNumber={pageNumber}&pageSize={pageSize}&isAscending={isAscending}&orderByColumn={orderByColumn}&searchName={searchName}&isFamilyHub={isFamilyHub}");

        using var response = await Client.SendAsync(request, cancellationToken);

        response.EnsureSuccessStatusCode();

        var locations = await DeserializeResponse<PaginatedList<LocationDto>>(response, cancellationToken) ?? new PaginatedList<LocationDto>();

        Logger.LogInformation($"{nameof(ServiceDirectoryClient)} Returning  {locations.TotalCount} Locations");

        return locations;
    }

    public async Task<PaginatedList<LocationDto>> GetLocationsByOrganisationId(long organisationId, bool? isAscending, string orderByColumn, string? searchName, bool? isFamilyHub, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage();
        request.Method = HttpMethod.Get;
        request.RequestUri = new Uri(Client.BaseAddress + $"api/organisationlocations/{organisationId}?pageNumber={pageNumber}&pageSize={pageSize}&isAscending={isAscending}&orderByColumn={orderByColumn}&searchName={searchName}&isFamilyHub={isFamilyHub}");

        using var response = await Client.SendAsync(request, cancellationToken);

        response.EnsureSuccessStatusCode();

        var locations = await DeserializeResponse<PaginatedList<LocationDto>>(response, cancellationToken) ?? new PaginatedList<LocationDto>();

        Logger.LogInformation($"{nameof(ServiceDirectoryClient)} Returning  {locations.TotalCount} Locations");

        return locations;
    }
}