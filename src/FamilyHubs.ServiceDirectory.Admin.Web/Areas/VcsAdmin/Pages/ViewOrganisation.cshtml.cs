using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.VcsAdmin.Pages
{
    public class ViewOrganisationModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string OrganisationId { get; set; } = string.Empty;

        public IActionResult OnGet()
        {
            return RedirectToPage("PlaceHolder");
        }
    }
}
