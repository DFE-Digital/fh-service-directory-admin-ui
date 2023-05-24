﻿using FamilyHubs.ServiceDirectory.Admin.Core.Services;
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
    public bool VcsAdmin { get; set; }

    public void OnGet()
    {
        var permissionModel = _cacheService.GetPermissionModel();
        if (permissionModel is not null)
        {
            VcsAdmin = permissionModel.VcsAdmin;
            VcsProfessional = permissionModel.VcsProfessional;
        }
    }
    
    public IActionResult OnPost()
    {
        if (ModelState.IsValid && (VcsAdmin || VcsProfessional))
        {
            var permissionModel = _cacheService.GetPermissionModel();
            ArgumentNullException.ThrowIfNull(permissionModel);
            
            permissionModel.VcsProfessional = VcsProfessional;
            permissionModel.VcsAdmin = VcsAdmin;
            _cacheService.StorePermissionModel(permissionModel);
            
            return RedirectToPage("/WhichLocalAuthority");
        }
        
        HasValidationError = true;
        return Page();
    }
}