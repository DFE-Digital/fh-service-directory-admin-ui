using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages;

public class WhichVcsOrganisation : AccountAdminViewModel
{
    private readonly IServiceDirectoryClient _serviceDirectoryClient;

    public WhichVcsOrganisation(ICacheService cacheService, IServiceDirectoryClient serviceDirectoryClient) : base(nameof(WhichVcsOrganisation), cacheService)
    {
        PageHeading = "Which organisation do they work for?";
        ErrorMessage = "Select an organisation";
        _serviceDirectoryClient = serviceDirectoryClient;
    }

    [BindProperty]
    public required string VcsOrganisationName { get; set; } = string.Empty;

    public required List<string> VcsOrganisations { get; set; } = new List<string>();

    public override async Task OnGet()
    {
        await base.OnGet();
        
        var vcsOrganisations = await _serviceDirectoryClient.GetCachedVcsOrganisations(PermissionModel.LaOrganisationId);
        VcsOrganisations = vcsOrganisations.Select(l => l.Name).ToList();

        VcsOrganisationName = PermissionModel.VcsOrganisationName;
    }

    public override async Task<IActionResult> OnPost()
    {
        await base.OnPost();

        var vcsOrganisations = await _serviceDirectoryClient.GetCachedVcsOrganisations(PermissionModel.LaOrganisationId);

        if (ModelState.IsValid && !string.IsNullOrWhiteSpace(VcsOrganisationName) && VcsOrganisationName.Length <= 255)
        {
            PermissionModel.VcsOrganisationId = vcsOrganisations.Single(l => l.Name == VcsOrganisationName).Id;
            PermissionModel.VcsOrganisationName = VcsOrganisationName;

            await CacheService.StorePermissionModel(PermissionModel);

            return RedirectToPage(NextPageLink);
        }
        
        HasValidationError = true;
        
        VcsOrganisations = vcsOrganisations.Select(l => l.Name).ToList();
        
        return Page();
    }
}