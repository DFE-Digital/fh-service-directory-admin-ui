using Azure.Identity;
using Azure.Security.KeyVault.Keys;
using FamilyHubs.SharedKernel.GovLogin.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace FamilyHubs.SharedKernel.Identity.SigningKey
{
    public class KeyVaultSigningKeyProvider : ISigningKeyProvider
    {
        private readonly GovUkOidcConfiguration _configuration;
        private DateTime? _keyLastRetreived;
        private byte[]? _keyBytes;
        ILogger<KeyVaultSigningKeyProvider> _logger;

        public KeyVaultSigningKeyProvider(GovUkOidcConfiguration govUkOidcConfiguration, ILogger<KeyVaultSigningKeyProvider> logger)
        {
            _configuration = govUkOidcConfiguration;
            _keyLastRetreived = null;
            _keyBytes = null;
            _logger = logger;
        }

        public SecurityKey GetBearerTokenSigningKey()
        {
            var bytes = GetKeyBytes();

            _logger.LogInformation("KEYVAULT - before create SymmetricSecurityKey");
            var key = new SymmetricSecurityKey(bytes);
            _logger.LogInformation("KEYVAULT - after create SymmetricSecurityKey");

            return key;
        }

        private byte[] GetKeyBytes()
        {
            //if(KeyRequiresRefresh())
            //{
            _logger.LogInformation("KEYVAULT - Attempting to get key from Key Vault");
                var client = new KeyClient(new Uri(_configuration.Oidc.KeyVault.Url!), new DefaultAzureCredential());
            _logger.LogInformation("KEYVAULT - client created");

            var key = client.GetKey(_configuration.Oidc.KeyVault.Key);
            _logger.LogInformation("KEYVAULT - key obtained");

            _logger.LogInformation($"KEYVAULT - symmetryKey length {key.Value.Key.K.Length}");

            _keyBytes = key.Value.Key.K;
                _keyLastRetreived = DateTime.UtcNow;
            //}

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
