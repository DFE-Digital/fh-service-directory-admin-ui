using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.MyAccount.Pages
{
    public class ViewPersonalDetails : MyAccountViewModel
    {

        public string FullName { get; set; }

        public ViewPersonalDetails(IConfiguration configuration)
        {
            PreviousPageLink = "/Welcome";
            GovOneLoginAccountPage = configuration.GetValue<string>("GovUkLoginAccountPage");
        }

        public void OnGet()
        {
            FullName = HttpContext.GetFamilyHubsUser().FullName;
        }
    }
}
