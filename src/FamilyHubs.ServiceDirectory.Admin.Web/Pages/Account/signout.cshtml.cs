using FamilyHubs.SharedKernel.GovLogin.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.Account
{
    public class signoutModel : PageModel
    {
        public async Task<SignOutResult> OnGet()
        {
            return await HttpContext.GovSignOut();
        }
    }
}
