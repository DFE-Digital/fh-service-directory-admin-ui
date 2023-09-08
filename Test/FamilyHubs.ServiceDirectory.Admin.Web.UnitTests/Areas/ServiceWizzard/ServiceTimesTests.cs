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

public class ServiceTimesTests
{
    private readonly Mock<IRequestDistributedCache> _mockRequestDistributedCache;
    private readonly ServiceTimesModel _serviceTimes;

    public ServiceTimesTests()
    {
        _mockRequestDistributedCache = new Mock<IRequestDistributedCache>();
        DefaultHttpContext httpContext = new DefaultHttpContext();
        _serviceTimes = new ServiceTimesModel(_mockRequestDistributedCache.Object);
        _serviceTimes.PageContext.HttpContext = httpContext;
        CompiledPageActionDescriptor compiledPageActionDescriptor = new();
        _serviceTimes.PageContext.ActionDescriptor = compiledPageActionDescriptor;
        _serviceTimes.PageContext.ActionDescriptor.DisplayName = "/ServiceTimes";
    }

    [Theory]
    [InlineData(true,"Yes")]
    [InlineData(false, "No")]
    public async Task ThenServiceTimesOnGetIsSuccessfull(bool? hasSetDaysAndTimes, string expected)
    {
        //Arrange
        _mockRequestDistributedCache.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(new Core.Models.OrganisationViewModel { ServiceName = "Service Name", MinAge = 2, MaxAge = 15, Languages = new List<string>() { "English", "French" }, IsPayedFor = "Yes", PayUnit = "Hour", Cost = 2.50M, CostDetails = "Some Cost Details", HasSetDaysAndTimes = hasSetDaysAndTimes });
        int callback = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callback++);

        // Act
        await _serviceTimes.OnGet();

        // Assert
        callback.Should().Be(1);
        _serviceTimes.HastimesChoice.Should().Be(expected);
    }

    [Fact]
    public async Task ThenServiceTimesOnGetJustReturnIfNoViewModel()
    {
        //Arrange
        _mockRequestDistributedCache.Setup(x => x.GetAsync(It.IsAny<string>()));
        int callback = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callback++);

        // Act
        await _serviceTimes.OnGet();

        // Assert
        callback.Should().Be(1);
        _serviceTimes.HastimesChoice.Should().BeNull();
    }

    [Fact]
    public async Task ThenServiceTimesOnPost_ReturnsInValidWhenHastimesChoiceIsEmpty()
    {
        // Act
        await _serviceTimes.OnPost();

        // Assert
        _serviceTimes.ValidationValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("Yes", "DaysTakePlace")]
    [InlineData("No", "ServiceDeliveryType")]
    public async Task ThenServiceTimesOnPostIsSuccessfull(string hastimesChoice, string expected)
    {
        //Arrange
        _serviceTimes.HastimesChoice = hastimesChoice;
        int callback = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callback++);

        // Act
        var result = await _serviceTimes.OnPost() as RedirectToPageResult;
        

        // Assert
        callback.Should().Be(1);
        ArgumentNullException.ThrowIfNull(result);
        result.PageName.Should().Be(expected);
    }
}
