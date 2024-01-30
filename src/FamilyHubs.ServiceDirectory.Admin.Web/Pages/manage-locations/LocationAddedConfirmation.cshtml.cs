using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Authorization;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_locations
{
    [Authorize(Roles = RoleGroups.AdminRole)]
    public class LocationAddedConfirmationModel : HeaderPageModel
    {
        public void OnGet()
        {
            //todo: clear cache here (or in post back on last page)
        }
    }
}
