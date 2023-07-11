using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages
{
    public class SignOutModel : PageModel
    {
        private readonly ICacheService _cacheService;

        public SignOutModel(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }
        public void OnGet()
        {
            _cacheService.ClearUserCache();
        }      
    }
}
