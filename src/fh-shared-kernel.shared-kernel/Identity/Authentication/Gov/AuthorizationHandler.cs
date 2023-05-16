using FamilyHubs.SharedKernel.GovLogin.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace FamilyHubs.SharedKernel.Identity.Authentication.Gov
{
    public class AccountActiveRequirement : IAuthorizationRequirement
    {
    }

    public class AuthorizationHandler : AuthorizationHandler<AccountActiveRequirement>
    {
        private readonly GovUkOidcConfiguration _configuration;

        public AuthorizationHandler(GovUkOidcConfiguration configuration)
        {
            _configuration = configuration;
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AccountActiveRequirement requirement)
        {
            if (context.Resource is HttpContext httpContext)
            {
                var isAccountSuspended = context.User.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.AuthorizationDecision))?.Value;
                if (isAccountSuspended != null && isAccountSuspended.Equals("Suspended", StringComparison.CurrentCultureIgnoreCase))
                {
                    httpContext.Response.Redirect(_configuration.Urls.AccountSuspendedRedirect);
                }
            }
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
