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
    private readonly IOrganisationAdminClientService _organisationAdminClientService;

    public List<ServiceDto> Services { get; private set; } = default!;

    public string LastPage { get; private set; } = default!;

    public WelcomeModel(
        ICacheService cacheService, 
        IOrganisationAdminClientService organisationAdminClientService,
        IConfiguration configuration)
    {
        _cacheService = cacheService;
        _organisationAdminClientService = organisationAdminClientService;
        IsUploadSpreadsheetEnabled = configuration.GetValue<bool>("IsUploadSpreadsheetEnabled");
    }

    public async Task OnGet(long? organisationId)
    {
        LastPage = $"/OrganisationAdmin/{_cacheService.RetrieveLastPageName()}";
        
        FamilyHubsUser = HttpContext.GetFamilyHubsUser();
        
        if (_cacheService.RetrieveOrganisationWithService() == null)
        {
            OrganisationWithServicesDto? organisation = null;

            if (organisationId.HasValue)
            {
                organisation = await _organisationAdminClientService.GetOrganisationById(organisationId.Value);
            }

            if (organisation != null)
            {
                OrganisationViewModel = new OrganisationViewModel
                {
                    Id = organisation.Id,
                    Name = organisation.Name
                };

                _cacheService.StoreOrganisationWithService(OrganisationViewModel);
            }
        }
        else
        {
            OrganisationViewModel = _cacheService.RetrieveOrganisationWithService() ?? new OrganisationViewModel();
        }

        Services = await _organisationAdminClientService.GetServicesByOrganisationId(OrganisationViewModel.Id);

        _cacheService.ResetLastPageName();
    }

    public IActionResult OnGetAddPermissionFlow()
    {
        _cacheService.StoreUserFlow("AddPermissionFlow");
        return RedirectToPage("/TypeOfRole", new { area = "AccountAdmin" });
    }
    
    public IActionResult OnGetAddServiceFlow(string organisationId, string serviceId, string strOrganisationViewModel)
    {
        _cacheService.StoreUserFlow("AddService");
        return RedirectToPage("/ServiceName", new { area = "OrganisationAdmin", organisationId });
    }

    public IActionResult OnGetManageServiceFlow(string organisationId)
    {
        _cacheService.StoreUserFlow("ManageService");
        return RedirectToPage("/ViewServices", new {area = "OrganisationAdmin", organisationId });
    }

    public IActionResult OnGetUploadSpreadsheetData(string organisationId)
    {
        _cacheService.StoreUserFlow("UploadSpreadsheetData");
        return RedirectToPage("/UploadSpreadsheetData", new {area = "OrganisationAdmin", organisationId });
    }
}
