using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FamilyHubs.ServiceDirectory.Admin.Core.Exceptions;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.ServiceDirectory.Shared.Models;
using FamilyHubs.SharedKernel.Exceptions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;

public interface IServiceDirectoryClient
{
    Task<PaginatedList<TaxonomyDto>> GetTaxonomyList(int pageNumber = 1, int pageSize = 10, TaxonomyType taxonomyType = TaxonomyType.NotSet);

    Task<List<OrganisationDto>> GetOrganisations(CancellationToken cancellationToken = default);
    Task<List<OrganisationDto>> GetOrganisationByAssociatedOrganisation(long id);
    Task<List<OrganisationDto>> GetCachedLaOrganisations(CancellationToken cancellationToken = default);
    Task<List<OrganisationDto>> GetCachedVcsOrganisations(long laOrganisationId, CancellationToken cancellationToken = default);
    Task<OrganisationWithServicesDto?> GetOrganisationById(long id);
    Task<Outcome<long, ApiException>> CreateOrganisation(OrganisationWithServicesDto organisation);
    Task<long> UpdateOrganisation(OrganisationWithServicesDto organisation);
    Task<bool> DeleteOrganisation(long id);
    Task<long> CreateService(ServiceDto service);
    Task<long> UpdateService(ServiceDto service);
    Task<ServiceDto> GetServiceById(long id);
    Task<List<ServiceDto>> GetServicesByOrganisationId(long id);
    Task<bool> DeleteServiceById(long id);
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

    public async Task<List<OrganisationDto>> GetOrganisations(CancellationToken cancellationToken)
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

    public async Task<OrganisationWithServicesDto?> GetOrganisationById(long id)
    {
        var request = new HttpRequestMessage();
        request.Method = HttpMethod.Get;
        request.RequestUri = new Uri(Client.BaseAddress + $"api/organisations/{id}");

        using var response = await Client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        Logger.LogInformation($"{nameof(ServiceDirectoryClient)} Returning Organisation");
        return await DeserializeResponse<OrganisationWithServicesDto>(response);
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
        ArgumentNullException.ThrowIfNull(retVal, nameof(retVal));
        Logger.LogInformation($"{nameof(ServiceDirectoryClient)} Organisation Deleted id:{id}");

        return retVal;
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
        Logger.LogInformation($"{nameof(ServiceDirectoryClient)} Service Created id:{stringResult}");
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
        Logger.LogInformation($"{nameof(ServiceDirectoryClient)} Service Updated id:{stringResult}");
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

        var result = await DeserializeResponse<ServiceDto>(response);

        ArgumentNullException.ThrowIfNull(result);

        Logger.LogInformation($"{nameof(ServiceDirectoryClient)} Returning Service Id:{id}");
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

        var services = await DeserializeResponse<List<ServiceDto>>(response) ?? new List<ServiceDto>();

        Logger.LogInformation($"{nameof(ServiceDirectoryClient)} Returning {services.Count} services");

        return services;
    }

    public async Task<bool> DeleteServiceById(long id)
    {
        var request = new HttpRequestMessage();
        request.Method = HttpMethod.Delete;
        request.RequestUri = new Uri(Client.BaseAddress + $"api/services/{id}");

        using var response = await Client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var retVal = await DeserializeResponse<bool>(response);
        ArgumentNullException.ThrowIfNull(retVal, nameof(retVal));

        return retVal;
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

}