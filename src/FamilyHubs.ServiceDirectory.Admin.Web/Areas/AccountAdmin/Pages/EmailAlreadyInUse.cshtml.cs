using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages
{
    public class EmailAlreadyInUseModel : PageModel
    {
        private readonly ICacheService _cacheService;

        public string PreviousPageLink { get; set; } = string.Empty;

        public EmailAlreadyInUseModel(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        public async Task OnGet()
        {
            PreviousPageLink = await _cacheService.RetrieveLastPageName();
        }
    }
}
