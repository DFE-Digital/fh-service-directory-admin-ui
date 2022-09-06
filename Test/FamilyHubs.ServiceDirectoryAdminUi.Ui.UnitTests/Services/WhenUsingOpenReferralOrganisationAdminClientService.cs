using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralOrganisations;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralTaxonomys;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using FamilyHubs.SharedKernel;
using FluentAssertions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.UnitTests.Services;

public class WhenUsingOpenReferralOrganisationAdminClientService : BaseClientService
{
    [Fact]
    public async Task ThenGetTaxonomyList()
    {
        //Arrange
        List<OpenReferralTaxonomyDto> list = new()
        {
            new OpenReferralTaxonomyDto(
                        "UnitTest bccsource:Organisation",
                        "Organisation",
                        "Test BCC Data Sources",
                        null
                        ),
            new OpenReferralTaxonomyDto(
                        "UnitTest bccprimaryservicetype:38",
                        "Support",
                        "Test BCC Primary Services",
                        null
                        )
        };


        PaginatedList<OpenReferralTaxonomyDto> paginatedList = new();
        paginatedList.Items.AddRange(list);
        var json = JsonConvert.SerializeObject(paginatedList);
        var mockClient = GetMockClient(json);
        OpenReferralOrganisationAdminClientService openReferralOrganisationAdminClientService = new(mockClient);

        //Act
        var result = await openReferralOrganisationAdminClientService.GetTaxonomyList();

        //Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(paginatedList);
    }

    [Fact]
    public async Task ThenGetListOpenReferralOrganisations()
    {
        //Arrange
        List<OpenReferralOrganisationDto> list = new()
        {
            new OpenReferralOrganisationDto(
                "56e62852-1b0b-40e5-ac97-54a67ea957dc",
                "Unit Test County Council",
                "Unit Test County Council",
                null,
                new Uri("https://www.unittest.gov.uk/").ToString(),
                "https://www.unittest.gov.uk/")
        };
        var json = JsonConvert.SerializeObject(list);
        var mockClient = GetMockClient(json);
        OpenReferralOrganisationAdminClientService openReferralOrganisationAdminClientService = new(mockClient);

        //Act
        var result = await openReferralOrganisationAdminClientService.GetListOpenReferralOrganisations();

        //Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(list);
    }

    [Fact]
    public async Task ThenGetOpenReferralOrganisationById()
    {
        //Arrange
        var organisation = GetTestCountyCouncilDto();
        var json = JsonConvert.SerializeObject(organisation);
        var mockClient = GetMockClient(json);
        OpenReferralOrganisationAdminClientService openReferralOrganisationAdminClientService = new(mockClient);

        //Act
        var result = await openReferralOrganisationAdminClientService.GetOpenReferralOrganisationById("56e62852-1b0b-40e5-ac97-54a67ea957dc");

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
        OpenReferralOrganisationAdminClientService openReferralOrganisationAdminClientService = new(mockClient);

        //Act
        var result = await openReferralOrganisationAdminClientService.CreateOrganisation(organisation);

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
        OpenReferralOrganisationAdminClientService openReferralOrganisationAdminClientService = new(mockClient);

        //Act
        var result = await openReferralOrganisationAdminClientService.UpdateOrganisation(organisation);

        //Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(organisation.Id);
    }
}
