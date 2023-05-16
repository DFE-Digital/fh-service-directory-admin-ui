using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.SharedKernel.Identity.Exceptions;
using FamilyHubs.SharedKernel.Identity.Models;
using Microsoft.Extensions.Configuration;

namespace FamilyHubs.SharedKernel.GovLogin.Configuration
{
    public static class ConfigurationExtensions
    {
        public static GovUkOidcConfiguration GetGovUkOidcConfiguration(this IConfiguration configuration)
        {
            var config = configuration.GetSection(nameof(GovUkOidcConfiguration)).Get<GovUkOidcConfiguration>();
            if (config == null)
            {
                throw new AuthConfigurationException("Could not get Section GovUkOidcConfiguration from configuration");
            }
            return config;
        }

        public static bool UseKeyVault(this GovUkOidcConfiguration configuration)
        {
            var keyVaultConfigIsNull = !ValidKeyVaultConfiguration(configuration.Oidc.KeyVault);
            var privateKeyConfigIsNull = string.IsNullOrWhiteSpace(configuration.Oidc.PrivateKey);

            if (keyVaultConfigIsNull && privateKeyConfigIsNull)
            {
                throw new AuthConfigurationException("Either KeyVaultIdentifier or PrivateKey must be populated, both cannot be null");
            }

            if (!keyVaultConfigIsNull && !privateKeyConfigIsNull)
            {
                throw new AuthConfigurationException("Both KeyVaultIdentifier or PrivateKey must not be populated, only one should be used");
            }

            if (!keyVaultConfigIsNull)
            {
                return true;
            }

            return false;
        }

        public static string KeyVaultIdentifier(this GovUkOidcConfiguration configuration)
        {
            if (!ValidKeyVaultConfiguration(configuration.Oidc.KeyVault))
            {
                throw new AuthConfigurationException("Keyvault configuration invalid, requires - Url, Key and VersionId");
            }

            return $"{configuration.Oidc.KeyVault.Url}/keys/{configuration.Oidc.KeyVault.Key}/{configuration.Oidc.KeyVault.VersionId}";
        }

        public static List<StubUser> GetStubUsers(this GovUkOidcConfiguration configuration)
        {
            if(configuration.StubAuthentication.StubUsers != null)
            {
                return configuration.StubAuthentication.StubUsers;
            }

            var stubUsers = new List<StubUser>();
            stubUsers.Add(CreateStubUser("admin", "Admin"));
            stubUsers.Add(CreateStubUser("general", "Basic"));

            return stubUsers;
        }

        private static StubUser CreateStubUser(string firstName, string role)
        {
            var email = $"{firstName}.user@stub.com";
            return new StubUser
            {
                User = new GovUkUser
                {
                    Email = email,
                    Sub = email
                },
                Claims = new List<AccountClaim>
                {
                    new AccountClaim { Name = FamilyHubsClaimTypes.Role, Value = role },
                    new AccountClaim { Name = FamilyHubsClaimTypes.FirstName, Value = firstName },
                    new AccountClaim { Name = FamilyHubsClaimTypes.LastName, Value = "User" },
                    new AccountClaim { Name = FamilyHubsClaimTypes.OrganisationId, Value = "1" },
                    new AccountClaim { Name = FamilyHubsClaimTypes.AccountStatus, Value = AccountStatus.Active.ToString() }
                }
            };
        }

        private static bool ValidKeyVaultConfiguration(KeyVaultConfiguration configuration)
        {
            if (string.IsNullOrWhiteSpace(configuration.Url))
                return false;

            if (string.IsNullOrWhiteSpace(configuration.Key))
                return false;

            if (string.IsNullOrWhiteSpace(configuration.VersionId))
                return false;

            return true;
        }
    }
}
