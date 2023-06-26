using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.ViewModel
{
    public class CheckDetailsViewModel : PageModel
    {
        public string BackButtonPath { get; set; } = string.Empty;
        public string ContinuePath { get; set; } = string.Empty;
    }
}
