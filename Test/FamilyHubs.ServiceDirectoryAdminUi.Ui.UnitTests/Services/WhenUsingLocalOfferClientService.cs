using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServices;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using FamilyHubs.SharedKernel;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.UnitTests.Services;

public class WhenUsingLocalOfferClientService : BaseClientService
{
    [Fact]
    public async Task ThenGetLocalOffers()
    {
        //Arrange
        List<OpenReferralServiceDto> list = new()
        {
            GetTestCountyCouncilServicesDto()
        };

        PaginatedList<OpenReferralServiceDto> paginatedList = new();
        paginatedList.Items.AddRange(list);
        var json = JsonConvert.SerializeObject(paginatedList);
        var mockClient = GetMockClient(json);
        LocalOfferClientService localOfferClientService = new(mockClient);

        //Act
        var result = await localOfferClientService.GetLocalOffers(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>());

        //Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(paginatedList);

    }

    [Fact]
    public async Task ThenGetLocalOfferById()
    {
        //Arrange
        var service = GetTestCountyCouncilServicesDto();
        var json = JsonConvert.SerializeObject(service);
        var mockClient = GetMockClient(json);
        LocalOfferClientService localOfferClientService = new(mockClient);

        //Act
        var result = await localOfferClientService.GetLocalOfferById(service.Id);

        //Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(service);

    }

    [Fact]
    public async Task GetServicesByOrganisationId()
    {
        //Arrange
        List<OpenReferralServiceDto> list = new()
        {
            GetTestCountyCouncilServicesDto()
        };

        var json = JsonConvert.SerializeObject(list);
        var mockClient = GetMockClient(json);
        LocalOfferClientService localOfferClientService = new(mockClient);

        //Act
        var result = await localOfferClientService.GetServicesByOrganisationId("56e62852-1b0b-40e5-ac97-54a67ea957dc");

        //Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(list);

    }
}
