using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.MyAccount.Pages
{
    public class ChangeNameModel : MyAccountViewModel
    {
        
        public ChangeNameModel()
        {
            PreviousPageLink = "/ViewPersonalDetails";
            ErrorMessage = "Enter a name";
            PageHeading = "Change your name";
        }

        public void OnGet()
        {
            FullName = HttpContext.GetFamilyHubsUser().FullName;
        }

        public IActionResult OnPost()
        {
            if(ModelState.IsValid && !string.IsNullOrWhiteSpace(FullName))
            {

            }

            HasValidationError = true;

            return Page();
        }
    }
}
