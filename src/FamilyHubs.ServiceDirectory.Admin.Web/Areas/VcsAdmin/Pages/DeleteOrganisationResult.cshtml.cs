using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages.ManagePermissions
{
    public class DeleteOrganisationResultModel : HeaderPageModel
    {
        private readonly ICacheService _cacheService;

        public bool OrganisationDeleted { get; set; }
        public string OrganisationName { get; set; }  = string.Empty;
        public string PageHeading { get; set; } = string.Empty;

        public DeleteOrganisationResultModel(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        public async Task OnGet(bool isDeleted)
        {
            OrganisationDeleted = isDeleted;
            OrganisationName = await _cacheService.RetrieveString("DeleteOrganisationName");
            if ( isDeleted )
            {
                PageHeading = $"You have deleted {OrganisationName}";
            }
            else
            {
                PageHeading = $"You have not deleted {OrganisationName}";
            }
        }
    }
}
