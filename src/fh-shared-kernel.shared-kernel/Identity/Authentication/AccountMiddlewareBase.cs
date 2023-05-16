using FamilyHubs.SharedKernel.GovLogin.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Web;

namespace FamilyHubs.SharedKernel.Identity.Authentication
{
    public abstract class AccountMiddlewareBase
    {
        private readonly GovUkOidcConfiguration _configuration;


        public AccountMiddlewareBase(GovUkOidcConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected bool ShouldSignOut(HttpContext httpContext)
        {
            if (httpContext.Request.Path.HasValue && httpContext.Request.Path.Value.Contains(AuthenticationConstants.SignOutPath, StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }

        protected void SetBearerToken(HttpContext httpContext)
        {
            var user = httpContext.User;
            if (!IsUserAuthenticated(user))
                return;

            var key = new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(_configuration.BearerTokenSigningKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var token = new JwtSecurityToken(
                claims: user.Claims,
                signingCredentials: creds,
                expires: DateTime.UtcNow.AddMinutes(_configuration.ExpiryInMinutes)
                );

            httpContext.Items.Add(AuthenticationConstants.BearerToken, new JwtSecurityTokenHandler().WriteToken(token));
        }

        private static bool IsUserAuthenticated(ClaimsPrincipal? user)
        {
            if (user == null) return false;

            if (user.Identity == null) return false;

            return user.Identity.IsAuthenticated;
        }
    }
}
