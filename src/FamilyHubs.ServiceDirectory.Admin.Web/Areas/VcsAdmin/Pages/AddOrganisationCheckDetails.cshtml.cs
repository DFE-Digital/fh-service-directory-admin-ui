using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Exceptions;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.VcsAdmin.Pages
{
    public class AddOrganisationCheckDetailsModel : CheckDetailsViewModel
    {
        private readonly ICacheService _cacheService;
        public IServiceDirectoryClient _serviceDirectoryClient { get; }

        [BindProperty]
        public string LocalAuthority { get; set; } = string.Empty;

        [BindProperty]
        public string OrganisationName { get; set; } = string.Empty;

        public bool IsAddPermissionFlow { get; set; }

        [BindProperty(SupportsGet = true)]
        public string CacheId { get; set; } = string.Empty;

        public bool IsDfeUser { get; set; } = false;

        public AddOrganisationCheckDetailsModel(ICacheService cacheService, IServiceDirectoryClient serviceDirectoryClient)
        {
            _cacheService = cacheService;
            _serviceDirectoryClient = serviceDirectoryClient;   
        }

        public async Task OnGet()
        {
            var cashIdPath = string.IsNullOrEmpty(CacheId) ? "" : $"&cacheId={CacheId}";
            BackButtonPath = "/VcsAdmin/AddOrganisation?changeName=true" + cashIdPath;

            OrganisationName = await _cacheService.RetrieveString(CacheKeyNames.AddOrganisationName);
            await SetLocalAuthority();
            IsAddPermissionFlow = ("AddPermissions" == await _cacheService.RetrieveUserFlow());
            IsDfeUser = HttpContext.IsUserDfeAdmin();
        }

        public async Task<IActionResult> OnPost()
        {
            var organisationName = await _cacheService.RetrieveString(CacheKeyNames.AddOrganisationName);

            var organisation = new OrganisationWithServicesDto
            {
                AdminAreaCode = await _cacheService.RetrieveString(CacheKeyNames.AdminAreaCode),
                Name = organisationName,
                OrganisationType = OrganisationType.VCFS,
                Description = organisationName,
                AssociatedOrganisationId = long.Parse(await _cacheService.RetrieveString(CacheKeyNames.LaOrganisationId))
            };

            var outcome = await _serviceDirectoryClient.CreateOrganisation(organisation);

            if(outcome.IsSuccess)
            {
                return RedirectToPage("/AddOrganisationResult");
            }

            if (outcome.FailureResult?.ApiErrorResponse.ErrorCode.ParseToErrorCode() == Core.Exceptions.ErrorCodes.AlreadyExistsException)
            {
                await _cacheService.StoreCurrentPageName($"/VcsAdmin/AddOrganisationCheckDetails");
                return RedirectToPage("AddOrganisationAlreadyExists");
            }

            throw new ArgumentException("Unknown response from API ServiceDirectory CreateOrganisation");
        }

        private async Task SetLocalAuthority()
        {
            var localAuthorities = await _serviceDirectoryClient.GetCachedLaOrganisations();
            var laOrganisationId = await _cacheService.RetrieveString(CacheKeyNames.LaOrganisationId);

            if (string.IsNullOrEmpty(laOrganisationId))
            {
                throw new ArgumentException("laOrganisationId missing");
            }

            var laOrganisation = localAuthorities.First(x => x.Id.ToString() == laOrganisationId);
            LocalAuthority = laOrganisation.Name;
        }
    }
}
