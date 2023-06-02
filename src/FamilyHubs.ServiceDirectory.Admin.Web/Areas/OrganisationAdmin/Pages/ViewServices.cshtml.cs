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

    private readonly IServiceDirectoryClient _serviceDirectoryClient;
    private readonly ICacheService _cacheService;

    public List<ServiceDto> Services { get; private set; } = default!;

    public ViewServicesModel(
        IServiceDirectoryClient serviceDirectoryClient,
        ICacheService cacheService)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
        _cacheService = cacheService;
    }

    public async Task OnGet(long orgId)
    {   
        var sessionOrgModel = await _cacheService.RetrieveOrganisationWithService();

        if (sessionOrgModel == null)
        {
            var organisation = await _serviceDirectoryClient.GetOrganisationById(orgId);
            OrganisationViewModel = new OrganisationViewModel
            {
                Id = orgId,
                Name = organisation?.Name
            };

            await _cacheService.StoreOrganisationWithService(OrganisationViewModel);
        }
        else
        {
            OrganisationViewModel = sessionOrgModel;
        }

        Services = await _serviceDirectoryClient.GetServicesByOrganisationId(OrganisationViewModel.Id);
        Services.Sort((x, y) => string.CompareOrdinal(x.Name, y.Name));
    }

    public async Task<IActionResult> OnGetRedirectToDetailsPage(long orgId, long serviceId)
    {
        var apiModel = await _serviceDirectoryClient.GetOrganisationById(orgId);
        ArgumentNullException.ThrowIfNull(apiModel);

        var orgVm = ApiModelToViewModelHelper.CreateViewModel(apiModel, serviceId);
        
        await _cacheService.StoreOrganisationWithService(orgVm);

        return RedirectToPage("/OrganisationAdmin/CheckServiceDetails");
    }
}