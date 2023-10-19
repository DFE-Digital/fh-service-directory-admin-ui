using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using FamilyHubs.SharedKernel.Identity;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.MyAccount.Pages;

public class ViewPersonalDetails : MyAccountViewModel
{
    public string FullName { get; set; } = string.Empty;

    public ViewPersonalDetails(IConfiguration configuration)
    {
        PreviousPageLink = "/Welcome";
        GovOneLoginAccountPage = configuration.GetValue<string>("GovUkLoginAccountPage")!;
    }

    public void OnGet()
    {
        var familyHubsUser = HttpContext.GetFamilyHubsUser();
        FullName = familyHubsUser.FullName;
    }
}