using System.Collections.Generic;
using System.Threading.Tasks;
using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests.Services;

public class WhenUsingLocalOfferClientService : BaseClientService
{
    private readonly Mock<ILogger<ServiceDirectoryClient>> _mockLogger;

    public WhenUsingLocalOfferClientService()
    {
        _mockLogger = new Mock<ILogger<ServiceDirectoryClient>>();
    }

    [Fact]
    public async Task ThenGetLocalOfferById()
    {
        //Arrange
        var service = GetTestCountyCouncilServicesDto(OrganisationId);
        var json = JsonConvert.SerializeObject(service);
        var mockClient = GetMockClient(json);
        var localOfferClientService = new ServiceDirectoryClient(mockClient, Mock.Of<ICacheService>(), _mockLogger.Object);

        //Act
        var result = await localOfferClientService.GetServiceById(service.Id);

        //Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(service);
    }

    //todo: reinstate
    //[Fact]
    //public async Task GetServicesByOrganisationId()
    //{
    //    //Arrange
    //    var list = new List<ServiceDto>
    //    {
    //        GetTestCountyCouncilServicesDto(OrganisationId)
    //    };

    //    var json = JsonConvert.SerializeObject(list);
    //    var mockClient = GetMockClient(json);
    //    var localOfferClientService = new ServiceDirectoryClient(mockClient, Mock.Of<ICacheService>(), _mockLogger.Object);

    //    //Act
    //    var result = await localOfferClientService.GetServicesByOrganisationId(123);

    //    //Assert
    //    result.Should().NotBeNull();
    //    result.Should().BeEquivalentTo(list);
    //}
}
