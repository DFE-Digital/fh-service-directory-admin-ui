using SFA.DAS.HashingService;
using System.Text.Json;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api
{
    public interface IPostcodeLocationClientService
    {
        Task<PostcodeApiModel> LookupPostcode(string postcode);
    }
    public class PostcodeLocationClientService : ApiService, IPostcodeLocationClientService
    {
        public PostcodeLocationClientService(HttpClient client, IHashingService hashingService)
            : base(client, hashingService)
        {
            client.BaseAddress = new Uri("http://api.postcodes.io");
        }

        public async Task<PostcodeApiModel> LookupPostcode(string postcode)
        {
            using var response = await _client.GetAsync($"/postcodes/{postcode}", HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();

#pragma warning disable CS8603 // Possible null reference return.
            return await JsonSerializer.DeserializeAsync<PostcodeApiModel>(await response.Content.ReadAsStreamAsync(), options: new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
#pragma warning restore CS8603 // Possible null reference return.

        }
    }
}
