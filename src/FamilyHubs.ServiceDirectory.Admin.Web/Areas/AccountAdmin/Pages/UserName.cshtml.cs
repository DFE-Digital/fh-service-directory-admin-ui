using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages;

public class UserName : AccountAdminViewModel
{
    public UserName(ICacheService cacheService) : base(nameof(UserName), cacheService)
    {
        PageHeading = "What's their full name?";
        ErrorMessage = "Enter a name";
    }

    [BindProperty] 
    public required string FullName { get; set; } = string.Empty;

    public override async Task OnGet()
    {
        await base.OnGet();

        FullName = PermissionModel.FullName;
    }

    public override async Task<IActionResult> OnPost()
    {
        await base.OnPost();

        if (ModelState.IsValid && !string.IsNullOrWhiteSpace(FullName) && FullName.Length <= 255)
        {
            PermissionModel.FullName = FullName;
            await CacheService.StorePermissionModel(PermissionModel);

            return RedirectToPage(NextPageLink);
        }

        HasValidationError = true;
        return Page();
    }
}