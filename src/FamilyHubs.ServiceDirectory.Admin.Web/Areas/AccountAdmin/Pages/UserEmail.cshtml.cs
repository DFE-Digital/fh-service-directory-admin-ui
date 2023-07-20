using FamilyHubs.ServiceDirectory.Admin.Core.Helpers;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages;

public class UserEmail : AccountAdminViewModel
{
    public UserEmail(ICacheService cacheService) : base(nameof(UserEmail), cacheService)
    {
        PageHeading = "What's their email address?";
        ErrorMessage = "Enter an email address";
    }

    [BindProperty] 
    public required string EmailAddress { get; set; } = string.Empty;

    public override async Task OnGet()
    {
        await base.OnGet();
        
        EmailAddress = PermissionModel.EmailAddress;
    }
    
    public override async Task<IActionResult> OnPost()
    {
        await base.OnPost();

        if (ModelState.IsValid && ValidationHelper.IsValidEmail(EmailAddress))
        {
            PermissionModel.EmailAddress = EmailAddress;
            await CacheService.StorePermissionModel(PermissionModel, CacheId);
            
            return RedirectToPage(NextPageLink, new { cacheId = CacheId });
        }
        
        HasValidationError = true;
        return Page();
    }

}