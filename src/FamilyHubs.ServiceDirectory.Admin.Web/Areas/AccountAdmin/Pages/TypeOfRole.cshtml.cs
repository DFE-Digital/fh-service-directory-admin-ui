using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.Errors;
using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.SharedKernel.Razor.ErrorNext;
using FamilyHubs.SharedKernel.Razor.FullPages.Radios;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages;

public class TypeOfRole : AccountAdminViewModel, IRadiosPageModel
{   
    private readonly IServiceDirectoryClient _directoryClient;

    public string Legend => "Who are you adding permissions for?";

    public IEnumerable<IRadio> Radios => new Radio[]
    {
        new(LaRoleTypeLabel, "LA"),
        new(VcsRoleTypeLabel, "VCS")
    };

    public IErrorState Errors { get; protected set; } = ErrorState.Empty;

    [BindProperty]
    public string? SelectedValue { get; set; }

    public TypeOfRole(ICacheService cacheService, IServiceDirectoryClient directoryClient) : base(nameof(TypeOfRole), cacheService)
    {
        _directoryClient = directoryClient;
    }

    public override async Task OnGet()
    {
        if (!HttpContext.IsUserDfeAdmin())
        {
            var organisationName = await GetOrganisationNameFromId();
            SetRoleTypeLabelsForCurrentUser(organisationName);
        }

        var permissionModel = await CacheService.GetPermissionModel(CacheId);
        if (permissionModel is not null)
        {
            SelectedValue = permissionModel.OrganisationType;
        }

        //Needs to override the navigation link here because we dont have permission model in cache here 
        SetNavigationLinks(SelectedValue, false);
    }

    public override async Task<IActionResult> OnPost()
    {
        var organisationName = string.Empty;
        if (!HttpContext.IsUserDfeAdmin())
        {
            organisationName = await GetOrganisationNameFromId();
        }

        if (ModelState.IsValid && SelectedValue != null)
        {
            await CacheService.StorePermissionModel(new PermissionModel
            {
                OrganisationType = SelectedValue,
                LaOrganisationName = organisationName,
                LaOrganisationId = HttpContext.IsUserLaManager() ? HttpContext.GetUserOrganisationId() : 0
            }, CacheId);

            // Needs to override the navigation link here because we dont have permission model in cache before the page is loaded
            SetNavigationLinks(SelectedValue, false);
            
            return RedirectToPage(NextPageLink, new {cacheid = CacheId});
        }

        // Needs to override the navigation link here because we dont have permission model in cache before the page is loaded
        SetNavigationLinks(SelectedValue, false);

        SetRoleTypeLabelsForCurrentUser(organisationName);

        Errors = ErrorState.Create(PossibleErrors.All, ErrorId.AccountAdmin_TypeOfRole_MissingSelection);
        return Page();
    }

    private async Task<string> GetOrganisationNameFromId()
    {
        var organisations = await _directoryClient.GetCachedLaOrganisations();

        ArgumentNullException.ThrowIfNull(organisations);

        return organisations.Single(o => o.Id == HttpContext.GetUserOrganisationId()).Name;
    }
}
