using System.Text.Json;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralTaxonomys;
using FamilyHubs.SharedKernel;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;

public interface ITaxonomyService
{
    Task<List<KeyValuePair<OpenReferralTaxonomyDto, List<OpenReferralTaxonomyDto>>>> GetCategories();
}

public class TaxonomyService : ApiService, ITaxonomyService
{
    public TaxonomyService(HttpClient client)
    : base(client)
    {

    }

    public async Task<List<KeyValuePair<OpenReferralTaxonomyDto,List<OpenReferralTaxonomyDto>>>> GetCategories()
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(_client.BaseAddress + "api/taxonomies?pageNumber=1&pageSize=99999999"),
        };

        using var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var retVal = await JsonSerializer.DeserializeAsync<PaginatedList<OpenReferralTaxonomyDto>>(await response.Content.ReadAsStreamAsync(), options: new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        List<KeyValuePair<OpenReferralTaxonomyDto, List<OpenReferralTaxonomyDto>>> keyValuePairs = new();

        if (retVal == null)
            return keyValuePairs;

        var topLevelCategories = retVal.Items.Where(x => x.Parent == null && !x.Name.Contains("bccusergroupTestDelete")).ToList();

        foreach(var topLevelCategory in topLevelCategories)
        {
            var subCategories = retVal.Items.Where(x => x.Parent == topLevelCategory.Id).ToList();
            var pair = new KeyValuePair<OpenReferralTaxonomyDto, List<OpenReferralTaxonomyDto>>(topLevelCategory, subCategories);
            keyValuePairs.Add(pair);
        }

        return keyValuePairs;
    }
}
