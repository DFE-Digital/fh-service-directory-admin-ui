using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages;

public class UserEmail : AccountAdminView
{
    public UserEmail()
    {
        PageHeading = "What's their email address?";
        ErrorMessage = "Enter an email address";
        BackLink = "/UserName";
    }
    
    [BindProperty]
    public required string EmailAddress { get; set; } = string.Empty; 
    
    public void OnGet()
    {
        
    }

    public IActionResult OnPost()
    {
        if (ModelState.IsValid && !string.IsNullOrWhiteSpace(EmailAddress) && EmailAddress.Length <= 255 && new EmailAddressAttribute().IsValid(EmailAddress))
        {
            return RedirectToPage("/AddPermissionCheckAnswer");
        }
        
        HasValidationError = true;
        return Page();
    }
}