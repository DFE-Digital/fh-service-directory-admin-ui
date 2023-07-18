using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using FamilyHubs.SharedKernel.Identity;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.MyAccount.Pages
{
    public class ChangeNameConfirmationModel : MyAccountViewModel
    {
        public ChangeNameConfirmationModel()
        {
            HasBackButton = false;
        }

        public string NewName { get; set; }= string.Empty;

        public void OnGet()
        {
            var familyHubsUser = HttpContext.GetFamilyHubsUser();
            NewName = familyHubsUser.FullName; 
        }
    }
}
