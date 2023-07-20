using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages
{
    public class ConfirmationModel : PageModel
    {
        private readonly ICacheService _cacheService;
        public string Name { get; set; } = string.Empty;

        public ConfirmationModel(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        public async Task OnGet(string cacheId)
        {
            var cachedModel = await _cacheService.GetPermissionModel(cacheId);
            ArgumentNullException.ThrowIfNull(cachedModel);
            Name = cachedModel.FullName;
        }
    }
}
