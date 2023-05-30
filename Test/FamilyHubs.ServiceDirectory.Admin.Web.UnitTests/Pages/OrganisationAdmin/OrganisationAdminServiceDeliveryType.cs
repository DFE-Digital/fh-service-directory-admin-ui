using System.Collections.Generic;
using System.Threading.Tasks;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.Areas.OrganisationAdmin.Pages;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests.Pages.OrganisationAdmin;

public class OrganisationAdminServiceDeliveryType
{
    private readonly ServiceDeliveryTypeModel _serviceDeliveryTypeModel;

    public OrganisationAdminServiceDeliveryType()
    {
        var mockICacheService = new Mock<ICacheService>();
        _serviceDeliveryTypeModel = new ServiceDeliveryTypeModel(mockICacheService.Object)
        {
            ServiceDeliverySelection = new List<string>()
        };
    }

    [Fact]
    public async Task ValidationShouldFail_WhenNoOptionSelected()
    {
        // Act
        _ = await _serviceDeliveryTypeModel.OnPost() as RedirectToPageResult;

        // Assert
        _serviceDeliveryTypeModel.ValidationValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidationShouldNotFail_WhenAnOptionSelected()
    {
        //Arrange
        _serviceDeliveryTypeModel.ServiceDeliverySelection.Add("Online");

        // Act
        _ = await _serviceDeliveryTypeModel.OnPost() as RedirectToPageResult;

        // Assert
        _serviceDeliveryTypeModel.ValidationValid.Should().BeTrue();
    }
}