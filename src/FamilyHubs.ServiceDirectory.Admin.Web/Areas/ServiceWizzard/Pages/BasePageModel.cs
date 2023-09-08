using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.SharedKernel.Identity;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.ServiceWizzard.Pages;

public abstract class BasePageModel : HeaderPageModel
{
    protected readonly IRequestDistributedCache _requestCache;
    protected BasePageModel(IRequestDistributedCache requestCache)
    {
        _requestCache = requestCache;
    }
    protected async Task<OrganisationViewModel?> GetOrganisationViewModel()
    {
        var user = HttpContext.GetFamilyHubsUser();
        string? currentPageName = PageContext.ActionDescriptor.DisplayName;
        if (!string.IsNullOrEmpty(currentPageName))
            await _requestCache.SetPageAsync(user.Email, currentPageName);
        return await _requestCache.GetAsync(user.Email);
    }

    protected async Task SetCacheAsync(OrganisationViewModel viewModel)
    {
        var user = HttpContext.GetFamilyHubsUser();
        await _requestCache.SetAsync(user.Email, viewModel);
    }

    protected async Task<string?> GetLastPage()
    {
        var user = HttpContext.GetFamilyHubsUser();
        return await _requestCache.GetLastPageAsync(user.Email);
    }
}
