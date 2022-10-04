using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServices;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

public class WelcomeModel : PageModel
{
    [BindProperty]
    public OrganisationViewModel OrganisationViewModel { get; set; } = new OrganisationViewModel();
    //public string? StrOrganisationViewModel { get; private set; }

    private readonly ILocalOfferClientService _localOfferClientService;
    private readonly ISessionService _session;

    public List<OpenReferralServiceDto> Services { get; private set; } = default!;

    public WelcomeModel(ILocalOfferClientService localOfferClientService, ISessionService sessionService)
    {
        _localOfferClientService = localOfferClientService;
        _session = sessionService;
    }

    public async Task OnGet(string strOrganisationViewModel)
    {
        /*** Using Session storage as a service ***/

        var sessionOrgModel = _session.RetrieveOrganisationWithService(HttpContext);
        OrganisationViewModel = sessionOrgModel ?? new OrganisationViewModel();

        if (OrganisationViewModel != null && sessionOrgModel != default)
            Services = await _localOfferClientService.GetServicesByOrganisationId(OrganisationViewModel.Id.ToString());
        else
            Services = new List<OpenReferralServiceDto>();
    }

    public async Task<IActionResult> OnGetAddServiceFlow(string organisationid, string serviceid, string strOrganisationViewModel)
    {
        _session.StoreUserFlow(HttpContext, "AddService");
        return RedirectToPage("/OrganisationAdmin/ServiceName", new { organisationid = organisationid });
    }

    public async Task<IActionResult> OnGetManageServiceFlow(string organisationid)
    {
        _session.StoreUserFlow(HttpContext, "ManageService");
        return RedirectToPage("/OrganisationAdmin/ViewServices", new { orgId = organisationid });
    }
}
