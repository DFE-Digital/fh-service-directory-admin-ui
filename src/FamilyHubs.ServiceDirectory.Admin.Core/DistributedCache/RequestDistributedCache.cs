using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.SharedKernel.Razor.DistributedCache;
using Microsoft.Extensions.Caching.Distributed;

namespace FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;

public class RequestDistributedCache : IRequestDistributedCache
{
    private readonly IDistributedCache _distributedCache;
    private readonly DistributedCacheEntryOptions _distributedCacheEntryOptions;

    public RequestDistributedCache(
        IDistributedCache distributedCache,
        DistributedCacheEntryOptions distributedCacheEntryOptions)
    {
        _distributedCache = distributedCache;
        _distributedCacheEntryOptions = distributedCacheEntryOptions;
    }

    public async Task<OrganisationViewModel?> GetAsync(string emailAddress)
    {
        return await _distributedCache.GetAsync<OrganisationViewModel>(emailAddress);
    }

    public async Task SetAsync(string emailAddress, OrganisationViewModel model)
    {
        await _distributedCache.SetAsync(emailAddress, model, _distributedCacheEntryOptions);
    }

    //todo: replace these with generic versions
    public async Task<ServiceModel?> GetServiceAsync(string emailAddress)
    {
        return await _distributedCache.GetAsync<ServiceModel>(emailAddress);
    }

    public async Task SetServiceAsync(string emailAddress, ServiceModel model)
    {
        await _distributedCache.SetAsync(emailAddress, model, _distributedCacheEntryOptions);
    }

    //todo: we will need a reset too

    public async Task<SubjectAccessRequestViewModel?> GetSarAsync(string emailAddress)
    {
        return await _distributedCache.GetAsync<SubjectAccessRequestViewModel>($"{emailAddress}-SAR");
    }

    public async Task SetSarAsync(string emailAddress, SubjectAccessRequestViewModel model)
    {
        await _distributedCache.SetAsync($"{emailAddress}-SAR", model, _distributedCacheEntryOptions);
    }
}