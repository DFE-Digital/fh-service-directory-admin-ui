using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages
{
    public class AgreeToTermsAndConditionsModel : PageModel
    {
        private readonly ITermsAndConditionsService _termsAndConditionsService;

        [BindProperty]
        public string ReturnPath { get; set; } = string.Empty;

        public AgreeToTermsAndConditionsModel(ITermsAndConditionsService termsAndConditionsService)
        {
            _termsAndConditionsService = termsAndConditionsService;
        }

        public void OnGet(string returnPath)
        {
            ReturnPath = returnPath;
        }

        public async Task<IActionResult> OnPost()
        {
            await _termsAndConditionsService.AcceptTermsAndConditions();
            return Redirect(ReturnPath);
        }
    }
}
