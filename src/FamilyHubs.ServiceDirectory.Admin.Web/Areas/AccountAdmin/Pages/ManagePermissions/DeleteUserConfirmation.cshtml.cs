using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages.ManagePermissions
{
    public class DeleteUserConfirmationModel : HeaderPageModel
    {
        private readonly ICacheService _cacheService;

        public bool UserDeleted { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string PageHeading { get; set; } = string.Empty;

        public DeleteUserConfirmationModel(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        public async Task OnGet(bool isDeleted)
        {
            UserDeleted = isDeleted;
            UserName = await _cacheService.RetrieveString("DeleteUserName");
            if ( isDeleted )
            {
                PageHeading = $"You have deleted {UserName}'s permissions";
            }
            else
            {
                PageHeading = $"You have not deleted {UserName}'s permissions";
            }
        }
    }
}
