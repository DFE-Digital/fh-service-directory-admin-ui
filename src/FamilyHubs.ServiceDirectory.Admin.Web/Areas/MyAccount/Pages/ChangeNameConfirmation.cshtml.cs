using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.SharedKernel.Identity;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.MyAccount.Pages;

public class ChangeNameConfirmationModel : HeaderPageModel
{
    public string? NewName { get; set; }

    public void OnGet()
    {
        NewName = HttpContext.GetFamilyHubsUser().FullName; 
    }
}