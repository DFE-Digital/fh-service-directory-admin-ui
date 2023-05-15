using System.Collections.Generic;
using System.Threading.Tasks;
using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.Areas.OrganisationAdmin.Pages;
using FluentAssertions;
using Moq;
using Xunit;

// ReSharper disable StringLiteralTypo

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests.Pages.OrganisationAdmin;

public class OrganisationAdminInPersonWhere
{
    private readonly InPersonWhereModel _inPersonWhereModel;

    public OrganisationAdminInPersonWhere()
    {
        var mockIPostcodeLocationClientService = new Mock<IPostcodeLocationClientService>();
        var mockICacheService = new Mock<ICacheService>();
        _inPersonWhereModel = new InPersonWhereModel(mockIPostcodeLocationClientService.Object, mockICacheService.Object);
        mockIPostcodeLocationClientService.Setup(s => s.LookupPostcode(It.IsAny<string>()))
            .ReturnsAsync(new PostcodesIoResponse { Result = new PostcodeInfo { Codes = new Codes() } });
    }

    [Fact]
    public async Task AddressEmpty()
    {
        // Arrange
        _inPersonWhereModel.Address1 = "";
        _inPersonWhereModel.City = "London";
        _inPersonWhereModel.PostalCode = "TW3 2DL";

        //Act
        await _inPersonWhereModel.OnPost();

        // Assert
        _inPersonWhereModel.Address1Valid.Should().BeFalse();
    }

    [Fact]
    public async Task CityEmpty()
    {
        // Arrange
        _inPersonWhereModel.Address1 = "ABCD";
        _inPersonWhereModel.City = "";
        _inPersonWhereModel.PostalCode = "TW3 2DL";

        // Act
        await _inPersonWhereModel.OnPost();

        // Assert
        _inPersonWhereModel.TownCityValid.Should().BeFalse();
    }

    [Fact]
    public async Task PostcodeEmpty()
    {
        // Arrange
        _inPersonWhereModel.Address1 = "ABCD";
        _inPersonWhereModel.City = "London";
        _inPersonWhereModel.PostalCode = "";

        //Act
        await _inPersonWhereModel.OnPost();

        // Assert
        _inPersonWhereModel.PostcodeValid.Should().BeFalse();
    }



    [Fact]
    public async Task AddressValid()
    {
        // Arrange
        _inPersonWhereModel.Address1 = "ABCD";
        _inPersonWhereModel.City = "London";
        _inPersonWhereModel.PostalCode = "TW3 2DL";
        _inPersonWhereModel.InPersonSelection = new List<string>();
        // Act
        await _inPersonWhereModel.OnPost();

        // Assert
        _inPersonWhereModel.PostcodeApiValid.Should().BeTrue();
    }
}