using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_locations
{
    [Authorize(Roles = RoleGroups.AdminRole)]
    public class LocationAddedConfirmationModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
