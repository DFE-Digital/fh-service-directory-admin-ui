using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages;

//todo: all pages seem to assume that user has been through the welcome page by accessing data from the cache, but not handling when it's not there
// which means that if a user bookmarks a page, they'll get an error (or if the cache expires, although that would depend on cache timeout vs login timeout)

public enum MenuPage
{
    La,
    Vcs,
    Dfe
}

[Authorize(Roles=RoleGroups.AdminRole)]
public class WelcomeModel : HeaderPageModel
{
    public MenuPage MenuPage { get; set; }

    public string? Heading { get; set; }
    public string? SubHeading { get; set; }

    private readonly ICacheService _cacheService;
    private readonly IServiceDirectoryClient _serviceDirectoryClient;

    public WelcomeModel(
        ICacheService cacheService,
        IServiceDirectoryClient serviceDirectoryClient)
    {
        _cacheService = cacheService;
        _serviceDirectoryClient = serviceDirectoryClient;
    }

    public async Task OnGet()
    {
        var familyHubsUser = HttpContext.GetFamilyHubsUser();
        SetVisibleSections(familyHubsUser.Role);

        Heading = familyHubsUser.FullName;
        //todo: no magic strings
        if (familyHubsUser.OrganisationId == "-1")
        {
            SubHeading = "Department for Education";
        }
        else
        {
            if (long.TryParse(familyHubsUser.OrganisationId, out var organisationId))
            {
                //todo: looks like we get the organisation with *all* it's services, just so that we can use the name!
                var organisation = await _serviceDirectoryClient.GetOrganisationById(organisationId);
                SubHeading = organisation.Name;
            }
        }

        await _cacheService.ResetLastPageName();
    }

    private void SetVisibleSections(string role)
    {
        switch (role)
        {
            case RoleTypes.DfeAdmin:
                MenuPage = MenuPage.Dfe;
                break;

            case RoleTypes.LaDualRole:
            case RoleTypes.LaManager:
                MenuPage = MenuPage.La;
                break;

            case RoleTypes.VcsDualRole:
            case RoleTypes.VcsManager:
                MenuPage = MenuPage.Vcs;
                break;

            default:
                throw new InvalidOperationException($"Unknown role: {role}");
        }
    }

    public IActionResult OnGetAddPermissionFlow()
    {
        _cacheService.StoreUserFlow("AddPermissions");
        return RedirectToPage("/TypeOfRole", new { area = "AccountAdmin", cacheid = Guid.NewGuid() });
    }

    public async Task<IActionResult> OnGetAddOrganisation()
    {
        await _cacheService.StoreUserFlow("AddOrganisation");
        await _cacheService.ResetString(CacheKeyNames.LaOrganisationId);
        await _cacheService.ResetString(CacheKeyNames.AddOrganisationName);
        return RedirectToPage("/AddOrganisationWhichLocalAuthority", new { area = "vcsAdmin" });
    }
}
