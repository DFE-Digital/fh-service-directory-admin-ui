using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages;

public class IndexModel : HeaderPageModel
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