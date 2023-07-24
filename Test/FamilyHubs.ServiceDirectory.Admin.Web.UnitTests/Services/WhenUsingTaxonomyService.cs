using System.Collections.Generic;
using System.Threading.Tasks;
using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.ServiceDirectory.Shared.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests.Services;

public class WhenUsingTaxonomyService : BaseClientService
{
    private readonly Mock<ILogger<TaxonomyService>> _mockLogger;

    public WhenUsingTaxonomyService()
    {
        _mockLogger = new Mock<ILogger<TaxonomyService>>();
    }

    [Fact]
    public async Task ThenGetCategories()
    {
        //Arrange
        var taxonomies = new List<TaxonomyDto>
        {
            new TaxonomyDto{Id = 1, Name = "Activities, clubs and groups", TaxonomyType = TaxonomyType.ServiceCategory },
            new TaxonomyDto{Id = 2, Name = "Activities", TaxonomyType = TaxonomyType.ServiceCategory, ParentId = 1 }
        };

        var paginatedList = new PaginatedList<TaxonomyDto>(taxonomies, taxonomies.Count, 1, 1);

        var json = JsonConvert.SerializeObject(paginatedList);
        var mockClient = GetMockClient(json);
        var taxonomyService = new TaxonomyService(mockClient, _mockLogger.Object);

        //Act
        var result = await taxonomyService.GetCategories();

        //Assert
        result.Should().NotBeNull();
        result[0].Key.Should().BeEquivalentTo(taxonomies[0]);
        result[0].Value[0].Should().BeEquivalentTo(taxonomies[1]);

    }
}
