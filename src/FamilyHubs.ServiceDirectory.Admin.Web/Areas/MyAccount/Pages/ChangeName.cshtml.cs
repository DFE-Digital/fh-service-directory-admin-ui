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

        public async Task OnGet()
        {            
            FullName = (await _cacheService.RetrieveFamilyHubsUser()).FullName;
        }

        public async Task<IActionResult> OnPost()
        {
            
            if (ModelState.IsValid && !string.IsNullOrWhiteSpace(FullName) && FullName.Length <= 255)
            {
                var userDetails = await _cacheService.RetrieveFamilyHubsUser();

                var request = new UpdateAccountDto {
                    AccountId = long.Parse(userDetails.AccountId),
                    Name = FullName,
                    Email = userDetails.Email
                };
                await _idamClient.UpdateAccount(request);

                
                userDetails.FullName = FullName;
                await _cacheService.ResetFamilyHubsUser();
                await _cacheService.StoreFamilyHubsUser(userDetails);

                return RedirectToPage("ChangeNameConfirmation");
            }

            HasValidationError = true;

            return Page();
        }
    }
}
