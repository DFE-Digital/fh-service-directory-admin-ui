using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;

public class AccountAdminViewModel : PageModel
{
    public ICacheService CacheService { get; set; }
    public bool HasValidationError { get; set; }

    public string PageHeading { get; set; } = string.Empty;
    
    public string LaRoleTypeLabel { get; set; } = "Someone who works for a local authority";

    public string VcsRoleTypeLabel { get; set; } = "Someone who works for a voluntary and community sector organisation";


    public string ErrorMessage { get; set; } = string.Empty;
    
    public string ErrorElementId { get; set; } = string.Empty;
    
    public string PreviousPageLink { get; set; } = string.Empty;
    public string CurrentPageName { get; set; }
    public string NextPageLink { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string CacheId { get; set; } = string.Empty;

    [FromQuery(Name = "backToCheckDetails")]
    public bool BackToCheckDetails { get; set; } = false;

    public PermissionModel PermissionModel { get; set; } = new PermissionModel();

    public AccountAdminViewModel(string currentPageName, ICacheService cacheService)
    {
        ArgumentException.ThrowIfNullOrEmpty(currentPageName);
        
        CacheService = cacheService;
        CurrentPageName = currentPageName;
    }

    public virtual async Task OnGet()
    {
        if (CurrentPageName == "TypeOfRole")
        {
            SetNavigationLinks(string.Empty, false);
            return;
        }
        
        var permissionModel = await CacheService.GetPermissionModel(CacheId);
        ArgumentNullException.ThrowIfNull(permissionModel);
        PermissionModel = permissionModel;

        SetNavigationLinks(permissionModel.OrganisationType, permissionModel.VcsJourney);
    }

    public virtual async Task<IActionResult> OnPost()
    {
        if (CurrentPageName == "TypeOfRole")
        {
            SetNavigationLinks(string.Empty, false);
            return Page();
        }
        
        var permissionModel = await CacheService.GetPermissionModel(CacheId);
        ArgumentNullException.ThrowIfNull(permissionModel);
        PermissionModel = permissionModel;
        
        SetNavigationLinks(permissionModel.OrganisationType, permissionModel.VcsJourney);

        return Page();
    }
    
    public void SetNavigationLinks(string organisationType, bool isVcsJourney)
    {
        var isUserLaManager = HttpContext.IsUserLaManager();
        
        switch (CurrentPageName)
        {
            case "TypeOfRole" :
            {
                PreviousPageLink = "/Welcome";
                NextPageLink = string.IsNullOrWhiteSpace(organisationType) ? string.Empty : organisationType == "LA" ? "/TypeOfUserLa" : "/TypeOfUserVcs";
                break;
            }
            case "TypeOfUserLa" : {
                PreviousPageLink = "/TypeOfRole";
                NextPageLink = isUserLaManager ? "/UserEmail" : "/WhichLocalAuthority";
                break;
            }
            case "TypeOfUserVcs" : {
                PreviousPageLink = "/TypeOfRole";
                NextPageLink = isUserLaManager ? "/WhichVcsOrganisation" : "/WhichLocalAuthority";
                break;
            }
            case "WhichLocalAuthority" : {
                PreviousPageLink = isVcsJourney ? "/TypeOfUserVcs" : "/TypeOfUserLa";
                NextPageLink = isVcsJourney ? "/WhichVcsOrganisation" : "/UserEmail";
                break;
            }
            case "WhichVcsOrganisation" : {
                PreviousPageLink = isUserLaManager ? "/TypeOfUserVcs" : "/WhichLocalAuthority";
                NextPageLink = "/UserEmail";
                break;
            }
            case "UserEmail" : {
                PreviousPageLink = isVcsJourney ? "/WhichVcsOrganisation" : isUserLaManager ? "/TypeOfUserLa" : "/WhichLocalAuthority";
                NextPageLink = "/UserName";
                break;
            }
            case "UserName" : {
                PreviousPageLink = "/UserEmail";
                NextPageLink = "/AddPermissionCheckAnswer";
                break;
            }
            case "AddPermissionCheckAnswer" : {
                PreviousPageLink = "/UserName";
                NextPageLink = "/Confirmation";
                break;
            }
        }

        if (BackToCheckDetails)
        {
            PreviousPageLink = "/AddPermissionCheckAnswer";
        }
    }

    protected void SetRoleTypeLabelsForCurrentUser(string organisationName)
    {
        if (!HttpContext.IsUserLaManager())
        {
            return;
        }
        LaRoleTypeLabel = $"Someone who works for {organisationName}";
        VcsRoleTypeLabel = $"Someone who works for a voluntary and community sector organisation {organisationName}";
    }
}