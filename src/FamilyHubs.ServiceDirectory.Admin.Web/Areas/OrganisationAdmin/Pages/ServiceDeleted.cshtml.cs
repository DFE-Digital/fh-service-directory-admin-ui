using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.OrganisationAdmin.Pages;

public class ServiceDeletedModel : PageModel
{
    private readonly ICacheService _cacheService;

    public ServiceDeletedModel(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }
    
    public async Task<IActionResult> OnPost()
    {
        var organisation = await _cacheService.RetrieveOrganisationWithService();
        return RedirectToPage("/OrganisationAdmin/Welcome", new { organisationId = organisation?.Id });
    }
}
