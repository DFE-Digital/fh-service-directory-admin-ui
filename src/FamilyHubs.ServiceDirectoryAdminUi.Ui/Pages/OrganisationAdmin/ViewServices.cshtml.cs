using FamilyHubs.ServiceDirectory.Shared.Dto;
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
    private readonly IOrganisationAdminClientService _organisationAdminClientService;
    private readonly IRedisCacheService _redis;

    public List<ServiceDto> Services { get; private set; } = default!;

    public ViewServicesModel(ILocalOfferClientService localOfferClientService,
                             ISessionService sessionService,
                             IOrganisationAdminClientService organisationAdminClientService,
                             IRedisCacheService redisCacheService)
    {
        _localOfferClientService = localOfferClientService;
        _organisationAdminClientService = organisationAdminClientService;
        _redis = redisCacheService;
    }

    public async Task OnGet(long orgId)
    {   
        var sessionOrgModel = _redis.RetrieveOrganisationWithService();

        if (sessionOrgModel == null)
        {
            var organisation = await _organisationAdminClientService.GetOrganisationById(orgId);
            OrganisationViewModel = new()
            {
                Id = orgId,
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

    public async Task<IActionResult> OnGetRedirectToDetailsPage(long orgId, long serviceId)
    {
        var apiModel = await _organisationAdminClientService.GetOrganisationById(orgId);
        
        var orgVm = ApiModelToViewModelHelper.CreateViewModel(apiModel, serviceId);
        
        _redis.StoreOrganisationWithService(orgVm);

        return RedirectToPage("/OrganisationAdmin/CheckServiceDetails");
    }
}