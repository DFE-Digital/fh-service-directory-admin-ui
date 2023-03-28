using System.Collections.Generic;
using System.Threading.Tasks;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using FamilyHubs.SharedKernel;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.UnitTests.Services;

public class WhenUsingTaxonomyService : BaseClientService
{
    [Fact]
    public async Task ThenGetCategories()
    {
        //Arrange
        var Taxonomies = new List<TaxonomyDto>
        {
            new TaxonomyDto{Id = 1, Name = "Activities, clubs and groups", TaxonomyType = TaxonomyType.ServiceCategory },
            new TaxonomyDto{Id = 2, Name = "Activities", TaxonomyType = TaxonomyType.ServiceCategory, ParentId = 1 },
        };

        PaginatedList<TaxonomyDto> paginatedList = new PaginatedList<TaxonomyDto>(Taxonomies, Taxonomies.Count, 1, 1);

        var json = JsonConvert.SerializeObject(paginatedList);
        var mockClient = GetMockClient(json);
        TaxonomyService taxonomyService = new(mockClient);

        //Act
        var result = await taxonomyService.GetCategories();

        //Assert
        result.Should().NotBeNull();
        result[0].Key.Should().BeEquivalentTo(Taxonomies[0]);
        result[0].Value[0].Should().BeEquivalentTo(Taxonomies[1]);

    }
}
