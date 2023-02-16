using System.Text.Json;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api
{
    public interface IPostcodeLocationClientService
    {
        Task<PostcodesIoResponse> LookupPostcode(string postcode);
    }
    public class PostcodeLocationClientService : ApiService, IPostcodeLocationClientService
    {
        public PostcodeLocationClientService(HttpClient client)
            : base(client)
        {

        }

        public async Task<PostcodesIoResponse> LookupPostcode(string postcode)
        {
            using var response = await _client.GetAsync($"/postcodes/{postcode}", HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();

#pragma warning disable CS8603 // Possible null reference return.
            return await JsonSerializer.DeserializeAsync<PostcodesIoResponse>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
#pragma warning restore CS8603 // Possible null reference return.

        }
    }
}
