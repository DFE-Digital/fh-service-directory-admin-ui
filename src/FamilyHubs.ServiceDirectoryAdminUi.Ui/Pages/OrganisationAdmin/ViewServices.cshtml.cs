using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralOrganisations;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServices;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

public class ViewServicesModel : PageModel
{
    [BindProperty]
    public OrganisationViewModel OrganisationViewModel { get; set; } = new OrganisationViewModel();
    public string? StrOrganisationViewModel { get; private set; }

    private readonly ILocalOfferClientService _localOfferClientService;
    private readonly ISessionService _session;
    private readonly IOpenReferralOrganisationAdminClientService _openReferralOrganisationAdminClientService;

    public List<OpenReferralServiceDto> Services { get; private set; } = default!;

    public ViewServicesModel(ILocalOfferClientService localOfferClientService,
                             ISessionService sessionService,
                             IOpenReferralOrganisationAdminClientService openReferralOrganisationAdminClientService)
    {
        _localOfferClientService = localOfferClientService;
        _session = sessionService;
        _openReferralOrganisationAdminClientService = openReferralOrganisationAdminClientService;
    }

    public async Task OnGet(string orgId)
    {
        var sessionOrgModel = _session.RetrieveOrganisationWithService(HttpContext);

        if (sessionOrgModel == null)
        {
            OrganisationViewModel = new()
            {
                Id = new Guid("72e653e8-1d05-4821-84e9-9177571a6013"),
                Name = "Bristol City Council"
            };
        }
        else
        {
            OrganisationViewModel = sessionOrgModel;
        }

        if (OrganisationViewModel != null)
            Services = await _localOfferClientService.GetServicesByOrganisationId(OrganisationViewModel.Id.ToString());
        else
            Services = new List<OpenReferralServiceDto>();
    }

    public async Task<IActionResult> OnGetRedirectToDetailsPage(string orgId, string serviceId)
    {
        OpenReferralOrganisationWithServicesDto apiModel = await _openReferralOrganisationAdminClientService.GetOpenReferralOrganisationById(orgId);
        
        var orgVm = ApiModelToViewModelHelper.CreateViewModel(apiModel, serviceId);
        
        if (orgVm != null)
            _session.StoreOrganisationWithService(HttpContext, orgVm);

        return RedirectToPage($"/OrganisationAdmin/CheckServiceDetails");
    }
}