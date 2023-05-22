using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages;

public class TypeOfRole : AccountAdminViewModel
{
    public TypeOfRole()
    {
        PageHeading = "Who are you adding permissions for?";
        ErrorMessage = "Select who you are adding permissions for";
        BackLink = "/Welcome";
    }
    
    [BindProperty]
    public required string RoleForOrganisationType { get; set; }
    
    public void OnGet()
    {
        
    }

    public IActionResult OnPost()
    {
        if (ModelState.IsValid)
        {
            if (RoleForOrganisationType == "LA")
            {
                return RedirectToPage("/TypeOfUserLa");
            }

            return Page();
        }
        
        HasValidationError = true;
        return Page();
    }
}