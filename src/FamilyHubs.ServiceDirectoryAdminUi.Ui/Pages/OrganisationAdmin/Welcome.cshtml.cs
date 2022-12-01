using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServices;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

[Authorize(Policy = "ServiceMaintainer")]
public class WelcomeModel : PageModel
{
    [BindProperty]
    public OrganisationViewModel OrganisationViewModel { get; set; } = new OrganisationViewModel();

    private readonly ILocalOfferClientService _localOfferClientService;

    private readonly ISessionService _session;
    private readonly IRedisCacheService _redis;
    private readonly IOpenReferralOrganisationAdminClientService _organisationAdminClientService;

    public List<OpenReferralServiceDto> Services { get; private set; } = default!;

    public WelcomeModel(IOpenReferralOrganisationAdminClientService organisationAdminClientService, ILocalOfferClientService localOfferClientService, ISessionService sessionService, IRedisCacheService redisCacheService)
    {
        _localOfferClientService = localOfferClientService;
        _session = sessionService;
        _redis = redisCacheService;
        _organisationAdminClientService = organisationAdminClientService;
    }

    public async Task<IActionResult> OnGet(string? organisationId)
    {   
        _redis.ResetOrganisationWithService();

        if (organisationId == null) 
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                organisationId = _redis.RetrieveStringValue($"OrganisationId-{User.Identity.Name}");
            }
        }

        if (organisationId == null)
        {
            return RedirectToPage("/OrganisationAdmin/SignIn");
        }

        if (_redis.RetrieveOrganisationWithService() == null)
        {
            var organisation = await _organisationAdminClientService.GetOpenReferralOrganisationById(organisationId ?? string.Empty);
            if (organisation != null) 
            {
                OrganisationViewModel = new()
                {
                    Id = new Guid(organisationId ?? string.Empty),
                    Name = organisation.Name
                };

                _redis.StoreOrganisationWithService(OrganisationViewModel);
            }
            else
            {
                OrganisationViewModel = new()
                {
                    Id = new Guid("72e653e8-1d05-4821-84e9-9177571a6013"),
                    Name = "Bristol City Council"
                };
            }
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

        return Page();
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
