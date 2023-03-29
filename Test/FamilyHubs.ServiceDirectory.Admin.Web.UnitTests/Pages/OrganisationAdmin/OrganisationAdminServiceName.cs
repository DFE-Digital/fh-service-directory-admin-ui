using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.OrganisationAdmin;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

// ReSharper disable StringLiteralTypo

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests.Pages.OrganisationAdmin;

public class OrganisationAdminServiceName
{
    private readonly ServiceNameModel _serviceName;

    public OrganisationAdminServiceName()
    {
        var mockOrganisationAdminClientService = new Mock<IOrganisationAdminClientService>();
        var mockIRedisCacheService = new Mock<IRedisCacheService>();
        _serviceName = new ServiceNameModel(mockOrganisationAdminClientService.Object, mockIRedisCacheService.Object);
    }


    [Fact]
    public void NullServiceName()
    {
        // Act
        _ = _serviceName.OnPost() as RedirectToPageResult;

        // Assert
        _serviceName.ValidationValid.Should().BeFalse();
    }

    [Fact]
    public void EmptyServiceName()
    {
        // Arrange
        _serviceName.ServiceName = "";

        // Act
        _ = _serviceName.OnPost() as RedirectToPageResult;

        // Assert
        _serviceName.ValidationValid.Should().BeFalse();
    }

    [Fact]
    public void MoreThan255CharServiceName()
    {
        // Arrange
        _serviceName.ServiceName = "ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ" +
                                   "ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ" +
                                   "ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ";

        // Act
        _ = _serviceName.OnPost() as RedirectToPageResult;

        // Assert
        _serviceName.ValidationValid.Should().BeFalse();
    }

    [Fact]
    public void ValidServiceName()
    {
        // Arrange
        _serviceName.ServiceName = "ASDFGHJKLMNOPQRSTUVWXYZ";

        // Act
        _ = _serviceName.OnPost() as ActionResult;

        // Assert
        _serviceName.ValidationValid.Should().BeTrue();
    }
}