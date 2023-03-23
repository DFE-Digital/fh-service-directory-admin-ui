using System;
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

public class WhenUsingOrganisationAdminClientService : BaseClientService
{
    [Fact]
    public async Task ThenGetTaxonomyList()
    {
        //Arrange
        List<TaxonomyDto> list = new()
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


        PaginatedList<TaxonomyDto> paginatedList = new();
        paginatedList.Items.AddRange(list);
        var json = JsonConvert.SerializeObject(paginatedList);
        var mockClient = GetMockClient(json);
        OrganisationAdminClientService organisationAdminClientService = new(mockClient);

        //Act
        var result = await organisationAdminClientService.GetTaxonomyList();

        //Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(paginatedList);
    }

    [Fact]
    public async Task ThenGetListOrganisations()
    {
        //Arrange
        List<OrganisationDto> list = new()
        {
            new OrganisationDto
            {
                Id = ORGANISATION_ID,
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
        OrganisationAdminClientService organisationAdminClientService = new(mockClient);

        //Act
        var result = await organisationAdminClientService.GetListOrganisations();

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
        OrganisationAdminClientService organisationAdminClientService = new(mockClient);

        //Act
        var result = await organisationAdminClientService.GetOrganisationById(ORGANISATION_ID);

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
        OrganisationAdminClientService organisationAdminClientService = new(mockClient);

        //Act
        var result = await organisationAdminClientService.CreateOrganisation(organisation);

        //Assert
        result.Should().BeGreaterThan(0);
        result.Should().Be(organisation.Id);
    }

    [Fact]
    public async Task ThenUpdatOrganisation()
    {
        //Arrange
        var organisation = GetTestCountyCouncilDto();
        var mockClient = GetMockClient(organisation.Id.ToString());
        OrganisationAdminClientService organisationAdminClientService = new(mockClient);

        //Act
        var result = await organisationAdminClientService.UpdateOrganisation(organisation);

        //Assert
        result.Should().BeGreaterThan(0);
        result.Should().Be(organisation.Id);
    }
}
