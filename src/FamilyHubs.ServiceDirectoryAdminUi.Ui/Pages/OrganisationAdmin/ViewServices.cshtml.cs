using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralOrganisations;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServices;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

[Authorize(Policy = "ServiceMaintainer")]
public class ViewServicesModel : PageModel
{
    [BindProperty]
    public OrganisationViewModel OrganisationViewModel { get; set; } = new OrganisationViewModel();
    public string? StrOrganisationViewModel { get; private set; }

    private readonly ILocalOfferClientService _localOfferClientService;
    private readonly ISessionService _session;
    private readonly IOpenReferralOrganisationAdminClientService _openReferralOrganisationAdminClientService;
    private readonly IRedisCacheService _redis;

    public List<OpenReferralServiceDto> Services { get; private set; } = default!;

    public ViewServicesModel(ILocalOfferClientService localOfferClientService,
                             ISessionService sessionService,
                             IOpenReferralOrganisationAdminClientService openReferralOrganisationAdminClientService,
                             IRedisCacheService redisCacheService)
    {
        _localOfferClientService = localOfferClientService;
        _session = sessionService;
        _openReferralOrganisationAdminClientService = openReferralOrganisationAdminClientService;
        _redis = redisCacheService;
    }

    public async Task OnGet(string orgId)
    {   
        var sessionOrgModel = _redis.RetrieveOrganisationWithService();

        if (sessionOrgModel == null)
        {
            var organisation = await _openReferralOrganisationAdminClientService.GetOpenReferralOrganisationById(orgId ?? string.Empty);
            if (organisation != null)
            {
                OrganisationViewModel = new()
                {
                    Id = new Guid(orgId ?? string.Empty),
                    Name = organisation.Name
                };

                _redis.StoreOrganisationWithService(OrganisationViewModel);
            }
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
            _redis.StoreOrganisationWithService(orgVm);

        return RedirectToPage($"/OrganisationAdmin/CheckServiceDetails");
    }
}