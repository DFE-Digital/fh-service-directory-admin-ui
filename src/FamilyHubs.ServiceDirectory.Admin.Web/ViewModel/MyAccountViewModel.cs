using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;

namespace FamilyHubs.ServiceDirectory.Admin.Web.ViewModel
{
    public class MyAccountViewModel : HeaderPageModel
    {                
        public string PreviousPageLink { get; set; } = string.Empty;
        public string GovOneLoginAccountPage { get; set; } = string.Empty;

        public bool HasValidationError { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string ErrorElementId { get; set; } = string.Empty;
        public string PageHeading { get; set; } = string.Empty;
        public bool HasBackButton { get; set; }
    }
}
