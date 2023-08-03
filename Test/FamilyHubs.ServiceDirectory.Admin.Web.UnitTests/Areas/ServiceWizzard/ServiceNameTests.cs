using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Web.Areas.ServiceWizzard.Pages;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests.Areas.ServiceWizzard;

public class ServiceNameTests
{
    private readonly Mock<IRequestDistributedCache> _mockRequestDistributedCache;
    private readonly ServiceNameModel _serviceName;

    public ServiceNameTests()
    {
        _mockRequestDistributedCache = new Mock<IRequestDistributedCache>();
        DefaultHttpContext httpContext = new DefaultHttpContext();
        _serviceName = new ServiceNameModel(_mockRequestDistributedCache.Object);
        _serviceName.PageContext.HttpContext = httpContext;
        CompiledPageActionDescriptor compiledPageActionDescriptor = new();
        _serviceName.PageContext.ActionDescriptor = compiledPageActionDescriptor;
        _serviceName.PageContext.ActionDescriptor.DisplayName = "/ServiceName";
    }

    [Fact]
    public async Task ThenServiceNameOnGetIsSuccessfull()
    {
        //Arrange
        _mockRequestDistributedCache.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(new Core.Models.OrganisationViewModel { ServiceName = "Service Name" });
        int callbask = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callbask++);

        // Act
        await _serviceName.OnGet("1", "1", default!);

        // Assert
        callbask.Should().Be(1);
        _serviceName.ServiceName.Should().Be("Service Name");
    }

    [Fact]
    public async Task ThenNullServiceName()
    {
        // Act
        await _serviceName.OnPost();

        // Assert
        _serviceName.ValidationValid.Should().BeFalse();
    }

    [Fact]
    public async Task ThenEmptyServiceName()
    {
        // Arrange
        _serviceName.ServiceName = "";

        // Act
        await _serviceName.OnPost();

        // Assert
        _serviceName.ValidationValid.Should().BeFalse();
    }

    [Fact]
    public async Task ThenMoreThan255CharServiceName()
    {
        // Arrange
        _serviceName.ServiceName = "ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ" +
            "ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ" +
            "ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ";

        // Act
        await _serviceName.OnPost();

        // Assert
        _serviceName.ValidationValid.Should().BeFalse();
    }

    [Fact]
    public async Task ThenValidServiceName()
    {
        // Arrange
        _mockRequestDistributedCache.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(new Core.Models.OrganisationViewModel());
        int callbask = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callbask++);
        _serviceName.ServiceName = "ASDFGHJKLMNOPQRSTUVWXYZ";

        // Act
        var result = await _serviceName.OnPost() as RedirectToPageResult;

        // Assert
        _serviceName.ValidationValid.Should().BeTrue();
        ArgumentNullException.ThrowIfNull(result);
        result.PageName.Should().Be("TypeOfSupport");
        callbask.Should().Be(1);
    }

    [Fact]
    public async Task ThenValidServiceName_AndCreatedNewViewModel()
    {
        // Arrange
        _mockRequestDistributedCache.Setup(x => x.GetAsync(It.IsAny<string>()));
        int callbask = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callbask++);
        _serviceName.ServiceName = "ASDFGHJKLMNOPQRSTUVWXYZ";

        // Act
        var result = await _serviceName.OnPost() as RedirectToPageResult;

        // Assert
        _serviceName.ValidationValid.Should().BeTrue();
        ArgumentNullException.ThrowIfNull(result);
        result.PageName.Should().Be("TypeOfSupport");
        callbask.Should().Be(1);
    }

    [Fact]
    public async Task ThenValidServiceName_ReturnsToCheckServiceDetails()
    {
        // Arrange 
        _mockRequestDistributedCache.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(new Core.Models.OrganisationViewModel());
        int callbask = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callbask++);
        _mockRequestDistributedCache.Setup(x => x.GetLastPageAsync(It.IsAny<string>())).ReturnsAsync("/CheckServiceDetails");
        _serviceName.ServiceName = "ASDFGHJKLMNOPQRSTUVWXYZ";

        // Act
        var result = await _serviceName.OnPost() as RedirectToPageResult;

        // Assert
        _serviceName.ValidationValid.Should().BeTrue();
        ArgumentNullException.ThrowIfNull(result);
        result.PageName.Should().Be("CheckServiceDetails");
        callbask.Should().Be(1);
    }

}
