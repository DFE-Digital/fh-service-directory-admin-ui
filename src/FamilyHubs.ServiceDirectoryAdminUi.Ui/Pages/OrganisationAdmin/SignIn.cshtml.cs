using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata.Ecma335;
using static BCrypt.Net.BCrypt;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin
{
    public class SignInModel : PageModel
    {
        private readonly ISessionService _session;
        private readonly IRedisCacheService _redis;
        private readonly IConfiguration _configuration;

        [BindProperty]
        public string Email { get; set; } = string.Empty;
        [BindProperty, Required(AllowEmptyStrings = false, ErrorMessage = "Please enter the Password")]
        public string Password { get; set; } = string.Empty;

        public SignInModel(ISessionService sessionService, IRedisCacheService redis, IConfiguration configuration)
        {
            _session = sessionService;
            _redis = redis;
            _configuration = configuration;
        }
        public void OnGet()
        {
            _redis.StoreCurrentPageName("SignIn");
        }

        public IActionResult OnPost()
        {
            if (!ValidatePassword())
            {
                ModelState.AddModelError(nameof(Password), "Enter a valid password");
                Password= string.Empty;
                return Page();
            }

            return RedirectToPage("/OrganisationAdmin/ChooseOrganisation");
        }

        private bool ValidatePassword() => Verify(Password, _configuration.GetValue<string>("PasswordHash"));

    }
}
