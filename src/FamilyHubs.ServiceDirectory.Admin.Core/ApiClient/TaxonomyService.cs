﻿using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Models;
using Microsoft.Extensions.Logging;

namespace FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;

public interface ITaxonomyService
{
    Task<List<KeyValuePair<TaxonomyDto, List<TaxonomyDto>>>> GetCategories(CancellationToken cancellationToken = default);
}

public class TaxonomyService : ApiService, ITaxonomyService
{
    private readonly ILogger<TaxonomyService> _logger;

    public TaxonomyService(HttpClient client, ILogger<TaxonomyService> logger)
    : base(client)
    {
        _logger = logger;
    }

    public async Task<List<KeyValuePair<TaxonomyDto, List<TaxonomyDto>>>> GetCategories(CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage();
        request.Method = HttpMethod.Get;
        request.RequestUri = new Uri(Client.BaseAddress + "api/taxonomies?taxonomyType=NotSet&pageNumber=1&pageSize=99999999");

        using var response = await Client.SendAsync(request, cancellationToken);

        response.EnsureSuccessStatusCode();

        var retVal = await DeserializeResponse<PaginatedList<TaxonomyDto>>(response, cancellationToken);

        var keyValuePairs = new List<KeyValuePair<TaxonomyDto, List<TaxonomyDto>>>();

        if (retVal == null)
        {
            _logger.LogInformation("No taxonomies found, returning empty list");
            return keyValuePairs;
        }

        var topLevelCategories = retVal.Items.Where(x => x.ParentId == null && !x.Name.Contains("bccusergroupTestDelete")).ToList();

        foreach (var topLevelCategory in topLevelCategories)
        {
            var subCategories = retVal.Items.Where(x => x.ParentId == topLevelCategory.Id).ToList();
            keyValuePairs.Add(new KeyValuePair<TaxonomyDto, List<TaxonomyDto>>(topLevelCategory, subCategories));
        }

        _logger.LogInformation("Returning {Count} taxonomies", keyValuePairs.Count);
        return keyValuePairs;
    }
}
