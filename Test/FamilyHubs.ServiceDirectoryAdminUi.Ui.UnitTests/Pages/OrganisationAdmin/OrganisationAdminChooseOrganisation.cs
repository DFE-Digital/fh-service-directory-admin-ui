using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralOrganisations;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.UnitTests.Pages.OrganisationAdmin;

public class OrganisationAdminChooseOrganisation
{
    [Fact]
    public async Task SelectedOrganisationEmpty()
    {
        // Arrange
        List<OpenReferralOrganisationDto> openReferralOrganisationDtos = new()
        {
            new OpenReferralOrganisationDto
            {
                Id = Guid.NewGuid().ToString(),
                Name= "Test 1",
            },
            new OpenReferralOrganisationDto
            {
                Id = Guid.NewGuid().ToString(),
                Name= "Test 2",
            }
        };

        var mockIOpenReferralOrganisationAdminClientService = new Mock<IOpenReferralOrganisationAdminClientService>();
        mockIOpenReferralOrganisationAdminClientService.Setup(x => x.GetListOpenReferralOrganisations()).ReturnsAsync(openReferralOrganisationDtos);
        var mockIRedisCacheService = new Mock<IRedisCacheService>();
        

        var sut = new FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin.ChooseOrganisationModel(mockIOpenReferralOrganisationAdminClientService.Object, mockIRedisCacheService.Object);

        // Act
        var result = await sut.OnPost() as RedirectToPageResult;


        // Assert
        sut.ValidationValid.Should().BeFalse();
    }

    [Fact]
    public async Task SelectedOrganisationIsValid()
    {
        // Arrange
        List<OpenReferralOrganisationDto> openReferralOrganisationDtos = new()
        {
            new OpenReferralOrganisationDto
            {
                Id = Guid.NewGuid().ToString(),
                Name= "Test 1",
            },
            new OpenReferralOrganisationDto
            {
                Id = Guid.NewGuid().ToString(),
                Name= "Test 2",
            }
        };

        var mockIOpenReferralOrganisationAdminClientService = new Mock<IOpenReferralOrganisationAdminClientService>();
        mockIOpenReferralOrganisationAdminClientService.Setup(x => x.GetListOpenReferralOrganisations()).ReturnsAsync(openReferralOrganisationDtos);
        var mockIRedisCacheService = new Mock<IRedisCacheService>();


        var sut = new FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin.ChooseOrganisationModel(mockIOpenReferralOrganisationAdminClientService.Object, mockIRedisCacheService.Object);
        sut.SelectedOrganisation = openReferralOrganisationDtos[0].Id.ToString();

        // Act
        var result = await sut.OnPost() as RedirectToPageResult;


        // Assert
        sut.ValidationValid.Should().BeTrue();
    }
}
