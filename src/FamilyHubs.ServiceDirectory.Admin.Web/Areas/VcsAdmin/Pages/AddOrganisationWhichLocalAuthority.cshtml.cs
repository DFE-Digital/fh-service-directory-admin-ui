using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages;
using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.VcsAdmin.Pages
{
    public class AddOrganisationWhichLocalAuthorityModel : InputPageViewModel
    {
        [BindProperty]
        public required string LaOrganisationName { get; set; } = string.Empty;

        public required List<string> LocalAuthorities { get; set; } = new List<string>();

        private readonly ICacheService _cacheService;
        private readonly IServiceDirectoryClient _serviceDirectoryClient;

        public AddOrganisationWhichLocalAuthorityModel(ICacheService cacheService, IServiceDirectoryClient serviceDirectoryClient) 
        {
            ErrorMessage = "Select a local authority";
            _cacheService = cacheService;
            _serviceDirectoryClient = serviceDirectoryClient;
            PageHeading = "Which local authority is the organisation in?";
            BackButtonPath = "/Welcome";
        }

        public async Task OnGet()
        {
            var localAuthorities = await _serviceDirectoryClient.GetCachedLaOrganisations();
            LocalAuthorities = localAuthorities.Select(l => l.Name).ToList();

            var laOrganisationId = await _cacheService.RetrieveString(CacheKeyNames.LaOrganisationId);
            if(!string.IsNullOrEmpty(laOrganisationId))
            {
                var laOrganisation = localAuthorities.Where(x => x.Id.ToString() == laOrganisationId).FirstOrDefault();
                if(laOrganisation != null)
                {
                    LaOrganisationName= laOrganisation.Name;
                }

            }

        }

        public async Task<IActionResult> OnPost()
        {
            var laOrganisations = await _serviceDirectoryClient.GetCachedLaOrganisations();

            if (ModelState.IsValid && !string.IsNullOrWhiteSpace(LaOrganisationName) && LaOrganisationName.Length <= 255)
            {
                var laOrganisationId = laOrganisations.Single(l => l.Name == LaOrganisationName).Id;
                await _cacheService.StoreString(CacheKeyNames.LaOrganisationId, laOrganisationId.ToString());

                return RedirectToPage("/AddOrganisation");
            }

            HasValidationError = true;

            LocalAuthorities = laOrganisations.Select(l => l.Name).ToList();

            return Page();
        }

        public string IsSelected(string organisationName)
        {
            if(organisationName == LaOrganisationName)
            {
                return "selected";
            }

            return string.Empty;
        }
    }
}
