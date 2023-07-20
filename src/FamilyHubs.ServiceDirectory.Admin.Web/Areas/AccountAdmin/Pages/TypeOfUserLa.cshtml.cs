using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages;

public class TypeOfUserLa : AccountAdminViewModel
{
    public TypeOfUserLa(ICacheService cacheService) : base(nameof(TypeOfUserLa), cacheService)
    {
        PageHeading = "What do they need to do?";
        ErrorMessage = "Select what they need to do";
    }
    
    [BindProperty]
    public bool LaProfessional { get; set; }

    [BindProperty]
    public bool LaManager { get; set; }

    public override async Task OnGet()
    {
        await base.OnGet();
        
        LaManager = PermissionModel.LaManager;
        LaProfessional = PermissionModel.LaProfessional;
    }
    
    public override async Task<IActionResult> OnPost()
    {
        await base.OnPost();
        
        if (ModelState.IsValid && (LaManager || LaProfessional))
        {
            PermissionModel.LaManager = LaManager;
            PermissionModel.LaProfessional = LaProfessional;
            
            PermissionModel.VcsManager = false;
            PermissionModel.VcsProfessional = false;
            
            PermissionModel.LaOrganisationId = HttpContext.IsUserLaManager() ? HttpContext.GetUserOrganisationId() : 0;
            
            await CacheService.StorePermissionModel(PermissionModel, CacheId);

            return RedirectToPage(NextPageLink, new {cacheId= CacheId});
        }
        
        HasValidationError = true;
        return Page();
    }
}