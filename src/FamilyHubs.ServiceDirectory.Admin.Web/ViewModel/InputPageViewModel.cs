using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;

namespace FamilyHubs.ServiceDirectory.Admin.Web.ViewModel
{
    public class InputPageViewModel : PageModel
    {
        [BindProperty]
        public string BackButtonPath { get; set; } = string.Empty;
        public string SubmitButtonPath { get; set; } = string.Empty;
        public string SubmitButtonText { get; set; } = "Continue";
        public bool HasValidationError { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string ErrorElementId { get; set; } = string.Empty;
        public string PageHeading { get; set; } = string.Empty;
        public string HintText { get; set; } = string.Empty;

        /// <summary>
        /// Sets the back button path using HttpContext
        /// </summary>
        protected void SetBackButtonPath()
        {
            if (!string.IsNullOrEmpty(BackButtonPath))
            {
                return;
            }

            var host = GetHost();
            var path = HttpContext.Request.Headers.Referer;

            if(path.IsNullOrEmpty())
            {
                throw new Exception("Request does not contain a path");
            }

            BackButtonPath = path.ToString().Substring(path.ToString().IndexOf(host) + host.Length);
        }

        private string GetHost()
        {
            var host = HttpContext.Request.Host.Value;

            if (HttpContext.Request.Headers.ContainsKey("X-Forwarded-Host"))
            {
                var xforwardedhost = HttpContext.Request.Headers["X-Forwarded-Host"];
                if (!xforwardedhost.IsNullOrEmpty())
                {
                    host = xforwardedhost.ToString();
                }
            }

            return host;
            
        }

    }
}
