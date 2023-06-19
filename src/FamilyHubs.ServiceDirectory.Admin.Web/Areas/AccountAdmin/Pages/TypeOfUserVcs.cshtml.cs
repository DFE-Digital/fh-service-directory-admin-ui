using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages;

public class TypeOfUserVcs : AccountAdminViewModel
{
    public TypeOfUserVcs(ICacheService cacheService) : base(nameof(TypeOfUserVcs), cacheService)
    {
        PageHeading = "What do they need to do?";
        ErrorMessage = "Select what they need to do";
    }
    
    [BindProperty]
    public bool VcsProfessional { get; set; }

    [BindProperty]
    public bool VcsManager { get; set; }

    public override async Task OnGet()
    {
        await base.OnGet();
        
        VcsManager = PermissionModel.VcsManager;
        VcsProfessional = PermissionModel.VcsProfessional;
    }
    
    public override async Task<IActionResult> OnPost()
    {
        await base.OnPost();
        
        if (ModelState.IsValid && (VcsManager || VcsProfessional))
        {
            PermissionModel.LaManager = false;
            PermissionModel.LaProfessional = false;

            PermissionModel.VcsProfessional = VcsProfessional;
            PermissionModel.VcsManager = VcsManager;
            await CacheService.StorePermissionModel(PermissionModel);
            
            return RedirectToPage(NextPageLink);
        }
        
        HasValidationError = true;
        return Page();
    }
}