using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.VcsAdmin.Pages
{
    public class AddOrganisationCheckDetailsModel : PageModel
    {
        private ICacheService _cacheService;

        public AddOrganisationCheckDetailsModel(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        public void OnGet()
        {
        }
    }
}
