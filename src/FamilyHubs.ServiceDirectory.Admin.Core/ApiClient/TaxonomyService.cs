using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Models;
using Microsoft.Extensions.Logging;

namespace FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;

public interface ITaxonomyService
{
    Task<List<KeyValuePair<TaxonomyDto, List<TaxonomyDto>>>> GetCategories();
}

public class TaxonomyService : ApiService<TaxonomyService>, ITaxonomyService
{
    public TaxonomyService(HttpClient client, ILogger<TaxonomyService> logger)
    : base(client, logger)
    {

    }

    public async Task<List<KeyValuePair<TaxonomyDto, List<TaxonomyDto>>>> GetCategories()
    {
        var request = new HttpRequestMessage();
        request.Method = HttpMethod.Get;
        request.RequestUri = new Uri(Client.BaseAddress + "api/taxonomies?taxonomyType=NotSet&pageNumber=1&pageSize=99999999");

        using var response = await Client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var retVal = await DeserializeResponse<PaginatedList<TaxonomyDto>>(response);

        var keyValuePairs = new List<KeyValuePair<TaxonomyDto, List<TaxonomyDto>>>();

        if (retVal == null)
        {
            Logger.LogInformation($"{nameof(TaxonomyService)} No taxonomies found, returning empty list");
            return keyValuePairs;
        }


        var topLevelCategories = retVal.Items.Where(x => x.ParentId == null && !x.Name.Contains("bccusergroupTestDelete")).ToList();

        foreach (var topLevelCategory in topLevelCategories)
        {
            var subCategories = retVal.Items.Where(x => x.ParentId == topLevelCategory.Id).ToList();
            keyValuePairs.Add(new KeyValuePair<TaxonomyDto, List<TaxonomyDto>>(topLevelCategory, subCategories));
        }

        Logger.LogInformation($"{nameof(TaxonomyService)} Returning {keyValuePairs.Count} taxonomies");
        return keyValuePairs;
    }
}
