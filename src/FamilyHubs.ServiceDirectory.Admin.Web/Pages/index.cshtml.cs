using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages;

public class IndexModel : PageModel
{
    public IActionResult OnGet()
    {
        if (HttpContext.IsUserLoggedIn())
        {
            return RedirectToPage("/Welcome");
        }

        return Page();
    }
}