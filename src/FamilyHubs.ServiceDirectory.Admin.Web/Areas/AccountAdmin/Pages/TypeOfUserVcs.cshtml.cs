using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages;

public class TypeOfUserVcs : AccountAdminViewModel
{
    private readonly ICacheService _cacheService;

    public TypeOfUserVcs(ICacheService cacheService)
    {
        _cacheService = cacheService;
        PageHeading = "What do they need to do?";
        ErrorMessage = "Select what they need to do";
        BackLink = "/TypeOfRole";
    }
    
    [BindProperty]
    public bool VcsProfessional { get; set; }

    [BindProperty]
    public bool VcsManager { get; set; }

    public async Task OnGet()
    {
        var permissionModel = await _cacheService.GetPermissionModel();
        ArgumentNullException.ThrowIfNull(permissionModel);
        
        VcsManager = permissionModel.VcsManager;
        VcsProfessional = permissionModel.VcsProfessional;
    }
    
    public async Task<IActionResult> OnPost()
    {
        if (ModelState.IsValid && (VcsManager || VcsProfessional))
        {
            var permissionModel = await _cacheService.GetPermissionModel();
            ArgumentNullException.ThrowIfNull(permissionModel);
            
            permissionModel.LaManager = false;
            permissionModel.LaProfessional = false;

            permissionModel.VcsProfessional = VcsProfessional;
            permissionModel.VcsManager = VcsManager;
            await _cacheService.StorePermissionModel(permissionModel);
            
            return RedirectToPage("/WhichLocalAuthority");
        }
        
        HasValidationError = true;
        return Page();
    }
}