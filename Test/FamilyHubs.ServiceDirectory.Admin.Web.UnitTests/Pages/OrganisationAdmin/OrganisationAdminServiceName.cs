using System.Threading.Tasks;
using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.Areas.OrganisationAdmin.Pages;
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
        var mockServiceDirectoryClient = new Mock<IServiceDirectoryClient>();
        var mockICacheService = new Mock<ICacheService>();
        _serviceName = new ServiceNameModel(mockServiceDirectoryClient.Object, mockICacheService.Object);
    }


    [Fact]
    public async Task NullServiceName()
    {
        // Act
        _ = await _serviceName.OnPost() as RedirectToPageResult;

        // Assert
        _serviceName.ValidationValid.Should().BeFalse();
    }

    [Fact]
    public async Task EmptyServiceName()
    {
        // Arrange
        _serviceName.ServiceName = "";

        // Act
        _ = await _serviceName.OnPost() as RedirectToPageResult;

        // Assert
        _serviceName.ValidationValid.Should().BeFalse();
    }

    [Fact]
    public async Task MoreThan255CharServiceName()
    {
        // Arrange
        _serviceName.ServiceName = "ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ" +
                                   "ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ" +
                                   "ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ";

        // Act
        _ = await _serviceName.OnPost() as RedirectToPageResult;

        // Assert
        _serviceName.ValidationValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidServiceName()
    {
        // Arrange
        _serviceName.ServiceName = "ASDFGHJKLMNOPQRSTUVWXYZ";

        // Act
        _ = await _serviceName.OnPost() as ActionResult;

        // Assert
        _serviceName.ValidationValid.Should().BeTrue();
    }
}