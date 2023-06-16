using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using FamilyHubs.SharedKernel.Identity;
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
    public bool LaProfessional { get; set; }

    [BindProperty]
    public bool LaManager { get; set; }

    public async Task OnGet()
    {
        var permissionModel = await _cacheService.GetPermissionModel();
        ArgumentNullException.ThrowIfNull(permissionModel);

        LaManager = permissionModel.LaManager;
        LaProfessional = permissionModel.LaProfessional;
    }
    
    public async Task<IActionResult> OnPost()
    {
        if (ModelState.IsValid && (LaManager || LaProfessional))
        {
            var permissionModel = await _cacheService.GetPermissionModel();
            ArgumentNullException.ThrowIfNull(permissionModel);

            permissionModel.LaManager = LaManager;
            permissionModel.LaProfessional = LaProfessional;

            permissionModel.VcsManager = false;
            permissionModel.VcsProfessional = false;

            permissionModel.LaOrganisationId = HttpContext.IsUserLaManager() ? HttpContext.GetUserOrganisationId() : 0;
            
            await _cacheService.StorePermissionModel(permissionModel);

            var redirectUrl = HttpContext.IsUserLaManager() ? "/UserEmail" : "/WhichLocalAuthority";
            
            return RedirectToPage(redirectUrl);
        }
        
        HasValidationError = true;
        return Page();
    }
}