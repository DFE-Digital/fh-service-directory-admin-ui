using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages;

public class WhichLocalAuthority : AccountAdminViewModel
{
    private readonly ICacheService _cacheService;
    private readonly IServiceDirectoryClient _serviceDirectoryClient;

    public WhichLocalAuthority(ICacheService cacheService, IServiceDirectoryClient serviceDirectoryClient)
    {
        PageHeading = string.Empty;
        ErrorMessage = "Select a local authority";
        BackLink = "/Welcome";
        _cacheService = cacheService;
        _serviceDirectoryClient = serviceDirectoryClient;
    }
    
    [BindProperty]
    public required string LaOrganisationName { get; set; } = string.Empty;

    public required List<string> LocalAuthorities { get; set; } = new List<string>();

    public async Task OnGet()
    {
        var permissionModel = await _cacheService.GetPermissionModel();
        ArgumentNullException.ThrowIfNull(permissionModel);
        
        var localAuthorities = await _serviceDirectoryClient.GetCachedLaOrganisations();
        LocalAuthorities = localAuthorities.Select(l => l.Name).ToList();

        LaOrganisationName = permissionModel.LaOrganisationName;
            
        BackLink = permissionModel.VcsJourney ? "/TypeOfUserVcs" : "/TypeOfUserLa";
            
        PageHeading = permissionModel.VcsJourney
            ? "Which local authority area do they work in?"
            : "Which local authority is the account for?";
    }

    public async Task<IActionResult> OnPost()
    {
        var laOrganisations = await _serviceDirectoryClient.GetCachedLaOrganisations();
        
        var permissionModel = await _cacheService.GetPermissionModel();
        ArgumentNullException.ThrowIfNull(permissionModel);
        
        if (ModelState.IsValid && !string.IsNullOrWhiteSpace(LaOrganisationName) && LaOrganisationName.Length <= 255)
        {
            permissionModel.LaOrganisationId = laOrganisations.Single(l => l.Name == LaOrganisationName).Id;
            permissionModel.LaOrganisationName = LaOrganisationName;

            await _cacheService.StorePermissionModel(permissionModel);

            return RedirectToPage(permissionModel.VcsJourney ? "/WhichVcsOrganisation" : "/UserEmail");
        }
        
        PageHeading = permissionModel.VcsJourney
            ? "Which local authority area do they work in?"
            : "Which local authority is the account for?";
        
        BackLink = permissionModel.VcsJourney ? "/TypeOfUserVcs" : "/TypeOfUserLa";
        
        HasValidationError = true;
        
        LocalAuthorities = laOrganisations.Select(l => l.Name).ToList();

        return Page();
    }
}