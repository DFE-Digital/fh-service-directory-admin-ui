using FamilyHubs.SharedKernel.GovLogin.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace FamilyHubs.SharedKernel.Identity.SigningKey
{
    public class StubSigningKeyProvider : ISigningKeyProvider
    {
        private readonly string _key;

        public StubSigningKeyProvider(GovUkOidcConfiguration govUkOidcConfiguration)
        {
            _key = govUkOidcConfiguration.StubAuthentication.PrivateKey;
        }

        public SecurityKey GetBearerTokenSigningKey()
        {
            return new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(_key));
        }
    }
}
