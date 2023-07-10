using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.VcsAdmin.Pages
{
    public class AddOrganisationModel : InputPageViewModel
    {
        private ICacheService _cacheService;

        [BindProperty]
        public string OrganisationName { get; set; } = string.Empty;

        public AddOrganisationModel(ICacheService cacheService)
        {
            _cacheService = cacheService;
            PageHeading = "What is the organisations name?";
            ErrorMessage = "Enter the organisation's name";
            SubmitButtonPath = "/AddOrganisationCheckDetails";
        }

        public async Task OnGet(bool changeName = false)
        {            
            if (changeName)
            {
                OrganisationName = await _cacheService.RetrieveString(CacheKeyNames.AddOrganisationName);
            }
            
            var flow = await _cacheService.RetrieveUserFlow();
            if (flow == "AddPermissions") {
                BackButtonPath = "/AccountAdmin/WhichVcsOrganisation";
            }
            else
            {
                SetBackButtonPath();
            }
        }

        public async Task<IActionResult> OnPost()
        {
            SetBackButtonPath();

            if (ModelState.IsValid && !string.IsNullOrWhiteSpace(OrganisationName) && OrganisationName.Length <= 255)
            {
                await _cacheService.StoreString(CacheKeyNames.AddOrganisationName, OrganisationName);
                return RedirectToPage(SubmitButtonPath);
            }

            HasValidationError = true;

            return Page();
        }
    }
}
