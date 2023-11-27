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

    private string GetKey<T>(string emailAddress)
    {
        // space is not allowable in an email or typename, so it's safe to use as a separator
        return $"{emailAddress} {nameof(T)}";
    }

    public async Task<T?> GetAsync<T>(string emailAddress)
    {
        return await _distributedCache.GetAsync<T>(GetKey<T>(emailAddress));
    }

    public async Task SetAsync<T>(string emailAddress, T model)
    {
        await _distributedCache.SetAsync(GetKey<T>(emailAddress), model, _distributedCacheEntryOptions);
    }

    //todo: we will need a reset too

    ////todo: replace these with generic versions
    //public async Task<ServiceModel?> GetServiceAsync(string emailAddress)
    //{
    //    return await _distributedCache.GetAsync<ServiceModel>(emailAddress);
    //}

    //public async Task SetServiceAsync(string emailAddress, ServiceModel model)
    //{
    //    await _distributedCache.SetAsync(emailAddress, model, _distributedCacheEntryOptions);
    //}

    ////todo: we will need a reset too

    //public async Task<SubjectAccessRequestViewModel?> GetSarAsync(string emailAddress)
    //{
    //    return await _distributedCache.GetAsync<SubjectAccessRequestViewModel>($"{emailAddress}-SAR");
    //}

    //public async Task SetSarAsync(string emailAddress, SubjectAccessRequestViewModel model)
    //{
    //    await _distributedCache.SetAsync($"{emailAddress}-SAR", model, _distributedCacheEntryOptions);
    //}
}