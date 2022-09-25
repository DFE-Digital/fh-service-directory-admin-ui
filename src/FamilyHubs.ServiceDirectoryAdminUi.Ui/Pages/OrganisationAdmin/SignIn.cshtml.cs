using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin
{
    public class SignInModel : PageModel
    {
        private readonly ISessionService _session;

        [BindProperty]
        public string Email { get; set; } = string.Empty;
        [BindProperty]
        public string Password { get; set; } = string.Empty;

        public SignInModel(ISessionService sessionService)
        {
            _session = sessionService;
        }
        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            OrganisationViewModel organisationViewModel = new()
            {
                Id = new Guid("72e653e8-1d05-4821-84e9-9177571a6013")
            };

            organisationViewModel.Name = "Bristol City Council";

            _session.StoreService(HttpContext, organisationViewModel);

            return RedirectToPage("/OrganisationAdmin/Welcome");



            //OrganisationViewModel organisationViewModel = new()
            //{
            //    Id = new Guid("72e653e8-1d05-4821-84e9-9177571a6013")
            //};

            //organisationViewModel.Name = "Bristol City Council";

            //var strOrganisationViewModel = JsonConvert.SerializeObject(organisationViewModel);

            //return RedirectToPage("/OrganisationAdmin/Welcome", new
            //{
            //    strOrganisationViewModel = strOrganisationViewModel
            //});

        }
    }
}
