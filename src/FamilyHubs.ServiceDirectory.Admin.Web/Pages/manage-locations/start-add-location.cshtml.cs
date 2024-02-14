using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_locations;

public class start_add_locationModel : PageModel
{
    private readonly IRequestDistributedCache _cache;

    public start_add_locationModel(IRequestDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var familyHubsUser = HttpContext.GetFamilyHubsUser();

        // the user's just starting the journey
        await _cache.SetAsync(familyHubsUser.Email, new LocationModel());

        return Redirect(LocationJourneyPageExtensions.GetAddFlowStartPagePath());
    }
}