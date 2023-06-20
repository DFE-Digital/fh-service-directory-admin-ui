using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.VcsAdmin.Pages
{
    public class AddOrganisationModel : InputPageViewModel
    {
        [BindProperty]
        public string OrganisationName { get; set; } = string.Empty;

        public AddOrganisationModel()
        {
            PageHeading = "What is the organisations name?";
            ErrorMessage = "Enter the organisation's name";
            ContinuePath = "/AddOrganisationCheckDetails";
        }

        public void OnGet()
        {
            SetBackButtonPath();
        }

        public IActionResult OnPost()
        {
            SetBackButtonPath();

            if (ModelState.IsValid && !string.IsNullOrWhiteSpace(OrganisationName) && OrganisationName.Length <= 255)
            {
                return RedirectToPage(ContinuePath);
            }

            HasValidationError = true;

            return Page();
        }
    }
}
