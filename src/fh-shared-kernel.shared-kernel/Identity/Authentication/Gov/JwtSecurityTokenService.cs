using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FamilyHubs.SharedKernel.Identity.Authentication.Gov
{
    public interface IJwtSecurityTokenService
    {
        string CreateToken(string clientId, string audience, ClaimsIdentity claimsIdentity,
            SigningCredentials signingCredentials);
    }
    internal class JwtSecurityTokenService : IJwtSecurityTokenService
    {
        public string CreateToken(string clientId, string audience, ClaimsIdentity claimsIdentity,
            SigningCredentials signingCredentials)
        {
            var handler = new JwtSecurityTokenHandler();
            var value = handler.CreateJwtSecurityToken(clientId, audience, claimsIdentity, DateTime.UtcNow,
                DateTime.UtcNow.AddMinutes(5), DateTime.UtcNow, signingCredentials);

            return value.RawData;
        }
    }
}
