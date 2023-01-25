using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServices;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

public class ViewServicesModel : PageModel
{
    [BindProperty]
    public OrganisationViewModel OrganisationViewModel { get; set; } = new OrganisationViewModel();

    private readonly ILocalOfferClientService _localOfferClientService;
    private readonly IOpenReferralOrganisationAdminClientService _openReferralOrganisationAdminClientService;
    private readonly IRedisCacheService _redis;

    public List<OpenReferralServiceDto> Services { get; private set; } = default!;

    public ViewServicesModel(ILocalOfferClientService localOfferClientService,
                             ISessionService sessionService,
                             IOpenReferralOrganisationAdminClientService openReferralOrganisationAdminClientService,
                             IRedisCacheService redisCacheService)
    {
        _localOfferClientService = localOfferClientService;
        _openReferralOrganisationAdminClientService = openReferralOrganisationAdminClientService;
        _redis = redisCacheService;
    }

    public async Task OnGet(string orgId)
    {   
        var sessionOrgModel = _redis.RetrieveOrganisationWithService();

        if (sessionOrgModel == null)
        {
            var organisation = await _openReferralOrganisationAdminClientService.GetOpenReferralOrganisationById(orgId);
            OrganisationViewModel = new()
            {
                Id = new Guid(orgId),
                Name = organisation.Name
            };

            _redis.StoreOrganisationWithService(OrganisationViewModel);
        }
        else
        {
            OrganisationViewModel = sessionOrgModel;
        }

        Services = await _localOfferClientService.GetServicesByOrganisationId(OrganisationViewModel.Id.ToString());
        Services.Sort((x, y) => string.CompareOrdinal(x.Name, y.Name));
    }

    public async Task<IActionResult> OnGetRedirectToDetailsPage(string orgId, string serviceId)
    {
        var apiModel = await _openReferralOrganisationAdminClientService.GetOpenReferralOrganisationById(orgId);
        
        var orgVm = ApiModelToViewModelHelper.CreateViewModel(apiModel, serviceId);
        
        _redis.StoreOrganisationWithService(orgVm);

        return RedirectToPage("/OrganisationAdmin/CheckServiceDetails");
    }
}