using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages;

public class WhichLocalAuthority : AccountAdminViewModel
{
    private readonly ICacheService _cacheService;
    private readonly IOrganisationAdminClientService _serviceDirectoryClient;

    public WhichLocalAuthority(ICacheService cacheService, IOrganisationAdminClientService serviceDirectoryClient)
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
        var localAuthorities = await TryGetLocalAuthoritiesFromCache();
        LocalAuthorities = localAuthorities.Select(l => l.Name).ToList();
        
        var permissionModel = _cacheService.GetPermissionModel();
        
        if (permissionModel is not null)
        {
            LaOrganisationName = permissionModel.LaOrganisationName;
            
            BackLink = permissionModel.VcsJourney ? "/TypeOfUserVcs" : "/TypeOfUserLa";
            
            PageHeading = permissionModel.VcsJourney
                ? "Which local authority area do they work in?"
                : "Which local authority is the account for?";
        }
    }

    public async Task<IActionResult> OnPost()
    {
        var localAuthorities = await TryGetLocalAuthoritiesFromCache();

        if (ModelState.IsValid && !string.IsNullOrWhiteSpace(LaOrganisationName) && LaOrganisationName.Length <= 255)
        {
            var permissionModel = _cacheService.GetPermissionModel();
            ArgumentNullException.ThrowIfNull(permissionModel);
            
            permissionModel.OrganisationId = localAuthorities.Single(l => l.Name == LaOrganisationName).Id;
            permissionModel.LaOrganisationName = LaOrganisationName;

            _cacheService.StorePermissionModel(permissionModel);

            return RedirectToPage(permissionModel.VcsJourney ? "/WhichVcsOrganisation" : "/UserEmail");
        }
        
        HasValidationError = true;
        
        LocalAuthorities = localAuthorities.Select(l => l.Name).ToList();

        return Page();
    }

    private async Task<List<OrganisationDto>> TryGetLocalAuthoritiesFromCache(CancellationToken cancellationToken = default)
    {
        var semaphore = new SemaphoreSlim(1, 1);
        var localAuthorities = _cacheService.GetLocalAuthorities();
        if (localAuthorities is not null)
            return localAuthorities;

        try
        {
            await semaphore.WaitAsync(cancellationToken);

            // recheck to make sure it didn't populate before entering semaphore
            localAuthorities = _cacheService.GetLocalAuthorities();
            if (localAuthorities is not null)
                return localAuthorities;

            var organisations = await _serviceDirectoryClient.GetListOrganisations();
            localAuthorities = organisations.Where(x => x.OrganisationType == OrganisationType.LA).ToList();

            _cacheService.StoreLocalAuthorities(localAuthorities);
        }
        finally
        {
            semaphore.Release();
        }
        return localAuthorities;
    }
}