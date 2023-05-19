﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages;

public class TypeOfRole : PageModel
{
    [BindProperty]
    public required string RoleForOrganisationType { get; set; }
    
    public bool HasValidationError { get; set; }
    
    public void OnGet()
    {
        
    }

    public IActionResult OnPost()
    {
        if (ModelState.IsValid)
        {
            if (RoleForOrganisationType == "LA")
            {
                return RedirectToPage("/TypeOfUserLa", new { Areas = "AccountAdmin" });
            }

            //return RedirectToPage("/TypeOfUserVcs", new { Areas = "AccountAdmin" });
            return Page();
        }
        
        HasValidationError = true;
        return Page();
    }
}