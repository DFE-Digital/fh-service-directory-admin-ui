using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.OrganisationAdmin;

public class DetailsSavedModel : PageModel
{
    private readonly ICacheService _cacheService;

    public DetailsSavedModel(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }
    public void OnGet()
    {   
        _cacheService.StoreCurrentPageName("DetailsSaved"); //TODO - replace page names with const
    }

    public IActionResult OnPost()
    {
        var organisation = _cacheService.RetrieveOrganisationWithService();
        return RedirectToPage("/OrganisationAdmin/Welcome", new
        {
            organisationId = organisation?.Id
        });
    }
}
