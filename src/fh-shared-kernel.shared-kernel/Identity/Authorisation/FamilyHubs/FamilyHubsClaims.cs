using FamilyHubs.SharedKernel.Identity.Models;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Security.Claims;
using System.Text.Json;

namespace FamilyHubs.SharedKernel.Identity.Authorisation.FamilyHubs
{
    public class FamilyHubsClaims : ICustomClaims
    {
        private HttpClient _httpClient;
        public FamilyHubsClaims(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory?.CreateClient(nameof(FamilyHubsClaims))!;
        }

        public async Task<IEnumerable<Claim>> GetClaims(TokenValidatedContext tokenValidatedContext)
        {
            var openId = tokenValidatedContext?.Principal?.Identities.First().Claims
                .FirstOrDefault(c => c.Type.Equals(ClaimTypes.NameIdentifier))?.Value;
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(_httpClient.BaseAddress + $"api/Account/GetAccountClaimsByOpenId?openId={openId}"),
            };

            using var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var customClaims = JsonSerializer.Deserialize<List<AccountClaim>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });


            var claims = new List<Claim>();

            foreach (var claim in customClaims!)
            {
                claims.Add(new Claim(claim.Name, claim.Value));
            }
            claims.Add(new Claim (FamilyHubsClaimTypes.LoginTime, DateTime.UtcNow.Ticks.ToString() ));
            return claims.AsEnumerable();
        }
    }


}
