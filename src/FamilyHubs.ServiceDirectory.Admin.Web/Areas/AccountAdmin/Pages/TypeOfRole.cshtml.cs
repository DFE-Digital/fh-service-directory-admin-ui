using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages;

public class TypeOfRole : AccountAdminViewModel
{
    private readonly ICacheService _cacheService;

    public TypeOfRole(ICacheService cacheService)
    {
        _cacheService = cacheService;
        PageHeading = "Who are you adding permissions for?";
        ErrorMessage = "Select who you are adding permissions for";
        BackLink = "/Welcome";
    }
    
    [BindProperty]
    public required string OrganisationType { get; set; }
    
    public async Task OnGet()
    {
        var permissionModel = await _cacheService.GetPermissionModel();
        if (permissionModel is not null)
        {
            OrganisationType = permissionModel.OrganisationType;
        }
    }
    public async Task<IActionResult> OnPost()
    {
        if (ModelState.IsValid)
        {
            await _cacheService.StorePermissionModel(new PermissionModel
            {
                OrganisationType = OrganisationType
            });

            return RedirectToPage(OrganisationType == "LA" ? "/TypeOfUserLa" : "/TypeOfUserVcs");
        }
        
        HasValidationError = true;
        return Page();
    }
}