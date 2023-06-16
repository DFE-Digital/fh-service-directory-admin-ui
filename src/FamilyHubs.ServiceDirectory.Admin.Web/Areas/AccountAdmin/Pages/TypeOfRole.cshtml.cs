using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages;

public class TypeOfRole : AccountAdminViewModel
{
    private readonly IServiceDirectoryClient _directoryClient;

    public TypeOfRole(ICacheService cacheService, IServiceDirectoryClient directoryClient) : base(nameof(TypeOfRole), cacheService)
    {
        _directoryClient = directoryClient;
        PageHeading = "Who are you adding permissions for?";
        ErrorMessage = "Select who you are adding permissions for";
    }

    [BindProperty] 
    public required string OrganisationType { get; set; }
    
    public override async Task OnGet()
    {
        var organisationName = await GetOrganisationNameFromId();

        GetRoleTypeLabelForCurrentUser(organisationName);

        var permissionModel = await CacheService.GetPermissionModel();
        if (permissionModel is not null)
        {
            OrganisationType = permissionModel.OrganisationType;
        }

        //Needs to override the navigation link here because we dont have permission model in cache here 
        SetNavigationLinks(OrganisationType, false);
    }

    public override async Task<IActionResult> OnPost()
    {
        var organisationName = await GetOrganisationNameFromId();

        if (ModelState.IsValid)
        {
            await CacheService.StorePermissionModel(new PermissionModel
            {
                OrganisationType = OrganisationType,
                LaOrganisationName = HttpContext.IsUserLaManager() ? organisationName : string.Empty,
                LaOrganisationId = HttpContext.IsUserLaManager() ? HttpContext.GetUserOrganisationId() : 0
            });

            //Needs to override the navigation link here because we dont have permission model in cache before the page is loaded
            SetNavigationLinks(OrganisationType, false);
            
            return RedirectToPage(NextPageLink);
        }
        
        //Needs to override the navigation link here because we dont have permission model in cache before the page is loaded
        SetNavigationLinks(OrganisationType, false);
        
        GetRoleTypeLabelForCurrentUser(organisationName);

        HasValidationError = true;
        
        return Page();
    }

    private async Task<string> GetOrganisationNameFromId()
    {
        var organisations = await _directoryClient.GetCachedLaOrganisations();

        ArgumentNullException.ThrowIfNull(organisations);

        var organisationName = organisations.Single(o => o.Id == HttpContext.GetUserOrganisationId()).Name;
        
        return organisationName;
    }
}