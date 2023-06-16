using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages;

public class WhichVcsOrganisation : AccountAdminViewModel
{
    private readonly ICacheService _cacheService;
    private readonly IServiceDirectoryClient _serviceDirectoryClient;

    public WhichVcsOrganisation(ICacheService cacheService, IServiceDirectoryClient serviceDirectoryClient)
    {
        PageHeading = "Which organisation do they work for?";
        ErrorMessage = "Select an organisation";
        BackLink = HttpContext.IsUserLaManager() ? "/TypeOfUserVcs" : "/WhichLocalAuthority";
        _cacheService = cacheService;
        _serviceDirectoryClient = serviceDirectoryClient;
    }

    [BindProperty]
    public required string VcsOrganisationName { get; set; } = string.Empty;

    public required List<string> VcsOrganisations { get; set; } = new List<string>();

    public async Task OnGet()
    {
        var permissionModel = await _cacheService.GetPermissionModel();
        ArgumentNullException.ThrowIfNull(permissionModel);
        
        var vcsOrganisations = await _serviceDirectoryClient.GetVcsOrganisations(permissionModel.LaOrganisationId);
        VcsOrganisations = vcsOrganisations.Select(l => l.Name).ToList();

        VcsOrganisationName = permissionModel.VcsOrganisationName;
    }

    public async Task<IActionResult> OnPost()
    {
        var permissionModel = await _cacheService.GetPermissionModel();
        ArgumentNullException.ThrowIfNull(permissionModel);
        
        var vcsOrganisations = await _serviceDirectoryClient.GetVcsOrganisations(permissionModel.LaOrganisationId);

        if (ModelState.IsValid && !string.IsNullOrWhiteSpace(VcsOrganisationName) && VcsOrganisationName.Length <= 255)
        {
            permissionModel.VcsOrganisationId = vcsOrganisations.Single(l => l.Name == VcsOrganisationName).Id;
            permissionModel.VcsOrganisationName = VcsOrganisationName;

            await _cacheService.StorePermissionModel(permissionModel);

            return RedirectToPage("/UserEmail");
        }
        
        HasValidationError = true;
        
        VcsOrganisations = vcsOrganisations.Select(l => l.Name).ToList();

        BackLink = HttpContext.IsUserLaManager() ? "/TypeOfUserVcs" : "/WhichLocalAuthority";

        return Page();
    }
}