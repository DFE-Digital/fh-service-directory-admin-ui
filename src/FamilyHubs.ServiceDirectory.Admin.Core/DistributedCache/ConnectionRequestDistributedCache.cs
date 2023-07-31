using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.SharedKernel.Razor.DistributedCache;
using Microsoft.Extensions.Caching.Distributed;

namespace FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;

public class ConnectionRequestDistributedCache : IConnectionRequestDistributedCache
{
    private readonly IDistributedCache _distributedCache;
    private readonly DistributedCacheEntryOptions _distributedCacheEntryOptions;

    public ConnectionRequestDistributedCache(
        IDistributedCache distributedCache,
        DistributedCacheEntryOptions distributedCacheEntryOptions)
    {
        _distributedCache = distributedCache;
        _distributedCacheEntryOptions = distributedCacheEntryOptions;
    }

    public async Task<ConnectionRequestModel?> GetAsync(string professionalsEmail)
    {
        return await _distributedCache.GetAsync<ConnectionRequestModel>(professionalsEmail);
    }

    public async Task SetAsync(string professionalsEmail, ConnectionRequestModel model)
    {
        await _distributedCache.SetAsync(professionalsEmail, model, _distributedCacheEntryOptions);
    }

    public async Task RemoveAsync(string professionalsEmail)
    {
        await _distributedCache.RemoveAsync(professionalsEmail);
    }
}