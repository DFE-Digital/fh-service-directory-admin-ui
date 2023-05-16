using FamilyHubs.SharedKernel.GovLogin.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace FamilyHubs.SharedKernel.Identity.SigningKey
{
    /// <summary>
    /// Provides the signing key from local configuration (appsettings.json - GovUkOidcConfiguration.Oidc.PrivateKey)
    /// </summary>
    public class LocalSigningKeyProvider : ISigningKeyProvider
    {
        private readonly string _privateKey;

        public LocalSigningKeyProvider(GovUkOidcConfiguration govUkOidcConfiguration)
        {
            _privateKey = govUkOidcConfiguration.Oidc.PrivateKey!;
        }

        public SecurityKey GetBearerTokenSigningKey()
        {
            return new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(_privateKey));
        }
    }
}
