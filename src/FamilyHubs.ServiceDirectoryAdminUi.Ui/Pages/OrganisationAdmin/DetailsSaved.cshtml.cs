using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

public class DetailsSavedModel : PageModel
{
    private readonly ISessionService _session;

    public DetailsSavedModel(ISessionService sessionService)
    {
        _session = sessionService;
    }
    public void OnGet()
    {
        _session.StoreCurrentPageName(HttpContext, "DetailsSaved"); //TODO - replace page names with consts
    }

    public IActionResult OnPost()
    {
        return RedirectToPage("/OrganisationAdmin/Welcome");
    }
}
