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
        //Reset session org
        _session.ResetOrganisationWithService(HttpContext);

        if (_session.RetrieveOrganisationWithService(HttpContext) == null)
        {
            //get from db for now
            OrganisationViewModel = new()
            {
                Id = new Guid("72e653e8-1d05-4821-84e9-9177571a6013"),
                Name = "Bristol City Council"
            };
        }
        else
        {
            OrganisationViewModel = _session?.RetrieveOrganisationWithService(HttpContext) ?? new();
        }
        
        if (OrganisationViewModel != null && OrganisationViewModel?.Id != null)
                Services = await _localOfferClientService.GetServicesByOrganisationId(OrganisationViewModel.Id.ToString());
        else
            Services = new List<OpenReferralServiceDto>();

        _session?.ResetLastPageName(HttpContext);
    }

    public IActionResult OnGetAddServiceFlow(string organisationid, string serviceid, string strOrganisationViewModel)
    {
        _session.StoreUserFlow(HttpContext, "AddService");
        return RedirectToPage("/OrganisationAdmin/ServiceName", new { organisationid = organisationid });
    }

    public IActionResult OnGetManageServiceFlow(string organisationid)
    {
        _session.StoreUserFlow(HttpContext, "ManageService");
        return RedirectToPage("/OrganisationAdmin/ViewServices", new { orgId = organisationid });
    }
}
