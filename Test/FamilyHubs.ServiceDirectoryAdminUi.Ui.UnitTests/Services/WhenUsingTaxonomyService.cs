using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralTaxonomys;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using FamilyHubs.SharedKernel;
using FluentAssertions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.UnitTests.Services;

public class WhenUsingTaxonomyService : BaseClientService
{
    [Fact]
    public async Task ThenGetCategories()
    {
        //Arrange
        var openReferralTaxonomies = new List<OpenReferralTaxonomyDto>()
        {
            new OpenReferralTaxonomyDto("16f3a451-e88d-4ad0-b53f-c8925d1cc9e4", "Activities, clubs and groups", "Activities, clubs and groups", null),
            new OpenReferralTaxonomyDto("aafa1cc3-b984-4b10-89d5-27388c5432de", "Activities", "Activities", "16f3a451-e88d-4ad0-b53f-c8925d1cc9e4"),
        };

        PaginatedList<OpenReferralTaxonomyDto> paginatedList = new PaginatedList<OpenReferralTaxonomyDto>(openReferralTaxonomies, openReferralTaxonomies.Count, 1, 1);

        var json = JsonConvert.SerializeObject(paginatedList);
        var mockClient = GetMockClient(json);
        TaxonomyService taxonomyService = new(mockClient);

        //Act
        var result = await taxonomyService.GetCategories();

        //Assert
        result.Should().NotBeNull();
        result[0].Key.Should().BeEquivalentTo(openReferralTaxonomies[0]);
        result[0].Value[0].Should().BeEquivalentTo(openReferralTaxonomies[1]);

    }
}
