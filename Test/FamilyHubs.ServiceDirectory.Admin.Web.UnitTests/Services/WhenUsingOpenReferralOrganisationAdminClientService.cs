using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.ServiceDirectory.Shared.Models;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests.Services;

public class WhenUsingOrganisationAdminClientService : BaseClientService
{
    [Fact]
    public async Task ThenGetTaxonomyList()
    {
        //Arrange
        var list = new List<TaxonomyDto>
        {
            new TaxonomyDto
            {
                Id= 1,
                Name = "Organisation",
                TaxonomyType = TaxonomyType.ServiceCategory
            },
            new TaxonomyDto
            {
                Id= 2,
                Name = "Organisation",
                TaxonomyType = TaxonomyType.ServiceCategory
            }
        };


        var paginatedList = new PaginatedList<TaxonomyDto>();
        paginatedList.Items.AddRange(list);
        var json = JsonConvert.SerializeObject(paginatedList);
        var mockClient = GetMockClient(json);
        var serviceDirectoryClient = new ServiceDirectoryClient(mockClient, Mock.Of<ICacheService>());

        //Act
        var result = await serviceDirectoryClient.GetTaxonomyList();

        //Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(paginatedList);
    }

    [Fact]
    public async Task ThenGetListOrganisations()
    {
        //Arrange
        var list = new List<OrganisationDto>
        {
            new OrganisationDto
            {
                Id = OrganisationId,
                AdminAreaCode = "E1234",
                Description = "Unit Test County Council",
                Name = "Unit Test County Council",
                Uri = new Uri("https://www.unittest.gov.uk/").ToString(),
                Url = "https://www.unittest.gov.uk/",
                OrganisationType = OrganisationType.LA
            }
        };

        var json = JsonConvert.SerializeObject(list);
        var mockClient = GetMockClient(json);
        var organisationAdminClientService = new ServiceDirectoryClient(mockClient, Mock.Of<ICacheService>());

        //Act
        var result = await organisationAdminClientService.GetCachedLaOrganisations();

        //Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(list);
    }

    [Fact]
    public async Task ThenGetOrganisationById()
    {
        //Arrange
        var organisation = GetTestCountyCouncilDto();
        var json = JsonConvert.SerializeObject(organisation);
        var mockClient = GetMockClient(json);
        var organisationAdminClientService = new ServiceDirectoryClient(mockClient, Mock.Of<ICacheService>());

        //Act
        var result = await organisationAdminClientService.GetOrganisationById(OrganisationId);

        //Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(organisation);
    }

    [Fact]
    public async Task ThenCreateOrganisation()
    {
        //Arrange
        var organisation = GetTestCountyCouncilDto();
        var mockClient = GetMockClient(organisation.Id.ToString());
        var serviceDirectoryClient = new ServiceDirectoryClient(mockClient, Mock.Of<ICacheService>());

        //Act
        var result = await serviceDirectoryClient.CreateOrganisation(organisation);

        //Assert
        result.Should().BeGreaterThan(0);
        result.Should().Be(organisation.Id);
    }

    [Fact]
    public async Task ThenUpdateOrganisation()
    {
        //Arrange
        var organisation = GetTestCountyCouncilDto();
        var mockClient = GetMockClient(organisation.Id.ToString());
        var serviceDirectoryClient = new ServiceDirectoryClient(mockClient, Mock.Of<ICacheService>());

        //Act
        var result = await serviceDirectoryClient.UpdateOrganisation(organisation);

        //Assert
        result.Should().BeGreaterThan(0);
        result.Should().Be(organisation.Id);
    }
}
