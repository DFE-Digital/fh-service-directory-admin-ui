using Azure.Identity;
using Azure.Security.KeyVault.Keys;
using FamilyHubs.SharedKernel.GovLogin.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace FamilyHubs.SharedKernel.Identity.SigningKey
{
    public class KeyVaultSigningKeyProvider : ISigningKeyProvider
    {
        private readonly GovUkOidcConfiguration _configuration;
        private DateTime? _keyLastRetreived;
        private byte[]? _keyBytes;

        public KeyVaultSigningKeyProvider(GovUkOidcConfiguration govUkOidcConfiguration)
        {
            _configuration = govUkOidcConfiguration;
            _keyLastRetreived = null;
            _keyBytes = null;
        }

        public SecurityKey GetBearerTokenSigningKey()
        {
            var bytes = GetKeyBytes();
            return new SymmetricSecurityKey(bytes);
        }

        private byte[] GetKeyBytes()
        {
            if(KeyRequiresRefresh())
            {
                var client = new KeyClient(new Uri(_configuration.Oidc.KeyVault.Url!), new DefaultAzureCredential());
                var key = client.GetKey(_configuration.Oidc.KeyVault.Key);
                _keyBytes = key.Value.Key.K;
                _keyLastRetreived = DateTime.UtcNow;
            }

            return _keyBytes!;
        }

        private bool KeyRequiresRefresh()
        {
            if (_keyBytes == null)
                return true;

            if (_keyLastRetreived == null)
                return true;

            if ((DateTime.Now - _keyLastRetreived).Value.TotalMinutes > _configuration.Oidc.KeyVault.KeyRefreshIntervalMinutes)
                return true;

            return false;
        }
    }
}
