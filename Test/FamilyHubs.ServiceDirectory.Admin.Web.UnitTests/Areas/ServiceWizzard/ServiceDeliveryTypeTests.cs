using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Web.Areas.ServiceWizzard.Pages;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests.Areas.ServiceWizzard;

public class ServiceDeliveryTypeTests
{
    private readonly Mock<IRequestDistributedCache> _mockRequestDistributedCache;
    private readonly ServiceDeliveryTypeModel _serviceDeliveryType;

    public ServiceDeliveryTypeTests()
    {
        _mockRequestDistributedCache = new Mock<IRequestDistributedCache>();
        DefaultHttpContext httpContext = new DefaultHttpContext();
        _serviceDeliveryType = new ServiceDeliveryTypeModel(_mockRequestDistributedCache.Object);
        _serviceDeliveryType.PageContext.HttpContext = httpContext;
        CompiledPageActionDescriptor compiledPageActionDescriptor = new();
        _serviceDeliveryType.PageContext.ActionDescriptor = compiledPageActionDescriptor;
        _serviceDeliveryType.PageContext.ActionDescriptor.DisplayName = "/ServiceDeliveryType";
    }

    [Fact]
    public async Task ThenServiceDeliveryTypeOnGetIsSuccessfull()
    {
        //Arrange
        var expected = new List<string> { "1" };
        _mockRequestDistributedCache.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(new Core.Models.OrganisationViewModel { ServiceName = "Service Name", MinAge = 2, MaxAge = 15, Languages = new List<string>() { "English", "French" }, IsPayedFor = "Yes", PayUnit = "Hour", Cost = 2.50M, CostDetails = "Some Cost Details", HasSetDaysAndTimes = true, DaySelection = new List<string> { "Monday", "Tuesday" }, IsSameTimeOnEachDay = true, ServiceDeliverySelection = expected });
        int callbask = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callbask++);

        // Act
        await _serviceDeliveryType.OnGet();

        // Assert
        callbask.Should().Be(1);
        _serviceDeliveryType.ServiceDeliverySelection.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task ThenServiceDeliveryTypeOnPost_IsInvalid()
    {
        // Arrange
        _serviceDeliveryType.ServiceDeliverySelection = new List<string>();

        //Act
        await _serviceDeliveryType.OnPost();

        //Assert
        _serviceDeliveryType.ValidationValid.Should().BeFalse();
    }

    [Fact]
    public async Task ThenServiceDeliveryTypeOnPostIsSuccessfull_AndRedirectsToInPersonWhere()
    {
        // Arrange
        int callbask = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
           .Callback(() => callbask++);
        _serviceDeliveryType.ServiceDeliverySelection = new List<string> { "1" };

        //Act
        var result = await _serviceDeliveryType.OnPost() as RedirectToPageResult;

        //Assert
        callbask.Should().Be(1);
        _serviceDeliveryType.ValidationValid.Should().BeTrue();
        ArgumentNullException.ThrowIfNull(result);
        result.PageName.Should().Be("InPersonWhere");
    }

    [Fact]
    public async Task ThenServiceDeliveryTypeOnPostIsSuccessfull_AndRedirectsToWhoFor()
    {
        // Arrange
        int callbask = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
           .Callback(() => callbask++);
        _serviceDeliveryType.ServiceDeliverySelection = new List<string> { "2" };

        //Act
        var result = await _serviceDeliveryType.OnPost() as RedirectToPageResult;

        //Assert
        callbask.Should().Be(1);
        _serviceDeliveryType.ValidationValid.Should().BeTrue();
        ArgumentNullException.ThrowIfNull(result);
        result.PageName.Should().Be("WhoFor");
    }
}
