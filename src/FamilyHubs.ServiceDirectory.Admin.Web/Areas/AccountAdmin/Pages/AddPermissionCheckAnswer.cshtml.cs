using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages;

public class AddPermissionCheckAnswer : PageModel
{
    private readonly ICacheService _cacheService;
    public PermissionModel PermissionModel { get; set; }
    public string WhoFor { get; set; } = string.Empty;
    public string TypeOfPermission { get; set; } = string.Empty;
    public string LocalAuthority { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
	
    public AddPermissionCheckAnswer(ICacheService cacheService)
    {
        _cacheService = cacheService;
        
        var cachedModel = _cacheService.GetPermissionModel();
        ArgumentNullException.ThrowIfNull(cachedModel);
        
        PermissionModel = cachedModel;
    }
    
    public void OnGet()
    {
		WhoFor = "Someone who works for a local authority";
        TypeOfPermission = "Add and manage services, family hubs and accounts";
        LocalAuthority = "Bristol";
        Email = "robert.warpole@firstpm.com";
        Name = "Robert Warpole";
    }
}