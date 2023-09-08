using FamilyHubs.ServiceDirectory.Admin.Web.Extensions;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages
{
    public class AccessibilityModel : PageModel
    {
        public string PreviousPageLink { get; set; } = string.Empty;

        public void OnGet()
        {
            PreviousPageLink = HttpContext.GetBackButtonPath();
        }
    }
}
