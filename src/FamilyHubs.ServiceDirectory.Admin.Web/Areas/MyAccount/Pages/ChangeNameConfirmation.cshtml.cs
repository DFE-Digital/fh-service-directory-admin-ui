using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using FamilyHubs.SharedKernel.Identity;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.MyAccount.Pages;

public class ChangeNameConfirmationModel : MyAccountViewModel
{
    public string? NewName { get; set; }

    public void OnGet()
    {
        NewName = HttpContext.GetFamilyHubsUser().FullName; 
    }
}