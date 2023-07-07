using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.MyAccount.Pages
{
    public class ViewPersonalDetails : MyAccountViewModel
    {
        private readonly ICacheService _cacheService;

        public string FullName { get; set; }

        public ViewPersonalDetails(IConfiguration configuration, ICacheService cacheService)
        {
            PreviousPageLink = "/Welcome";
            HasBackButton = true;
            GovOneLoginAccountPage = configuration.GetValue<string>("GovUkLoginAccountPage");
            _cacheService = cacheService;
        }

        public async Task OnGet()
        {
            FullName = (await _cacheService.RetrieveFamilyHubsUser()).FullName;
        }
    }
}
