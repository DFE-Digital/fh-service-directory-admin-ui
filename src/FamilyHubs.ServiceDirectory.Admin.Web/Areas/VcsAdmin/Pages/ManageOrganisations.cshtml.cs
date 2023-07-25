using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.VcsAdmin.Pages
{
    public class ManageOrganisationsModel : PageModel
    {
        public IActionResult OnGet()
        {
            return RedirectToPage("PlaceHolder");
        }
    }
}
