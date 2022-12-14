using FamilyHubs.ServiceDirectory.Shared.Helpers;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServices;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

public class WelcomeModel : PageModel
{
    [BindProperty]
    public OrganisationViewModel OrganisationViewModel { get; set; } = new OrganisationViewModel();

    private readonly ILocalOfferClientService _localOfferClientService;

    private readonly ISessionService _session;
    private readonly IRedisCacheService _redis;

    public List<OpenReferralServiceDto> Services { get; private set; } = default!;

    public WelcomeModel(ILocalOfferClientService localOfferClientService, ISessionService sessionService, IRedisCacheService redisCacheService)
    {
        _localOfferClientService = localOfferClientService;
        _session = sessionService;
        _redis = redisCacheService;
    }

    public async Task OnGet(string strOrganisationViewModel)
    {   
        _redis.ResetOrganisationWithService();

        if (_redis.RetrieveOrganisationWithService() == null)
        {
            OrganisationViewModel = new()
            {
                Id = new Guid("72e653e8-1d05-4821-84e9-9177571a6013"),
                Name = "Bristol City Council"
            };
        }
        else
        {   
            OrganisationViewModel = _redis?.RetrieveOrganisationWithService() ?? new();
        }
        
        if (OrganisationViewModel != null && OrganisationViewModel?.Id != null)
                Services = await _localOfferClientService.GetServicesByOrganisationId(OrganisationViewModel.Id.ToString());
        else
            Services = new List<OpenReferralServiceDto>();

        _redis?.ResetLastPageName();
    }

    public IActionResult OnGetAddServiceFlow(string organisationid, string serviceid, string strOrganisationViewModel)
    {   
        _redis.StoreUserFlow("AddService");
        return RedirectToPage("/OrganisationAdmin/ServiceName", new { organisationid = organisationid });
    }

    public IActionResult OnGetManageServiceFlow(string organisationid)
    {   
        _redis.StoreUserFlow("ManageService");
        return RedirectToPage("/OrganisationAdmin/ViewServices", new { orgId = organisationid });
    }
}
