using FamilyHubs.ServiceDirectory.Admin.Core.Helpers;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages.ManagePermissions
{
    public class EditEmailModel : InputPageViewModel
    {
        [BindProperty(SupportsGet = true)]
        public string AccountId { get; set; } = string.Empty; //Route Property

        [BindProperty]
        public required string EmailAddress { get; set; } = string.Empty;

        public EditEmailModel()
        {
            PageHeading = "What's their email address?";
            ErrorMessage = "Enter an email address";
            SubmitButtonPath = "/AddOrganisationCheckDetails";
            SubmitButtonText = "Confirm";
            HintText = "They will use this to sign in to their account.";
        }

        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPost()
        {
            if (ModelState.IsValid && ValidationHelper.IsValidEmail(EmailAddress))
            {
                //  Post to api
                return RedirectToPage(SubmitButtonPath);
            }

            HasValidationError = true;
            return Page();
        }

    }
}
