using System.Text.Json;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using Microsoft.Extensions.Logging;

namespace FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;

public interface IPostcodeLocationClientService
{
    Task<PostcodesIoResponse> LookupPostcode(string postcode);
}
public class PostcodeLocationClientService : ApiService<PostcodeLocationClientService>, IPostcodeLocationClientService
{
    private readonly Dictionary<string, PostcodesIoResponse> _postCodesCache = new Dictionary<string, PostcodesIoResponse>();

    public PostcodeLocationClientService(HttpClient client, ILogger<PostcodeLocationClientService> logger)
        : base(client,logger)
    {

    }

    public async Task<PostcodesIoResponse> LookupPostcode(string postcode)
    {
        var formattedPostCode = postcode.Replace(" ", "").ToLower();

        if (_postCodesCache.ContainsKey(formattedPostCode))
            return _postCodesCache[formattedPostCode];

        using var response = await Client.GetAsync($"/postcodes/{formattedPostCode}", HttpCompletionOption.ResponseHeadersRead);

        response.EnsureSuccessStatusCode();

        var postcodesIoResponse = await JsonSerializer.DeserializeAsync<PostcodesIoResponse>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        _postCodesCache.Add(formattedPostCode, postcodesIoResponse!);

        return postcodesIoResponse!;
    }
}