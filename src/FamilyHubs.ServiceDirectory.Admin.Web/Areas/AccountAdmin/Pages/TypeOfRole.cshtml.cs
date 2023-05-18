using Microsoft.AspNetCore.Mvc;
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
            //return RedirectToPage("/what-local-authority", new { Areas = "AccountAdmin" });
            return Page();
        }
        
        HasValidationError = true;
        return Page();
    }
}