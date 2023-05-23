using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages;

public class TypeOfUserLa : AccountAdminViewModel
{
    public TypeOfUserLa()
    {
        PageHeading = "What do they need to do?";
        ErrorMessage = "Select who you are adding permissions for";
        BackLink = "/TypeOfRole";
    }
    
    [BindProperty]
    public required string UserTypeForLa { get; set; }
    
    public void OnGet()
    {
        
    }

    public IActionResult OnPost()
    {
        if (ModelState.IsValid)
        {
            return RedirectToPage("/WhichLocalAuthority");
        }
        
        HasValidationError = true;
        return Page();
    }
}