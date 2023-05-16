using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Security.Claims;

namespace FamilyHubs.SharedKernel.Identity.Authorisation
{
    public interface ICustomClaims
    {
        Task<IEnumerable<Claim>> GetClaims(TokenValidatedContext tokenValidatedContext);
    }
}
