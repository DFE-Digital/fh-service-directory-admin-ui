using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralOrganisations;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

public class ServiceAddedModel : PageModel
{
    private readonly ISessionService _session;

    public ServiceAddedModel(ISessionService sessionService)
    {
        _session = sessionService;
    }
    public void OnGet()
    {
        _session.StoreCurrentPageName(HttpContext, "ServiceAdded"); //TODO - replace page names with consts
    }

    public IActionResult OnPost()
    {
        return RedirectToPage("/OrganisationAdmin/Welcome");
    }
}
