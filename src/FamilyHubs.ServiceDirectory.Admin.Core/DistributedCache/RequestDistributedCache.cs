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

    public Task<T?> GetAsync<T>(string emailAddress)
    {
        return _distributedCache.GetAsync<T>(GetKey<T>(emailAddress));
    }

    public async Task<T> SetAsync<T>(string emailAddress, T model)
    {
        await _distributedCache.SetAsync(GetKey<T>(emailAddress), model, _distributedCacheEntryOptions);
        return model;
    }

    public Task RemoveAsync<T>(string emailAddress)
    {
        return _distributedCache.RemoveAsync(GetKey<T>(emailAddress));
    }
}