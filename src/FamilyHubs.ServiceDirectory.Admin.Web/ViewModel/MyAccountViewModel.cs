using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.ViewModel
{
    public class MyAccountViewModel : PageModel
    {

        public string PreviousPageLink { get; set; } = string.Empty;
        public string GovOneLoginAccountPage { get; set; } = string.Empty;
    }
}
