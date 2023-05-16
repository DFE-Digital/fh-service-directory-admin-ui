using Microsoft.IdentityModel.Tokens;

namespace FamilyHubs.SharedKernel.Identity.SigningKey
{
    public interface ISigningKeyProvider
    {
        public SecurityKey GetBearerTokenSigningKey();
    }
}
