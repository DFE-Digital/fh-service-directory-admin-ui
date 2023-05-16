using FamilyHubs.SharedKernel.Identity.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FamilyHubs.SharedKernel.Identity
{
    public static class HttpContextExtensions
    {
        public static string GetBearerToken(this HttpContext httpContext)
        {
            if (!httpContext.Items.ContainsKey(AuthenticationConstants.BearerToken))
                return string.Empty;

            var token = httpContext.Items[AuthenticationConstants.BearerToken] as string;

            if(token == null) 
                return string.Empty;

            return token;

        }

        public static async Task<SignOutResult> GovSignOut(this HttpContext httpContext)
        {
            var idToken = await httpContext.GetTokenAsync(AuthenticationConstants.IdToken);

            var authenticationProperties = new AuthenticationProperties();
            authenticationProperties.Parameters.Clear();
            authenticationProperties.Parameters.Add(AuthenticationConstants.IdToken, idToken);

            string[] schemes = { CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme };
            return new SignOutResult(schemes, authenticationProperties );
        }

        public static string GetUrlQueryValue(this HttpContext httpContext, string key)
        {
            var value = httpContext.Request.Query[key].First();

            if(string.IsNullOrEmpty(value))
            {
                throw new Exception($"{key} not found in url query parameters");
            }

            return value;
        }

        public static FamilyHubsUser GetFamilyHubsUser(this HttpContext httpContext)
        {
            var user = new FamilyHubsUser
            {
                Role = GetClaimValue(httpContext, FamilyHubsClaimTypes.Role),
                OrganisationId = GetClaimValue(httpContext, FamilyHubsClaimTypes.OrganisationId),
                AccountStatus = GetClaimValue(httpContext, FamilyHubsClaimTypes.AccountStatus),
                FirstName = GetClaimValue(httpContext, FamilyHubsClaimTypes.FirstName),
                LastName = GetClaimValue(httpContext, FamilyHubsClaimTypes.LastName),
                LoginTime = GetDataTimeClaimValue(httpContext, FamilyHubsClaimTypes.LoginTime),
                Email = GetClaimValue(httpContext, ClaimTypes.Email),
                PhoneNumber = GetClaimValue(httpContext, FamilyHubsClaimTypes.PhoneNumber),
            };

            return user;
        }

        private static string GetClaimValue(HttpContext httpContext, string key)
        {

            var claim = httpContext?.User?.Claims?.FirstOrDefault(x => x.Type == key);
            if (claim != null)
            {
                return claim.Value;
            }

            return string.Empty;
        }

        private static DateTime? GetDataTimeClaimValue(HttpContext httpContext, string key)
        {

            var claim = httpContext?.User?.Claims?.FirstOrDefault(x => x.Type == FamilyHubsClaimTypes.LoginTime);


            if (claim != null && long.TryParse(claim.Value, out var utcNumber))
            {
                return new DateTime(utcNumber);
            }

            return null;
        }
    }
}
