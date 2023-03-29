using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.OrganisationAdmin;

public class DetailsSavedModel : PageModel
{
    private readonly IRedisCacheService _redis;

    public DetailsSavedModel(IRedisCacheService redis)
    {
        _redis = redis;
    }
    public void OnGet()
    {   
        _redis.StoreCurrentPageName("DetailsSaved"); //TODO - replace page names with const
    }

    public IActionResult OnPost()
    {
        var organisation = _redis.RetrieveOrganisationWithService();
        return RedirectToPage("/OrganisationAdmin/Welcome", new
        {
            organisationId = organisation?.Id
        });
    }
}
