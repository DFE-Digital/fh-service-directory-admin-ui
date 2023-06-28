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
            ContinuePath = "/AddOrganisationCheckDetails";
        }

        public async Task OnGet(bool changeName = false)
        {            
            if (changeName)
            {
                OrganisationName = await _cacheService.RetrieveString(CacheKeyNames.AddOrganisationName);
            }
            
            SetBackButtonPath();
        }

        public async Task<IActionResult> OnPost()
        {
            SetBackButtonPath();

            if (ModelState.IsValid && !string.IsNullOrWhiteSpace(OrganisationName) && OrganisationName.Length <= 255)
            {
                await _cacheService.StoreString(CacheKeyNames.AddOrganisationName, OrganisationName);
                return RedirectToPage(ContinuePath);
            }

            HasValidationError = true;

            return Page();
        }
    }
}
