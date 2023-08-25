using FamilyHubs.ServiceDirectory.Admin.Core.Exceptions;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.VcsAdmin.Pages
{
    public class AddOrganisationResultModel : PageModel
    {
        public string DisplayMessage { get; set; } = string.Empty;

        public void OnGet()
        {
            var user = HttpContext.GetFamilyHubsUser();
            switch (user.Role)
            {
                case RoleTypes.DfeAdmin:
                    DisplayMessage = "You can now create user accounts for the organisation. You can also add its services to the directory.";
                    break;

                case RoleTypes.LaDualRole:
                case RoleTypes.LaManager:
                    DisplayMessage = "Before this organisation can accept support requests for its services, you need to add a user.";
                    break;
                default:
                    throw new IdentityException($"Invalid Role {user.Role} for Add Organisation results page");
            }
        }
    }
}
