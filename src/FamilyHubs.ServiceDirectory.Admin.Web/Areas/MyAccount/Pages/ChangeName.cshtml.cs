using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.SharedKernel.Identity.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.MyAccount.Pages
{
    public class ChangeNameModel : MyAccountViewModel
    {
        private readonly IIdamClient _idamClient;
        private readonly ICacheService _cacheService;

        [BindProperty]
        public string FullName { get; set; } = string.Empty;        

        public ChangeNameModel(IIdamClient idamClient, ICacheService cacheService)
        {
            PreviousPageLink = "/ViewPersonalDetails";
            ErrorMessage = "Enter a name";
            PageHeading = "Change your name";
            HasBackButton = true;
            _idamClient = idamClient;
            _cacheService = cacheService;
        }

        public void OnGet()
        {
            var familyHubsUser = HttpContext.GetFamilyHubsUser();
            FullName = familyHubsUser.FullName;
        }

        public async Task<IActionResult> OnPost()
        {
            
            if (ModelState.IsValid && !string.IsNullOrWhiteSpace(FullName) && FullName.Length <= 255)
            {
                var familyHubsUser = HttpContext.GetFamilyHubsUser();

                var request = new UpdateAccountDto {
                    AccountId = long.Parse(familyHubsUser.AccountId),
                    Name = FullName,
                    Email = familyHubsUser.Email
                };
                await _idamClient.UpdateAccount(request);

                HttpContext.RefreshClaims();

                return RedirectToPage("ChangeNameConfirmation");
            }

            HasValidationError = true;

            return Page();
        }
    }
}
