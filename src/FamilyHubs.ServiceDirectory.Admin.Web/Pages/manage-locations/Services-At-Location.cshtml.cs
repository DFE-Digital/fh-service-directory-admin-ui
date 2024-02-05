using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Authorization;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_locations
{
    [Authorize(Roles = RoleGroups.AdminRole)]
    public class ServicesAtLocationModel : HeaderPageModel
    {
        public ServicesAtLocationModel()
        {
        }

        public async Task OnGetAsync()
        {

        }
    }
}
