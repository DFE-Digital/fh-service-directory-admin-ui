using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralOrganisations;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.UnitTests.Pages.OrganisationAdmin;

public class OrganisationAdminChooseOrganisation
{
    private ChooseOrganisationModel chooseOrganisationModel;
    private Mock<IOpenReferralOrganisationAdminClientService> mockIOpenReferralOrganisationAdminClientService;

    public OrganisationAdminChooseOrganisation()
    {
        mockIOpenReferralOrganisationAdminClientService = new Mock<IOpenReferralOrganisationAdminClientService>();
        var mockIRedisCacheService = new Mock<IRedisCacheService>();
        
        chooseOrganisationModel = new ChooseOrganisationModel(mockIOpenReferralOrganisationAdminClientService.Object, mockIRedisCacheService.Object);
    }

    [Fact]
    public async Task ChooseOrganisation_ValidationShouldFail_WhenNoOptionSelected()
    {
        //Arrange
        List<OpenReferralOrganisationDto> openReferralOrganisationDtos = new List<OpenReferralOrganisationDto>
        {
            new OpenReferralOrganisationDto()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Organisation 1"
            },
            new OpenReferralOrganisationDto()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Organisation 2"
            }
        };
        mockIOpenReferralOrganisationAdminClientService.Setup(x => x.GetListOpenReferralOrganisations()).ReturnsAsync(openReferralOrganisationDtos);
        chooseOrganisationModel.ModelState.AddModelError("SelectedOrganisation","Please select one option");

        // Act
        var result = await chooseOrganisationModel.OnPost() as RedirectToPageResult;

        // Assert
        chooseOrganisationModel.ValidationValid.Should().BeFalse();
    }

    [Fact]
    public async Task ChooseOrganisation_ValidationShouldFail_WhenIdCannotbeparsed()
    {
        //Arrange
        List<OpenReferralOrganisationDto> openReferralOrganisationDtos = new List<OpenReferralOrganisationDto>
        {
            new OpenReferralOrganisationDto()
            {
                Id = "Test1",
                Name = "Organisation 1"
            },
            new OpenReferralOrganisationDto()
            {
                Id = "Test2",
                Name = "Organisation 2"
            }
        };
        mockIOpenReferralOrganisationAdminClientService.Setup(x => x.GetListOpenReferralOrganisations()).ReturnsAsync(openReferralOrganisationDtos);
        chooseOrganisationModel.SelectedOrganisation = openReferralOrganisationDtos[0].Id;

        // Act
        var result = await chooseOrganisationModel.OnPost() as RedirectToPageResult;

        // Assert
        chooseOrganisationModel.ValidationValid.Should().BeFalse();
    }

    [Fact]
    public async Task ChooseOrganisation_ValidationShouldSucceed_WhenNoOptionSelected()
    {
        //Arrange
        List<OpenReferralOrganisationDto> openReferralOrganisationDtos = new List<OpenReferralOrganisationDto>
        {
            new OpenReferralOrganisationDto()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Organisation 1"
            },
            new OpenReferralOrganisationDto()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Organisation 2"
            }
        };
        mockIOpenReferralOrganisationAdminClientService.Setup(x => x.GetListOpenReferralOrganisations()).ReturnsAsync(openReferralOrganisationDtos);
        

        // Act
        var result = await chooseOrganisationModel.OnPost() as RedirectToPageResult;

        // Assert
        chooseOrganisationModel.ValidationValid.Should().BeFalse();
    }
}
