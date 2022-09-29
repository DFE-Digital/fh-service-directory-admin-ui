using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

public class ErrorServiceModel : PageModel
{
    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        return RedirectToPage("/OrganisationAdmin/Welcome");
    }
}
