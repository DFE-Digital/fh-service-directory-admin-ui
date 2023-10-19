using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;

namespace FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;

public class MyAccountViewModel : HeaderPageModel
{                
    public string PreviousPageLink { get; set; } = string.Empty;
    public string GovOneLoginAccountPage { get; set; } = string.Empty;
}