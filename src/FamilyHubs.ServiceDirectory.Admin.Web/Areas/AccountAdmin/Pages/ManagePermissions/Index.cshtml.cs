using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages.ManagePermissions
{
    public class IndexModel : PageModel
    {
        private readonly IIdamClient _idamClient;

        public IndexModel(IIdamClient idamClient)
        {
            _idamClient = idamClient;
        }

        public async Task OnGet()
        {
            var users = await _idamClient.GetAccounts(HttpContext.GetUserOrganisationId(), 1);
        }
    }
}
