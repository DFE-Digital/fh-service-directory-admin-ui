using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages;

public class TypeOfUserLa : AccountAdminViewModel
{
    private readonly ICacheService _cacheService;

    public TypeOfUserLa(ICacheService cacheService)
    {
        _cacheService = cacheService;
        PageHeading = "What do they need to do?";
        ErrorMessage = "Select what they need to do";
        BackLink = "/TypeOfRole";
    }
    
    [BindProperty]
    public required string UserTypeForLa { get; set; }

    public async Task OnGet()
    {
        var permissionModel = await _cacheService.GetPermissionModel();
        if (permissionModel is not null)
        {
            UserTypeForLa = permissionModel.LaAdmin ? "Admin" : permissionModel.LaProfessional ? "Professional" : string.Empty;
        }
    }
    
    public async Task<IActionResult> OnPost()
    {
        if (ModelState.IsValid)
        {
            var permissionModel = await _cacheService.GetPermissionModel();
            ArgumentNullException.ThrowIfNull(permissionModel);

            permissionModel.LaAdmin = UserTypeForLa == "Admin";
            permissionModel.LaProfessional = UserTypeForLa == "Professional";
            await _cacheService.StorePermissionModel(permissionModel);
            
            return RedirectToPage("/WhichLocalAuthority");
        }
        
        HasValidationError = true;
        return Page();
    }
}