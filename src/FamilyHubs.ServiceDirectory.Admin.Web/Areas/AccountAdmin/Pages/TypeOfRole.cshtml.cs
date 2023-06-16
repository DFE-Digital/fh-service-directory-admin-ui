using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages;

public class TypeOfRole : AccountAdminViewModel
{
    private readonly ICacheService _cacheService;
    private readonly IServiceDirectoryClient _directoryClient;

    public TypeOfRole(ICacheService cacheService, IServiceDirectoryClient directoryClient)
    {
        _cacheService = cacheService;
        _directoryClient = directoryClient;
        PageHeading = "Who are you adding permissions for?";
        ErrorMessage = "Select who you are adding permissions for";
        BackLink = "/Welcome";
    }

    [BindProperty] public required string OrganisationType { get; set; }

    public string LaRoleTypeLabel { get; set; } = string.Empty;

    public string VcsRoleTypeLabel { get; set; } = string.Empty;

    public async Task OnGet()
    {
        await GetRoleTypeLabelForCurrentUser();

        var permissionModel = await _cacheService.GetPermissionModel();
        if (permissionModel is not null)
        {
            OrganisationType = permissionModel.OrganisationType;
        }
    }

    public async Task<IActionResult> OnPost()
    {
        if (ModelState.IsValid)
        {
            await _cacheService.StorePermissionModel(new PermissionModel
            {
                OrganisationType = OrganisationType
            });

            return RedirectToPage(OrganisationType == "LA" ? "/TypeOfUserLa" : "/TypeOfUserVcs");
        }

        await GetRoleTypeLabelForCurrentUser();

        HasValidationError = true;
        return Page();
    }

    private async Task GetRoleTypeLabelForCurrentUser()
    {
        var organisations = await _directoryClient.GetCachedLaOrganisations();

        ArgumentNullException.ThrowIfNull(organisations);

        var organisationName = organisations.Single(o => o.Id == HttpContext.GetUserOrganisationId()).Name;
        
        LaRoleTypeLabel = $"Someone who works for {(HttpContext.IsUserLaManager() ? organisationName : "a local authority")}";
        VcsRoleTypeLabel = $"Someone who works for a voluntary and community sector organisation {(HttpContext.IsUserLaManager() ? $"in {organisationName} area" : string.Empty)}";
    }
}