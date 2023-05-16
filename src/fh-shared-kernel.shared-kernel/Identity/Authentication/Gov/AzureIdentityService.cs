using Azure.Core;
using Azure.Identity;

namespace FamilyHubs.SharedKernel.Identity.Authentication.Gov
{
    public interface IAzureIdentityService
    {
        Task<string> AuthenticationCallback(string authority, string resource, string scope);
    }
    internal class AzureIdentityService : IAzureIdentityService
    {
        public async Task<string> AuthenticationCallback(string authority, string resource, string scope)
        {
            var chainedTokenCredential = new ChainedTokenCredential(
                new ManagedIdentityCredential(),
                new AzureCliCredential());

            var token = await chainedTokenCredential.GetTokenAsync(
                new TokenRequestContext(scopes: new[] { "https://vault.azure.net/.default" }));

            return token.Token;
        }
    }
}
