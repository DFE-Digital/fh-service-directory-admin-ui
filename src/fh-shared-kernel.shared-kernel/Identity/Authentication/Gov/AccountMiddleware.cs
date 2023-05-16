using FamilyHubs.SharedKernel.GovLogin.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Web;

namespace FamilyHubs.SharedKernel.Identity.Authentication.Gov
{
    public class AccountMiddleware : AccountMiddlewareBase
    {
        private readonly RequestDelegate _next;
        private readonly GovUkOidcConfiguration _configuration;
        private readonly ILogger<AccountMiddleware> _logger;

        public AccountMiddleware(
            RequestDelegate next, 
            GovUkOidcConfiguration configuration, 
            ILogger<AccountMiddleware> logger) : base(configuration)
        {
            _next = next;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            LogAccountRequests(context);

            if (ShouldSignOut(context))
            {
                await SignOut(context);
                return;
            }

            SetBearerToken(context);
            await _next(context);
        }

        private async Task SignOut(HttpContext httpContext)
        {
            var idToken = await httpContext.GetTokenAsync(AuthenticationConstants.IdToken);
            var postLogOutUrl = HttpUtility.UrlEncode($"{_configuration.AppHost}{AuthenticationConstants.AccountLogoutCallback}");
            var logoutRedirect = $"{_configuration.Oidc.BaseUrl}/logout?id_token_hint={idToken}&post_logout_redirect_uri={postLogOutUrl}";
            httpContext.Response.Redirect(logoutRedirect);
        }

        /// <summary>
        /// Only logs requests related to account activity
        /// </summary>
        private void LogAccountRequests(HttpContext httpContext)
        {
            if (!_configuration.EnableDebugLogging)
                return;

            if (!httpContext.Request.Path.HasValue)
                return;

            if (!httpContext.Request.Path.Value.Contains(AuthenticationConstants.AccountPaths, StringComparison.CurrentCultureIgnoreCase))
                return;

            _logger.LogInformation("Account Request Path:{path} Headers:{@headers}", httpContext.Request.Path.Value, httpContext.Request.Headers);

        }
    }
}
