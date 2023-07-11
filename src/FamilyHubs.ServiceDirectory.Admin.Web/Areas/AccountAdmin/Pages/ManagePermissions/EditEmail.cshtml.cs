using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Helpers;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages.ManagePermissions
{
    public class EditEmailModel : InputPageViewModel
    {
        private readonly IIdamClient _idamClient;

        [BindProperty(SupportsGet = true)]
        public string AccountId { get; set; } = string.Empty; //Route Property

        [BindProperty]
        public required string EmailAddress { get; set; } = string.Empty;

        public EditEmailModel(IIdamClient idamClient)
        {
            PageHeading = "What's their email address?";
            ErrorMessage = "Enter an email address";
            BackButtonPath = $"/AccountAdmin/ManagePermissions/{AccountId}";
            SubmitButtonText = "Confirm";
            HintText = "They will use this to sign in to their account.";

            _idamClient = idamClient;
        }

        public void OnGet()
        {
            BackButtonPath = $"/AccountAdmin/ManagePermissions/{AccountId}";
            var _ = GetAccountId();
        }

        public async Task<IActionResult> OnPost()
        {
            if (ModelState.IsValid && ValidationHelper.IsValidEmail(EmailAddress))
            {
                var id = GetAccountId();
                var account = await _idamClient.GetAccountById(id);

                if (account == null)
                {
                    throw new Exception("User Account not found");
                }

                var updateDto = new UpdateAccountDto 
                { 
                    AccountId = account.Id,
                    Name = account.Name,
                    Email = EmailAddress,
                };

                await _idamClient.UpdateAccount(updateDto);

                return RedirectToPage("EditEmailChangedConfirmation", new {AccountId = AccountId });
            }

            BackButtonPath = $"/AccountAdmin/ManagePermissions/{AccountId}";
            HasValidationError = true;
            return Page();
        }

        private long GetAccountId()
        {
            if (long.TryParse(AccountId, out long id))
            {
                return id;
            }

            throw new Exception("Invalid AccountId");
        }
    }
}
