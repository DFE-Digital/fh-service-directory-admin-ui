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
            new TaxonomyDto(
                        "UnitTest bccsource:Organisation",
                        "Organisation",
                        TaxonomyType.ServiceCategory,
                        null
                        ),
            new TaxonomyDto(
                        "UnitTest bccprimaryservicetype:38",
                        "Support",
                        TaxonomyType.ServiceCategory,
                        null
                        )
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
            new OrganisationDto(
                "56e62852-1b0b-40e5-ac97-54a67ea957dc",
                new OrganisationTypeDto("1", "LA", "Local Authority"),
                "Unit Test County Council",
                "Unit Test County Council",
                null,
                new Uri("https://www.unittest.gov.uk/").ToString(),
                "https://www.unittest.gov.uk/")
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
        var result = await organisationAdminClientService.GetOrganisationById("56e62852-1b0b-40e5-ac97-54a67ea957dc");

        //Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(organisation);
    }

    [Fact]
    public async Task ThenCreateOrganisation()
    {
        //Arrange
        var organisation = GetTestCountyCouncilDto();
        var mockClient = GetMockClient(organisation.Id);
        OrganisationAdminClientService organisationAdminClientService = new(mockClient);

        //Act
        var result = await organisationAdminClientService.CreateOrganisation(organisation);

        //Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(organisation.Id);
    }

    [Fact]
    public async Task ThenUpdatOrganisation()
    {
        //Arrange
        var organisation = GetTestCountyCouncilDto();
        var mockClient = GetMockClient(organisation.Id);
        OrganisationAdminClientService organisationAdminClientService = new(mockClient);

        //Act
        var result = await organisationAdminClientService.UpdateOrganisation(organisation);

        //Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(organisation.Id);
    }
}
