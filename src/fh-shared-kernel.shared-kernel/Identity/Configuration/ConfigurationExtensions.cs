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
            var keyVaultConfigIsNull = string.IsNullOrWhiteSpace(configuration.Oidc.KeyVaultIdentifier);
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
    }
}
