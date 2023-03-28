using System.Text.Json;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.SharedKernel;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;

public interface ITaxonomyService
{
    Task<List<KeyValuePair<TaxonomyDto, List<TaxonomyDto>>>> GetCategories();
}

public class TaxonomyService : ApiService, ITaxonomyService
{
    public TaxonomyService(HttpClient client)
    : base(client)
    {

    }

    public async Task<List<KeyValuePair<TaxonomyDto,List<TaxonomyDto>>>> GetCategories()
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(_client.BaseAddress + "api/taxonomies?taxonomyType=NotSet&pageNumber=1&pageSize=99999999"),
        };

        using var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var retVal = await JsonSerializer.DeserializeAsync<PaginatedList<TaxonomyDto>>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        List<KeyValuePair<TaxonomyDto, List<TaxonomyDto>>> keyValuePairs = new();

        if (retVal == null)
            return keyValuePairs;

        var topLevelCategories = retVal.Items.Where(x => x.ParentId == null && !x.Name.Contains("bccusergroupTestDelete")).ToList();

        foreach(var topLevelCategory in topLevelCategories)
        {
            var subCategories = retVal.Items.Where(x => x.ParentId == topLevelCategory.Id).ToList();
            var pair = new KeyValuePair<TaxonomyDto, List<TaxonomyDto>>(topLevelCategory, subCategories);
            keyValuePairs.Add(pair);
        }

        return keyValuePairs;
    }
}
