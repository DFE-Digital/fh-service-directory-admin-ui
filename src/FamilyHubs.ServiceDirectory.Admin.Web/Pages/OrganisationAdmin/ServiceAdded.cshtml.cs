using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.OrganisationAdmin;

public class ServiceAddedModel : PageModel
{
    private readonly IRedisCacheService _redis;

    public ServiceAddedModel(
        IRedisCacheService redisCacheService)
    {
        _redis = redisCacheService;
    }
    public void OnGet()
    {
        _redis.StoreCurrentPageName("ServiceAdded"); //TODO - replace page names with const
    }

    public IActionResult OnPost()
    {
        var organisation = _redis.RetrieveOrganisationWithService();
        return RedirectToPage("/OrganisationAdmin/Welcome", new { organisationId = organisation?.Id });
    }
}