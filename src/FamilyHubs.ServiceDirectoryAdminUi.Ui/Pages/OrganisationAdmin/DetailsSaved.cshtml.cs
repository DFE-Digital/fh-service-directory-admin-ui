using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

[Authorize(Policy = "ServiceMaintainer")]
public class DetailsSavedModel : PageModel
{
    private readonly ISessionService _session;
    private readonly IRedisCacheService _redis;

    public DetailsSavedModel(ISessionService sessionService, IRedisCacheService redis)
    {
        _session = sessionService;
        _redis = redis;
    }
    public void OnGet()
    {   
        _redis.StoreCurrentPageName("DetailsSaved"); //TODO - replace page names with consts
    }

    public IActionResult OnPost()
    {
        var organisation = _redis.RetrieveOrganisationWithService();
        return RedirectToPage("/OrganisationAdmin/Welcome", new
        {
            organisationId = organisation?.Id,
        });
    }
}
