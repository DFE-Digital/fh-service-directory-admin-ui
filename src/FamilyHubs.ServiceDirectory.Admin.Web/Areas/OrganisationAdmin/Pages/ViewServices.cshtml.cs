using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.OrganisationAdmin.Pages;

public class ViewServicesModel : PageModel
{
    [BindProperty]
    public OrganisationViewModel OrganisationViewModel { get; set; } = new OrganisationViewModel();

    private readonly IOrganisationAdminClientService _organisationAdminClientService;
    private readonly ICacheService _cacheService;

    public List<ServiceDto> Services { get; private set; } = default!;

    public ViewServicesModel(
        IOrganisationAdminClientService organisationAdminClientService,
        ICacheService cacheService)
    {
        _organisationAdminClientService = organisationAdminClientService;
        _cacheService = cacheService;
    }

    public async Task OnGet(long orgId)
    {   
        var sessionOrgModel = _cacheService.RetrieveOrganisationWithService();

        if (sessionOrgModel == null)
        {
            var organisation = await _organisationAdminClientService.GetOrganisationById(orgId);
            OrganisationViewModel = new OrganisationViewModel
            {
                Id = orgId,
                Name = organisation?.Name
            };

            _cacheService.StoreOrganisationWithService(OrganisationViewModel);
        }
        else
        {
            OrganisationViewModel = sessionOrgModel;
        }

        Services = await _organisationAdminClientService.GetServicesByOrganisationId(OrganisationViewModel.Id);
        Services.Sort((x, y) => string.CompareOrdinal(x.Name, y.Name));
    }

    public async Task<IActionResult> OnGetRedirectToDetailsPage(long orgId, long serviceId)
    {
        var apiModel = await _organisationAdminClientService.GetOrganisationById(orgId);
        ArgumentNullException.ThrowIfNull(apiModel);

        var orgVm = ApiModelToViewModelHelper.CreateViewModel(apiModel, serviceId);
        
        _cacheService.StoreOrganisationWithService(orgVm);

        return RedirectToPage("/OrganisationAdmin/CheckServiceDetails");
    }
}