using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.SharedKernel.Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages;

[Authorize]
public class WelcomeModel : PageModel
{
    [BindProperty]
    public OrganisationViewModel OrganisationViewModel { get; set; } = new OrganisationViewModel();

    public FamilyHubsUser FamilyHubsUser { get; set; } = new FamilyHubsUser();

    public bool IsUploadSpreadsheetEnabled { get; private set; }

    private readonly ICacheService _cacheService;
    private readonly IServiceDirectoryClient _serviceDirectoryClient;

    public List<ServiceDto> Services { get; private set; } = default!;

    public string LastPage { get; private set; } = default!;

    public WelcomeModel(
        ICacheService cacheService,
        IServiceDirectoryClient serviceDirectoryClient,
        IConfiguration configuration)
    {
        _cacheService = cacheService;
        _serviceDirectoryClient = serviceDirectoryClient;
        IsUploadSpreadsheetEnabled = configuration.GetValue<bool>("IsUploadSpreadsheetEnabled");
    }

    public async Task OnGet(long? organisationId)
    {
        LastPage = $"/OrganisationAdmin/{await _cacheService.RetrieveLastPageName()}";
        
        FamilyHubsUser = HttpContext.GetFamilyHubsUser();
        
        if (await _cacheService.RetrieveOrganisationWithService() == null)
        {
            OrganisationWithServicesDto? organisation = null;

            if (organisationId.HasValue)
            {
                organisation = await _serviceDirectoryClient.GetOrganisationById(organisationId.Value);
            }

            if (organisation != null)
            {
                OrganisationViewModel = new OrganisationViewModel
                {
                    Id = organisation.Id,
                    Name = organisation.Name
                };

                await _cacheService.StoreOrganisationWithService(OrganisationViewModel);
            }
        }
        else
        {
            OrganisationViewModel = await _cacheService.RetrieveOrganisationWithService() ?? new OrganisationViewModel();
        }

        Services = await _serviceDirectoryClient.GetServicesByOrganisationId(OrganisationViewModel.Id);

        await _cacheService.ResetLastPageName();
    }

    public IActionResult OnGetAddPermissionFlow()
    {
        _cacheService.ResetPermissionModel();
        _cacheService.StoreUserFlow("AddPermissions"); 
        return RedirectToPage("/TypeOfRole", new { area = "AccountAdmin" });
    }
    
    public async Task<IActionResult> OnGetAddServiceFlow(string organisationId, string serviceId, string strOrganisationViewModel)
    {
        await _cacheService.StoreUserFlow("AddService");
        return RedirectToPage("/ServiceName", new { area = "OrganisationAdmin", organisationId });
    }

    public async Task<IActionResult> OnGetManageServiceFlow(string organisationId)
    {
        await _cacheService.StoreUserFlow("ManageService");
        return RedirectToPage("/ViewServices", new {area = "OrganisationAdmin", organisationId });
    }

    public async Task<IActionResult> OnGetUploadSpreadsheetData(string organisationId)
    {
        await _cacheService.StoreUserFlow("UploadSpreadsheetData");
        return RedirectToPage("/UploadSpreadsheetData", new {area = "OrganisationAdmin", organisationId });
    }

    public async Task<IActionResult> OnGetAddOrganisation()
    {
        await _cacheService.StoreUserFlow("AddOrganisation");
        return RedirectToPage("/AddOrganisation", new { area = "vcsAdmin" });
    }
}
