using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Newtonsoft.Json.Linq;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin
{
    public class SignInModel : PageModel
    {
        private readonly ISessionService _session;
        private readonly IRedisCacheService _redis;
        private readonly IAuthService _authenticationService;
        private readonly ITokenService _tokenService;

        [BindProperty]
        public string Email { get; set; } = string.Empty;
        [BindProperty]
        public string Password { get; set; } = string.Empty;

        public SignInModel(ISessionService sessionService, IRedisCacheService redis, IAuthService authenticationService, ITokenService tokenService)
        {
            _session = sessionService;
            _redis = redis;
            _authenticationService = authenticationService;
            _tokenService = tokenService;
        }
        public IActionResult OnGet()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                OrganisationViewModel? viewModel = _redis.RetrieveOrganisationWithService();
                if (viewModel != null)
                {
                    return RedirectToPage("/OrganisationAdmin/Welcome", new
                    {
                        organisationId = viewModel.Id,
                    });
                }
            }
            

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            Guid organisationId = new Guid("72e653e8-1d05-4821-84e9-9177571a6013");

            var tokenModel = await _authenticationService.Login(Email, Password);
            if (tokenModel != null)
            {

                var handler = new JwtSecurityTokenHandler();
                var jwtSecurityToken = handler.ReadJwtToken(tokenModel.Token);
                var claims = jwtSecurityToken.Claims.ToList();

                var appIdentity = new ClaimsIdentity(claims);
                User.AddIdentity(appIdentity);

                var claim = claims.FirstOrDefault(x => x.Type == "OpenReferralOrganisationId");
                if (claim != null) 
                {
                    organisationId = new Guid(claim.Value);
                }
                //string data = JObject.Parse(json)["id"].ToString();

                //Initialize a new instance of the ClaimsIdentity with the claims and authentication scheme    
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                //Initialize a new instance of the ClaimsPrincipal with ClaimsIdentity    
                var principal = new ClaimsPrincipal(identity);

                _tokenService.SetToken(tokenModel.Token, jwtSecurityToken.ValidTo, tokenModel.RefreshToken);

                //SignInAsync is a Extension method for Sign in a principal for the specified scheme.    
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties()
                {
                    IsPersistent = false //Input.RememberMe,
                });
            }

            OrganisationViewModel organisationViewModel = new()
            {
                Id = organisationId
            };

            _redis.StoreOrganisationWithService(organisationViewModel);

            if (User != null && User.Identity != null)
                _redis.StoreStringValue($"OrganisationId-{User.Identity.Name}", organisationId.ToString());

            return RedirectToPage("/OrganisationAdmin/Welcome", new
            {
                organisationId = organisationId,
            }); 

        }
    }
}
