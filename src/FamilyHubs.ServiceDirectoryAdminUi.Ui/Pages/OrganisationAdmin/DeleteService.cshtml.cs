using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;
public class DeleteServiceModel : PageModel
{
    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost()
    {
        //TODO - just a stub - delete it
        return Page();
    }
}
