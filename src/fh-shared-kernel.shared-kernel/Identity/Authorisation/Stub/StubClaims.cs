using FamilyHubs.SharedKernel.GovLogin.Configuration;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Security.Claims;


namespace FamilyHubs.SharedKernel.Identity.Authorisation.Stub
{
    public class StubClaims : ICustomClaims
    {
        private List<Claim> _claims = new List<Claim>();

        public StubClaims(GovUkOidcConfiguration govUkOidcConfiguration)
        {
            if(govUkOidcConfiguration.StubAuthentication.StubClaims != null)
            {
                foreach (var claim in govUkOidcConfiguration.StubAuthentication.StubClaims)
                {
                    _claims.Add(new Claim(claim.Name, claim.Value));
                }
            }
        }

        public Task<IEnumerable<Claim>> GetClaims(TokenValidatedContext tokenValidatedContext)
        {
            return Task.FromResult(_claims.AsEnumerable());
        }
    }
}
