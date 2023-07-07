using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.MyAccount.Pages
{
    public class ChangeNameConfirmationModel : MyAccountViewModel
    {
        private readonly ICacheService _cacheService;

        public ChangeNameConfirmationModel(ICacheService cacheService)
        {
            HasBackButton = false;
            _cacheService = cacheService;
        }
        public string NewName { get; set; }
        public async Task OnGet()
        {            
            NewName = (await _cacheService.RetrieveFamilyHubsUser()).FullName;
            
        }
    }
}
