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

    public bool IsUploadSpreadsheetEnabled { get; private set; } = false;
    public bool ShowAccountsSection { get; set; } = false;
    public bool ShowLaSection { get; set; } = false;
    public bool ShowActivateLa { get; set; } = false;
    public bool ShowVcsSection { get; set; } = false;
    public bool ShowVcsHeaderBreak { get; set; } = false;
    public bool ShowVcsServiceLinks { get; set; } = false;
    public bool ShowVcsOrganisationLinks { get; set; } = false;
    public bool ShowSpreadsheetUploadSection { get; set; } = false;
    public string AddVscOrganisationLinkText { get; set; } = string.Empty;
    public string ManageVscOrganisationLinkText { get; set; } = string.Empty;
    public string ManageVscOrganisationText { get; set; } = string.Empty;

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

    public async Task OnGet()
    {
        LastPage = $"/OrganisationAdmin/{await _cacheService.RetrieveLastPageName()}";

        FamilyHubsUser = HttpContext.GetFamilyHubsUser();
        await SetOrganisation();
        SetVisibleSections(FamilyHubsUser.Role);

        Services = await _serviceDirectoryClient.GetServicesByOrganisationId(OrganisationViewModel.Id);

        await _cacheService.ResetLastPageName();
    }

    public IActionResult OnGetAddPermissionFlow()
    {        
        _cacheService.StoreUserFlow("AddPermissions"); 
        return RedirectToPage("/TypeOfRole", new { area = "AccountAdmin" , cacheid = Guid.NewGuid() });
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
        await _cacheService.ResetString(CacheKeyNames.LaOrganisationId);
        await _cacheService.ResetString(CacheKeyNames.AddOrganisationName);
        return RedirectToPage("/AddOrganisationWhichLocalAuthority", new { area = "vcsAdmin" });
    }

    private async Task SetOrganisation()
    {
        if (HttpContext.IsUserDfeAdmin())
            return;

        if (await _cacheService.RetrieveOrganisationWithService() == null)
        {
            OrganisationWithServicesDto? organisation = null;

            if (long.TryParse(FamilyHubsUser.OrganisationId, out var organisationId))
            {
                organisation = await _serviceDirectoryClient.GetOrganisationById(organisationId);
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

    }

    private void SetVisibleSections(string role)
    {
        switch (role)
        {
            case RoleTypes.DfeAdmin:
                ShowAccountsSection = true;
                ShowLaSection = true;
                ShowActivateLa = true;
                ShowVcsSection = true;
                ShowVcsHeaderBreak = true;
                ShowVcsOrganisationLinks = true;
                ShowVcsServiceLinks = true;
                ShowSpreadsheetUploadSection = true;
                AddVscOrganisationLinkText = "Add a VCS organisation";
                ManageVscOrganisationLinkText = "Manage VCS organisations";
                ManageVscOrganisationText = "View, change or delete existing organisations.";
                break;

            case RoleTypes.LaDualRole:
            case RoleTypes.LaManager:
                ShowAccountsSection = true;
                ShowLaSection = true;
                ShowVcsSection = true;
                ShowVcsHeaderBreak = true;
                ShowVcsOrganisationLinks = true;
                AddVscOrganisationLinkText = "Add an organisation";
                ManageVscOrganisationLinkText = "Manage organisations";
                ManageVscOrganisationText = "View or delete organisations.";
                break;

            case RoleTypes.LaProfessional:
                ShowLaSection = true;
                ShowVcsSection = true;
                ShowVcsHeaderBreak = true;
                ShowVcsOrganisationLinks = true;
                AddVscOrganisationLinkText = "Add an organisation";
                ManageVscOrganisationLinkText = "Manage organisations";
                ManageVscOrganisationText = "View or delete organisations.";
                break;

            case RoleTypes.VcsDualRole:
            case RoleTypes.VcsManager:
                ShowVcsSection = true;
                ShowVcsServiceLinks = true;
                //AddVscOrganisationLinqText = "Add a VCS organisation";    --  NotVisible while ShowVcsSection = false or ShowVcsOrganisationLinks = false
                //ManageVscOrganisationLinqText = "Manage organisations";   --  NotVisible while ShowVcsSection = false or ShowVcsOrganisationLinks = false
                break;

            default:
                //  Show nothing
                break;
        }

    }
}
