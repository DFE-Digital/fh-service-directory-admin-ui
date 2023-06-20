using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;

namespace FamilyHubs.ServiceDirectory.Admin.Web.ViewModel
{
    public class InputPageViewModel : PageModel
    {
        [BindProperty]
        public string BackButtonPath { get; set; } = string.Empty;
        public string ContinuePath { get; set; } = string.Empty;
        public bool HasValidationError { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string ErrorElementId { get; set; } = string.Empty;
        public string PageHeading { get; set; } = string.Empty;

        /// <summary>
        /// Sets the back button path using HttpContext
        /// </summary>
        protected void SetBackButtonPath()
        {
            if (!string.IsNullOrEmpty(BackButtonPath))
            {
                return;
            }

            var host = HttpContext.Request.Host.Value;
            var path = HttpContext.Request.Headers.Referer;

            if(path.IsNullOrEmpty() || string.IsNullOrEmpty(host))
            {
                return;
            }

            BackButtonPath = path.ToString().Substring(path.ToString().IndexOf(host) + host.Length);
        }
    }
}
