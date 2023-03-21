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
        private readonly Dictionary<string, PostcodesIoResponse> _postCodesCache = new Dictionary<string, PostcodesIoResponse>();

        public PostcodeLocationClientService(HttpClient client)
            : base(client)
        {

        }

        public async Task<PostcodesIoResponse> LookupPostcode(string postcode)
        {
            var formattedPostCode = postcode.Replace(" ","").ToLower();

            if (_postCodesCache.ContainsKey(formattedPostCode)) 
                return _postCodesCache[formattedPostCode];

            using var response = await _client.GetAsync($"/postcodes/{formattedPostCode}", HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();

            var postcodesIoResponse = await JsonSerializer.DeserializeAsync<PostcodesIoResponse>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            _postCodesCache.Add(formattedPostCode, postcodesIoResponse!);

            return postcodesIoResponse!;
        }
    }
}
