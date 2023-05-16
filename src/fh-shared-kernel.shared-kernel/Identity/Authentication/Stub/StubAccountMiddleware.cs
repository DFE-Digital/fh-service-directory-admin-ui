using FamilyHubs.SharedKernel.GovLogin.Configuration;
using FamilyHubs.SharedKernel.Identity.Exceptions;
using FamilyHubs.SharedKernel.Identity.SigningKey;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace FamilyHubs.SharedKernel.Identity.Authentication.Stub
{
    public class StubAccountMiddleware : AccountMiddlewareBase
    {
        private readonly RequestDelegate _next;
        private readonly GovUkOidcConfiguration _configuration;

        public StubAccountMiddleware(
            RequestDelegate next, 
            GovUkOidcConfiguration configuration,
            ISigningKeyProvider signingKeyProvider) : base(configuration, signingKeyProvider)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (ShouldSignOut(context))
            {
                SignOut(context);
                return;
            }

            if (StubLoginPage.ShouldRedirectToStubLoginPage(context))
            {
                await StubLoginPage.RenderStubLoginPage(context, _configuration);
                return;
            }

            if (ShouldCompleteLogin(context))
            {
                CompleteLogin(context);
                return;
            }

            SetBearerToken(context);
            await _next(context);
        }

        private static bool ShouldCompleteLogin(HttpContext context)
        {
            if (context.Request.Path.HasValue && context.Request.Path.Value.Contains(StubConstants.RoleSelectedPath, StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private void CompleteLogin(HttpContext context)
        {
            var userId = context.GetUrlQueryValue("user");

            var user = _configuration.GetStubUsers().First(x => x.User.Email == userId);
            if (user == null)
                throw new Exception("Invalid user selected");

            user.Claims.Add(new Models.AccountClaim { Name=FamilyHubsClaimTypes.LoginTime, Value = DateTime.UtcNow.Ticks.ToString() });
            var json = JsonConvert.SerializeObject(user);

            if (string.IsNullOrWhiteSpace(_configuration.CookieName))
                throw new AuthConfigurationException($"CookieName is not configured in {nameof(GovUkOidcConfiguration)} section of appsettings");

            context.Response.Cookies.Append(_configuration.CookieName, json);

            var redirectUrl = context.GetUrlQueryValue("redirect");
            context.Response.Redirect(redirectUrl);

        }
        private void SignOut(HttpContext httpContext)
        {
            if (string.IsNullOrWhiteSpace(_configuration.CookieName))
                throw new AuthConfigurationException($"CookieName is not configured in {nameof(GovUkOidcConfiguration)} section of appsettings");

            httpContext.Response.Cookies.Delete(_configuration.CookieName);
            httpContext.Response.Redirect(_configuration.Urls.SignedOutRedirect);
        }
    }
}
