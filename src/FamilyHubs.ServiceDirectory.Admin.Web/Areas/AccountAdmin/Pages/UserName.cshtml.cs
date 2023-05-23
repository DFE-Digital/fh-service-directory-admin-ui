using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages;

public class UserName : AccountAdminViewModel
{
    public UserName()
    {
        PageHeading = "What is the user's full name?";
        ErrorMessage = "Enter a name";
        BackLink = "/UserEmail";
    }
    
    [BindProperty]
    public required string FullName { get; set; } = string.Empty; 
    
    public void OnGet()
    {
        
    }

    public IActionResult OnPost()
    {
        if (ModelState.IsValid && !string.IsNullOrWhiteSpace(FullName) && FullName.Length <= 255)
        {
            return RedirectToPage("/AddPermissionCheckAnswer");
        }
        
        HasValidationError = true;
        return Page();
    }
}