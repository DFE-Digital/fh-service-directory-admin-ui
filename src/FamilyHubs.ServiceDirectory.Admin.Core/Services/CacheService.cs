using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using Microsoft.Extensions.Caching.Distributed;

namespace FamilyHubs.ServiceDirectory.Admin.Core.Services;

public interface ICacheService
{
    public Task<OrganisationViewModel?> RetrieveOrganisationWithService();
    public Task StoreOrganisationWithService(OrganisationViewModel? vm);
    public Task ResetOrganisationWithService();

    public Task<string> RetrieveUserFlow();
    public Task StoreUserFlow(string userFlow);

    public Task<string> RetrieveLastPageName();
    public Task StoreCurrentPageName(string? currentPage);
    public Task ResetLastPageName();
    Task StorePermissionModel(PermissionModel permissionModel);
    Task<PermissionModel?> GetPermissionModel();
    void ResetPermissionModel();
    Task<List<OrganisationDto>?> GetOrganisations();
    Task StoreOrganisations(List<OrganisationDto> localAuthorities);
}

public class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ICacheKeys _cacheKeys;
    private readonly DistributedCacheEntryOptions _distributedCacheEntryOptions;

    public CacheService(IDistributedCache cache, ICacheKeys cacheKeys,
        DistributedCacheEntryOptions distributedCacheEntryOptions)
    {
        _cache = cache;
        _cacheKeys = cacheKeys;
        _distributedCacheEntryOptions = distributedCacheEntryOptions;
    }

    public async Task<OrganisationViewModel?> RetrieveOrganisationWithService()
    {
        return await _cache.GetAsync<OrganisationViewModel>(_cacheKeys.KeyOrgWithService);
    }

    public async Task StoreOrganisationWithService(OrganisationViewModel? vm)
    {
        if (vm != null)
            await _cache.SetAsync(_cacheKeys.KeyOrgWithService, vm, _distributedCacheEntryOptions);
    }

    public async Task ResetOrganisationWithService()
    {
        await _cache.RemoveAsync(_cacheKeys.KeyOrgWithService);
    }

    public async Task<string> RetrieveLastPageName()
    {
        return await _cache.GetAsync<string>(_cacheKeys.KeyCurrentPage) ?? string.Empty;
    }

    public async Task StoreCurrentPageName(string? currentPage)
    {
        if (currentPage != null)
            await _cache.SetAsync(_cacheKeys.KeyCurrentPage, currentPage, _distributedCacheEntryOptions);
    }

    public async Task ResetLastPageName()
    {
        await _cache.RemoveAsync(_cacheKeys.KeyCurrentPage);
    }

    public async Task StoreService(ServiceDto serviceDto)
    {
        await _cache.SetAsync(_cacheKeys.KeyService, serviceDto, _distributedCacheEntryOptions);
    }

    public async Task<string> RetrieveUserFlow()
    {
        return (await _cache.GetAsync<string>(_cacheKeys.KeyUserFlow)) ?? string.Empty;
    }

    public async Task StoreUserFlow(string userFlow)
    {
        await _cache.SetAsync(_cacheKeys.KeyUserFlow, userFlow, _distributedCacheEntryOptions);
    }

    public async Task StorePermissionModel(PermissionModel permissionModel)
    {
        await _cache.SetAsync(_cacheKeys.KeyUserPermission, permissionModel, _distributedCacheEntryOptions);
    }

    public async Task<PermissionModel?> GetPermissionModel()
    {
        return await _cache.GetAsync<PermissionModel?>(_cacheKeys.KeyUserPermission);
    }

    public void ResetPermissionModel()
    {
        _cache.RemoveAsync(_cacheKeys.KeyUserPermission);
    }

    public async Task<List<OrganisationDto>?> GetOrganisations()
    {
        return await _cache.GetAsync<List<OrganisationDto>?>(_cacheKeys.KeyLaOrganisations);
    }

    public async Task StoreOrganisations(List<OrganisationDto> localAuthorities)
    {
        await _cache.SetAsync(_cacheKeys.KeyLaOrganisations, localAuthorities, _distributedCacheEntryOptions);
    }
}