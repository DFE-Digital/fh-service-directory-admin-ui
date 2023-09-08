using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.ServiceWizzard.Pages;

public class ErrorServiceModel : PageModel
{
    public IActionResult OnPost()
    {
        return RedirectToPage("Welcome", new { area = "ServiceWizzard" });
    }
}
