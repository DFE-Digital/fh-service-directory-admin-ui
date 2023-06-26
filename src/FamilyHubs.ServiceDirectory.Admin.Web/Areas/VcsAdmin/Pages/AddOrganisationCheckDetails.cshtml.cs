using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.VcsAdmin.Pages
{
    public class AddOrganisationCheckDetailsModel : CheckDetailsViewModel
    {
        private ICacheService _cacheService;
        public IServiceDirectoryClient _serviceDirectoryClient { get; }

        [BindProperty]
        public string OrganisationName { get; set; } = string.Empty;
        

        public AddOrganisationCheckDetailsModel(ICacheService cacheService, IServiceDirectoryClient serviceDirectoryClient)
        {
            _cacheService = cacheService;
            _serviceDirectoryClient = serviceDirectoryClient;
        }

        public async Task OnGet()
        {
            OrganisationName = await _cacheService.RetrieveString(CacheKeyNames.AddOrganisationName);
        }

        public async Task<IActionResult> OnPost()
        {
            //SetBackButtonPath();

            var organisationName = await _cacheService.RetrieveString(CacheKeyNames.AddOrganisationName);

            var organisation = new OrganisationWithServicesDto
            {
                AdminAreaCode = await _cacheService.RetrieveString(CacheKeyNames.AdminAreaCode),
                Name = organisationName,
                OrganisationType = OrganisationType.VCFS,
                Description = organisationName,
                AssociatedOrganisationId = long.Parse(await _cacheService.RetrieveString(CacheKeyNames.LaOrganisationId))
            };
            await _serviceDirectoryClient.CreateOrganisation(organisation);
            return RedirectToPage("/AddOrganisationResult");
        }
    }
}
