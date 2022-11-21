using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

[Authorize(Policy = "ServiceMaintainer")]
public class ServiceNotDeletedModel : PageModel
{
    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        return RedirectToPage("/OrganisationAdmin/Welcome");
    }
}
