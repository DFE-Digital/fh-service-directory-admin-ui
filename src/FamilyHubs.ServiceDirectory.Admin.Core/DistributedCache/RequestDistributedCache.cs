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

    public async Task<SubjectAccessRequestViewModel?> GetSarAsync(string emailAddress)
    {
        return await _distributedCache.GetAsync<SubjectAccessRequestViewModel>($"{emailAddress}-SAR");
    }

    public async Task SetSarAsync(string emailAddress, SubjectAccessRequestViewModel model)
    {
        await _distributedCache.SetAsync($"{emailAddress}-SAR", model, _distributedCacheEntryOptions);
    }

    public async Task<string?> GetCurrentPageAsync(string emailAddress)
    {
        return await _distributedCache.GetAsync<string>($"{emailAddress}-currentpage");
    }

    public async Task<string?> GetLastPageAsync(string emailAddress)
    {
        return await _distributedCache.GetAsync<string>($"{emailAddress}-lastpage");
    }

    public async Task SetPageAsync(string emailAddress, string page)
    {
        if (string.IsNullOrEmpty(emailAddress) || string.IsNullOrEmpty(page))
            return;

        string? currentPage = await _distributedCache.GetAsync<string>($"{emailAddress}-currentpage");
        if (!string.IsNullOrEmpty(currentPage) && string.Compare(page,currentPage,StringComparison.OrdinalIgnoreCase) != 0)
        {
            await _distributedCache.SetAsync($"{emailAddress}-lastpage", currentPage, _distributedCacheEntryOptions);
        }

        await _distributedCache.SetAsync($"{emailAddress}-currentpage", page, _distributedCacheEntryOptions);
    }

    public async Task RemoveAsync(string emailAddress)
    {
        await _distributedCache.RemoveAsync(emailAddress);
    }
}