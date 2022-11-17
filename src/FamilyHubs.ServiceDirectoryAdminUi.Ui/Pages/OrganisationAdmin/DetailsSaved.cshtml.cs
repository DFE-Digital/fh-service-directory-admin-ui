using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

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
        return RedirectToPage("/OrganisationAdmin/Welcome");
    }
}
