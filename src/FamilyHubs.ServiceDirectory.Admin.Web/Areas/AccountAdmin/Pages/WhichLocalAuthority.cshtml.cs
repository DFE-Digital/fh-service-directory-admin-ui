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
        
        var localAuthorities = await TryGetLaOrganisationFromCache();
        LocalAuthorities = localAuthorities.Select(l => l.Name).ToList();

        LaOrganisationName = permissionModel.LaOrganisationName;
            
        BackLink = permissionModel.VcsJourney ? "/TypeOfUserVcs" : "/TypeOfUserLa";
            
        PageHeading = permissionModel.VcsJourney
            ? "Which local authority area do they work in?"
            : "Which local authority is the account for?";
    }

    public async Task<IActionResult> OnPost()
    {
        var laOrganisations = await TryGetLaOrganisationFromCache();

        if (ModelState.IsValid && !string.IsNullOrWhiteSpace(LaOrganisationName) && LaOrganisationName.Length <= 255)
        {
            var permissionModel = await _cacheService.GetPermissionModel();
            ArgumentNullException.ThrowIfNull(permissionModel);
            
            permissionModel.LaOrganisationId = laOrganisations.Single(l => l.Name == LaOrganisationName).Id;
            permissionModel.LaOrganisationName = LaOrganisationName;

            await _cacheService.StorePermissionModel(permissionModel);

            return RedirectToPage(permissionModel.VcsJourney ? "/WhichVcsOrganisation" : "/UserEmail");
        }
        
        HasValidationError = true;
        
        LocalAuthorities = laOrganisations.Select(l => l.Name).ToList();

        return Page();
    }

    private async Task<List<OrganisationDto>> TryGetLaOrganisationFromCache(CancellationToken cancellationToken = default)
    {
        var semaphore = new SemaphoreSlim(1, 1);
        var laOrganisations = await _cacheService.GetLaOrganisations();
        if (laOrganisations is not null)
            return laOrganisations;

        try
        {
            await semaphore.WaitAsync(cancellationToken);

            // recheck to make sure it didn't populate before entering semaphore
            laOrganisations = await _cacheService.GetLaOrganisations();
            if (laOrganisations is not null)
                return laOrganisations;

            var organisations = await _serviceDirectoryClient.GetListOrganisations();
            laOrganisations = organisations.Where(x => x.OrganisationType == OrganisationType.LA).ToList();

            await _cacheService.StoreLaOrganisations(laOrganisations);
        }
        finally
        {
            semaphore.Release();
        }
        return laOrganisations;
    }
}