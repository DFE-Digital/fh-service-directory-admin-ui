using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.OrganisationAdmin;

public class ServiceNotDeletedModel : PageModel
{
    private readonly IRedisCacheService _redis;

    public ServiceNotDeletedModel(IRedisCacheService redisCacheService)
    {
        _redis = redisCacheService;
    }

    public IActionResult OnPost()
    {
        var organisation = _redis.RetrieveOrganisationWithService();
        return RedirectToPage("/OrganisationAdmin/Welcome", new { organisationId = organisation?.Id });
    }
}
