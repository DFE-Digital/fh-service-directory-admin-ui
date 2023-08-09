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

public class DaysTakePlaceTests
{
    private readonly Mock<IRequestDistributedCache> _mockRequestDistributedCache;
    private readonly DaysTakePlaceModel _daysTakePlace;

    public DaysTakePlaceTests()
    {
        _mockRequestDistributedCache = new Mock<IRequestDistributedCache>();
        DefaultHttpContext httpContext = new DefaultHttpContext();
        _daysTakePlace = new DaysTakePlaceModel(_mockRequestDistributedCache.Object);
        _daysTakePlace.PageContext.HttpContext = httpContext;
        CompiledPageActionDescriptor compiledPageActionDescriptor = new();
        _daysTakePlace.PageContext.ActionDescriptor = compiledPageActionDescriptor;
        _daysTakePlace.PageContext.ActionDescriptor.DisplayName = "/DaysTakePlace";
    }

    [Fact]
    public async Task ThenDaysTakePlaceOnGetIsSuccessfull()
    {
        //Arrange
        var daySelection = new List<string> { "Monday", "Tuesday" };
        _mockRequestDistributedCache.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(new Core.Models.OrganisationViewModel { ServiceName = "Service Name", MinAge = 2, MaxAge = 15, Languages = new List<string>() { "English", "French" }, IsPayedFor = "Yes", PayUnit = "Hour", Cost = 2.50M, CostDetails = "Some Cost Details", HasSetDaysAndTimes = true, DaySelection = daySelection });
        int callback = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callback++);

        // Act
        await _daysTakePlace.OnGet();

        // Assert
        callback.Should().Be(1);
        _daysTakePlace.DaySelection.Should().BeEquivalentTo(daySelection);
    }

    [Fact]
    public async Task ThenDaysTakePlaceOnGetJustReturnIfNoViewModel()
    {
        //Arrange
        _mockRequestDistributedCache.Setup(x => x.GetAsync(It.IsAny<string>()));
        int callback = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callback++);

        // Act
        await _daysTakePlace.OnGet();

        // Assert
        callback.Should().Be(1);
        _daysTakePlace.DaySelection.Should().BeNull();
    }

    [Fact]
    public async Task ThenServiceTimesOnPost_ReturnsInValidWhenDaysSelectedIsEmpty()
    {
        // Act
        await _daysTakePlace.OnPost();

        // Assert
        _daysTakePlace.ValidationValid.Should().BeFalse();
    }

    [Fact]
    public async Task ThenServiceTimesOnPostIsSuccessfull()
    {
        //Arrange
        _daysTakePlace.DaySelection = new List<string> { "Monday", "Tuesday" };
        int callback = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callback++);

        // Act
        var result = await _daysTakePlace.OnPost() as RedirectToPageResult;


        // Assert
        callback.Should().Be(1);
        ArgumentNullException.ThrowIfNull(result);
        result.PageName.Should().Be("WhenServiceTakesPlace");
    }
}
