using FamilyHubs.SharedKernel.GovLogin.Configuration;
using FamilyHubs.SharedKernel.Identity.Authorisation;
using FamilyHubs.SharedKernel.Identity.Models;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.KeyVaultExtensions;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;

namespace FamilyHubs.SharedKernel.Identity.Authentication.Gov
{
    public interface IOidcService
    {
        Task<Token?> GetToken(OpenIdConnectMessage openIdConnectMessage);
        Task PopulateAccountClaims(TokenValidatedContext tokenValidatedContext);
    }

    public class OidcService : IOidcService
    {
        private readonly HttpClient _httpClient;
        private readonly IAzureIdentityService _azureIdentityService;
        private readonly IJwtSecurityTokenService _jwtSecurityTokenService;
        private readonly ICustomClaims _customClaims;
        private readonly GovUkOidcConfiguration _configuration;
        private readonly ILogger<OidcService> _logger;

        public OidcService(
            HttpClient httpClient,
            IAzureIdentityService azureIdentityService,
            IJwtSecurityTokenService jwtSecurityTokenService,
            GovUkOidcConfiguration configuration,
            ICustomClaims customClaims,
            ILogger<OidcService> logger)
        {
            _httpClient = httpClient;
            _azureIdentityService = azureIdentityService;
            _jwtSecurityTokenService = jwtSecurityTokenService;
            _customClaims = customClaims;
            _configuration = configuration;
            _httpClient.BaseAddress = new Uri(_configuration.Oidc.BaseUrl);
            _logger = logger;
        }

        public async Task<Token?> GetToken(OpenIdConnectMessage openIdConnectMessage)
        {
            _logger.LogDebug("OidcService.GetToken:Entering");

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "/token")
            {
                Headers =
                {
                    Accept =
                    {
                        new MediaTypeWithQualityHeaderValue("*/*"),
                        new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded")
                    },
                    UserAgent = {new ProductInfoHeaderValue("DfE", "1")},
                }
            };

            httpRequestMessage.Content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", openIdConnectMessage?.Code ?? ""),
                new KeyValuePair<string, string>("redirect_uri", openIdConnectMessage?.RedirectUri ?? ""),
                new KeyValuePair<string, string>("client_assertion_type", "urn:ietf:params:oauth:client-assertion-type:jwt-bearer"),
                new KeyValuePair<string, string>("client_assertion", CreateJwtAssertion()),
            });

            httpRequestMessage.Content.Headers.Clear();
            httpRequestMessage.Content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");


            var response = await _httpClient.SendAsync(httpRequestMessage);

            if(response == null || !response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to retreive token from OneLogin StatusCode:{response?.StatusCode} Content:{response?.Content.ReadAsStringAsync()}");
            }

            var valueString = await response!.Content.ReadAsStringAsync();
            var content = JsonSerializer.Deserialize<Token>(valueString);

            _logger.LogDebug("OidcService.GetToken:Exiting");
            return content;
        }

        public async Task PopulateAccountClaims(TokenValidatedContext tokenValidatedContext)
        {
            _logger.LogDebug("OidcService.PopulateAccountClaims:Entering");
            if (tokenValidatedContext.TokenEndpointResponse == null || tokenValidatedContext.Principal == null)
            {
                _logger.LogWarning("OidcService.PopulateAccountClaims:Exiting due to null TokenEndpointResponse or Principal");
                return;
            }

            var accessToken = tokenValidatedContext.TokenEndpointResponse.Parameters["access_token"];

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "/userinfo")
            {
                Headers =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", accessToken)
                }
            };
            var response = await _httpClient.SendAsync(httpRequestMessage);
            var valueString = response.Content.ReadAsStringAsync().Result;
            var content = JsonSerializer.Deserialize<GovUkUser>(valueString);
            if (content?.Email != null)
            {
                tokenValidatedContext.Principal.Identities.First().AddClaim(new Claim(ClaimTypes.Email, content.Email));
            }

            tokenValidatedContext.Principal.Identities.First()
                .AddClaims(await _customClaims.GetClaims(tokenValidatedContext));

            _logger.LogDebug("OidcService.PopulateAccountClaims:Exiting");
        }

        private string CreateJwtAssertion()
        {
            var jti = Guid.NewGuid().ToString();
            var claimsIdentity = new ClaimsIdentity(
                new List<Claim>
                {
                    new Claim("sub", _configuration.Oidc.ClientId),
                    new Claim("jti", jti)

                });

            var signingCredentials = GetSigningCredentials();

            var value = _jwtSecurityTokenService.CreateToken(
                _configuration.Oidc.ClientId,
                $"{_configuration.Oidc.BaseUrl}/token",
                claimsIdentity,
                signingCredentials);

            return value;
        }

        private SigningCredentials GetSigningCredentials()
        {
            if (_configuration.UseKeyVault())
            {
                _logger.LogDebug("OidcService retrieving privateKey from KeyVault");
                return new SigningCredentials(
                    new KeyVaultSecurityKey(_configuration.KeyVaultIdentifier(),
                        _azureIdentityService.AuthenticationCallback), "RS512")
                {
                    CryptoProviderFactory = new CryptoProviderFactory
                    {
                        CustomCryptoProvider = new KeyVaultCryptoProvider()
                    }
                };
            }

            _logger.LogDebug("OidcService retrieving privateKey from LocalConfig");
            var unencodedKey = _configuration.Oidc.PrivateKey!;
            var privateKeyBytes = Convert.FromBase64String(unencodedKey);

            var rsa = RSA.Create();
            try
            {
                rsa.ImportPkcs8PrivateKey(privateKeyBytes, out _);
            }
            catch (Exception ex)
            {
                var foo = ex.Message;
            }
            var key = new RsaSecurityKey(rsa);
            return new SigningCredentials(key, "RS256");
        }

    }
}
