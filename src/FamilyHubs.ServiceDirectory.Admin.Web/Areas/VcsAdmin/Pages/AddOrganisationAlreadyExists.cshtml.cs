using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.VcsAdmin.Pages
{
    public class AddOrganisationAlreadyExistsModel : HeaderPageModel
    {
        private readonly ICacheService _cacheService;

        public string PreviousPageLink { get; set; } = string.Empty;

        public AddOrganisationAlreadyExistsModel(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        public async Task OnGet()
        {
            PreviousPageLink = await _cacheService.RetrieveLastPageName();
        }
    }
}
