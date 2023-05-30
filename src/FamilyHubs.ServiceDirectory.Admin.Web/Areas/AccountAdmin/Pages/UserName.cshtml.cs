using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages;

public class UserName : AccountAdminViewModel
{
    private readonly ICacheService _cacheService;

    public UserName(ICacheService cacheService)
    {
        _cacheService = cacheService;
        PageHeading = "What is the user's full name?";
        ErrorMessage = "Enter a name";
        BackLink = "/UserEmail";
    }

    [BindProperty] 
    public required string FullName { get; set; } = string.Empty;

    public async Task OnGet()
    {
        var permissionModel = await _cacheService.GetPermissionModel();
        ArgumentNullException.ThrowIfNull(permissionModel);

        FullName = permissionModel.FullName;
    }

    public async Task<IActionResult> OnPost()
    {
        if (ModelState.IsValid && !string.IsNullOrWhiteSpace(FullName) && FullName.Length <= 255)
        {
            var permissionModel = await _cacheService.GetPermissionModel();
            ArgumentNullException.ThrowIfNull(permissionModel);

            permissionModel.FullName = FullName;
            await _cacheService.StorePermissionModel(permissionModel);

            return RedirectToPage("/AddPermissionCheckAnswer");
        }

        HasValidationError = true;
        return Page();
    }
}