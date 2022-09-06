using FamilyHubs.ServiceDirectory.Shared.Models.Api;
using System.Text;
using System.Text.Json;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;

public interface IUICacheService
{
    Task<UICacheDto> GetUICacheById(string id);
    Task<string> CreateUICache(UICacheDto uiCacheDto);
    Task<string> UpdateUICache(UICacheDto uiCacheDto);
}

public class UICacheService : ApiService, IUICacheService
{
    public UICacheService(HttpClient client)
    : base(client)
    {

    }

    public async Task<UICacheDto> GetUICacheById(string id)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(_client.BaseAddress + $"api/uicaches/{id}"),

        };

        using var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var retVal = await JsonSerializer.DeserializeAsync<UICacheDto>(await response.Content.ReadAsStreamAsync(), options: new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        ArgumentNullException.ThrowIfNull(retVal, nameof(retVal));

        return retVal;
    }

    public async Task<string> CreateUICache(UICacheDto uiCacheDto)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(_client.BaseAddress + "api/uicaches"),
            Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(uiCacheDto), Encoding.UTF8, "application/json"),
        };

        using var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var stringResult = await response.Content.ReadAsStringAsync();
        return stringResult;
    }

    public async Task<string> UpdateUICache(UICacheDto uiCacheDto)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Put,
            RequestUri = new Uri(_client.BaseAddress + $"api/uicaches/{uiCacheDto.Id}"),
            Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(uiCacheDto), Encoding.UTF8, "application/json"),
        };

        using var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var stringResult = await response.Content.ReadAsStringAsync();
        return stringResult;
    }
}
