using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.SharedKernel.Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages;

//todo: all pages seem to assume that user has been through the welcome page by accessing data from the cache, but not handling when it's not there
// which means that if a user bookmarks a page, they'll get an error (or if the cache expires, although that would depend on cache timeout vs login timeout)

[Authorize]
public class WelcomeModel : HeaderPageModel
{
    [BindProperty]
    public OrganisationViewModel OrganisationViewModel { get; set; } = new();

    public FamilyHubsUser FamilyHubsUser { get; set; } = new();

    public bool IsUploadSpreadsheetEnabled { get; private set; }
    public bool ShowAccountsSection { get; set; }
    public bool ShowLaSection { get; set; }
    public bool ShowActivateLa { get; set; }
    public bool ShowVcsSection { get; set; }
    public bool ShowVcsServiceLinks { get; set; }
    public bool ShowVcsOrganisationLinks { get; set; }
    public bool ShowSpreadsheetUploadSection { get; set; }
    public bool ShowSubjectAccessRequestSection { get; set; }
    public string VcsHeaderText { get; set; } = string.Empty;
    public string SubjectAccessRequestText { get; set; } = "Subject Access Request";
    public string AddVscOrganisationLinkText { get; set; } = string.Empty;
    public string ManageVscOrganisationLinkText { get; set; } = string.Empty;
    public string ManageVscOrganisationText { get; set; } = string.Empty;

    private readonly ICacheService _cacheService;
    private readonly IServiceDirectoryClient _serviceDirectoryClient;

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

        //todo: if org already in cache, retrieves twice!
        if (await _cacheService.RetrieveOrganisationWithService() == null)
        {
            OrganisationWithServicesDto? organisation = null;

            if (long.TryParse(FamilyHubsUser.OrganisationId, out var organisationId))
            {
                //todo: looks like we get the organisation with *all* it's services, just so that we can use the name!
                organisation = await _serviceDirectoryClient.GetOrganisationById(organisationId);
            }

            if (organisation != null)
            {
                OrganisationViewModel = new OrganisationViewModel
                {
                    Id = organisation.Id,
                    Name = organisation.Name
                };

                // stores big model in cache, but only populates id and name
                await _cacheService.StoreOrganisationWithService(OrganisationViewModel);
            }
        }
        else
        {
            //todo: if org in cache, then it's not when retrieving it moments latest, sets organisation view model to blank, which will break the view!
            // need to handle the org being missing
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
                ShowVcsSection = true;
                ShowVcsOrganisationLinks = true;
                ShowVcsServiceLinks = true;
                ShowSpreadsheetUploadSection = true;
                ShowSubjectAccessRequestSection = false;
                VcsHeaderText = "Voluntary community organisations (VCSs)";
                AddVscOrganisationLinkText = "Add a VCS organisation";
                ManageVscOrganisationLinkText = "Manage VCS organisations";
                ManageVscOrganisationText = "View, change or delete existing organisations.";
                SubjectAccessRequestText = "Subject Access Request";
                break;

            case RoleTypes.LaDualRole:
            case RoleTypes.LaManager:
                ShowAccountsSection = true;
                ShowLaSection = true;
                ShowVcsSection = true;
                ShowVcsOrganisationLinks = true;
                ShowSubjectAccessRequestSection = true;
                VcsHeaderText = "Voluntary community organisations (VCSs)";
                AddVscOrganisationLinkText = "Add an organisation";
                ManageVscOrganisationLinkText = "Manage organisations";
                ManageVscOrganisationText = "View or delete organisations.";
                SubjectAccessRequestText = "Subject Access Request";
                break;

            case RoleTypes.VcsDualRole:
            case RoleTypes.VcsManager:
                ShowVcsSection = true;
                ShowVcsServiceLinks = true;
                break;

            default:
                //  Show nothing
                break;
        }

    }
}
