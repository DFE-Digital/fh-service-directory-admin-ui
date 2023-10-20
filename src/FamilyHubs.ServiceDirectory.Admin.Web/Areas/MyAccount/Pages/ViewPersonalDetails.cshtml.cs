using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.SharedKernel.Identity;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.MyAccount.Pages;

public class ViewPersonalDetails : HeaderPageModel
{
    public string? FullName { get; set; }
    public string? GovOneLoginAccountPage { get; set; }

    public ViewPersonalDetails(IConfiguration configuration)
    {
        GovOneLoginAccountPage = configuration.GetValue<string>("GovUkLoginAccountPage")!;
    }

    public void OnGet()
    {
        var familyHubsUser = HttpContext.GetFamilyHubsUser();
        FullName = familyHubsUser.FullName;
    }
}