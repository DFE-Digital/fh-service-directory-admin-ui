using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages;

public class WhichLocalAuthority : AccountAdminViewModel
{
    private readonly IServiceDirectoryClient _serviceDirectoryClient;

    public WhichLocalAuthority(ICacheService cacheService, IServiceDirectoryClient serviceDirectoryClient) : base(nameof(WhichLocalAuthority), cacheService)
    {
        PageHeading = string.Empty;
        ErrorMessage = "Select a local authority";
        _serviceDirectoryClient = serviceDirectoryClient;
    }
    
    [BindProperty]
    public required string LaOrganisationName { get; set; } = string.Empty;

    public required List<string> LocalAuthorities { get; set; } = new List<string>();

    public override async Task OnGet()
    {
        await base.OnGet();
        
        var localAuthorities = await _serviceDirectoryClient.GetCachedLaOrganisations();
        LocalAuthorities = localAuthorities.Select(l => l.Name).ToList();

        LaOrganisationName = PermissionModel.LaOrganisationName;
            
        PageHeading = PermissionModel.VcsJourney
            ? "Which local authority area do they work in?"
            : "Which local authority is the account for?";
    }

    public override async Task<IActionResult> OnPost()
    {
        var laOrganisations = await _serviceDirectoryClient.GetCachedLaOrganisations();
        
        await base.OnPost();

        if (ModelState.IsValid && !string.IsNullOrWhiteSpace(LaOrganisationName) && LaOrganisationName.Length <= 255)
        {
            PermissionModel.LaOrganisationId = laOrganisations.Single(l => l.Name == LaOrganisationName).Id;
            PermissionModel.LaOrganisationName = LaOrganisationName;

            await CacheService.StorePermissionModel(PermissionModel);

            return RedirectToPage(NextPageLink);
        }
        
        PageHeading = PermissionModel.VcsJourney
            ? "Which local authority area do they work in?"
            : "Which local authority is the account for?";
        
        HasValidationError = true;
        
        LocalAuthorities = laOrganisations.Select(l => l.Name).ToList();

        return Page();
    }
}