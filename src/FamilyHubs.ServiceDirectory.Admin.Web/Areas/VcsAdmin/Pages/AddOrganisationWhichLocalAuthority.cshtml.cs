using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;

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

        public async Task<IActionResult> OnGet()
        {
            if (!HttpContext.IsUserDfeAdmin())
            {
                var userOrganisationId = HttpContext.GetUserOrganisationId();
                await _cacheService.StoreString(CacheKeyNames.LaOrganisationId, userOrganisationId.ToString());
                return RedirectToPage("/AddOrganisation", new {IsLaUser=true});
            }

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

            return Page();

        }

        public async Task<IActionResult> OnPost()
        {
            var laOrganisations = await _serviceDirectoryClient.GetCachedLaOrganisations();
            var laOrganisationsNames = laOrganisations.Select(x=>x.Name).ToList(); 

            if (ModelState.IsValid && !string.IsNullOrWhiteSpace(LaOrganisationName) && LaOrganisationName.Length <= 255
                && laOrganisationsNames.Contains(LaOrganisationName))
            {
                var laOrganisationId = laOrganisations.Single(l => l.Name == LaOrganisationName).Id;
                await _cacheService.StoreString(CacheKeyNames.LaOrganisationId, laOrganisationId.ToString());

                return RedirectToPage("/AddOrganisation");
            }

            HasValidationError = true;

            LocalAuthorities = laOrganisations.Select(l => l.Name).ToList();

            return Page();
        }      

        
    }
}
