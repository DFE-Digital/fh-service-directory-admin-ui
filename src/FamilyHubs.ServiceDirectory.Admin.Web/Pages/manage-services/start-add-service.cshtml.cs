using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public class start_add_serviceModel : PageModel
{
    private readonly IRequestDistributedCache _connectionRequestCache;

    public start_add_serviceModel(IRequestDistributedCache connectionRequestCache)
    {
        _connectionRequestCache = connectionRequestCache;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var familyHubsUser = HttpContext.GetFamilyHubsUser();

        await _connectionRequestCache.RemoveAsync<ServiceModel>(familyHubsUser.Email);

        return Redirect(ServiceJourneyPageExtensions.GetAddFlowStartPagePath());
    }
}