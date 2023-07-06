using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.MyAccount.Pages
{
    public class ChangeNameModel : MyAccountViewModel
    {
        private readonly IIdamClient _idamClient;

        [BindProperty]
        public string FullName { get; set; } = string.Empty;

        public ChangeNameModel(IIdamClient idamClient)
        {
            PreviousPageLink = "/ViewPersonalDetails";
            ErrorMessage = "Enter a name";
            PageHeading = "Change your name";
            _idamClient = idamClient;
        }

        public void OnGet()
        {
            FullName = HttpContext.GetFamilyHubsUser().FullName;
        }

        public async Task<IActionResult> OnPost()
        {
            if (ModelState.IsValid && !string.IsNullOrWhiteSpace(FullName) && FullName.Length <= 255)
            {
                var request = new UpdateAccountDto {
                    AccountId = long.Parse(HttpContext.GetFamilyHubsUser().AccountId),
                    Name = FullName
                };
                await _idamClient.UpdateAccount(request);
                return RedirectToPage("ChangeNameConfirmation");
            }

            HasValidationError = true;

            return Page();
        }
    }
}
