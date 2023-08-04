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

public class TimesEachDayTests
{
    private readonly Mock<IRequestDistributedCache> _mockRequestDistributedCache;
    private readonly TimesEachDayModel _timesEachDayModel;

    public TimesEachDayTests()
    {
        _mockRequestDistributedCache = new Mock<IRequestDistributedCache>();
        DefaultHttpContext httpContext = new DefaultHttpContext();
        _timesEachDayModel = new TimesEachDayModel(_mockRequestDistributedCache.Object);
        _timesEachDayModel.PageContext.HttpContext = httpContext;
        CompiledPageActionDescriptor compiledPageActionDescriptor = new();
        _timesEachDayModel.PageContext.ActionDescriptor = compiledPageActionDescriptor;
        _timesEachDayModel.PageContext.ActionDescriptor.DisplayName = "/TimesEachDay";
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ThenWhenTimesEachDayOnGetIsSuccessfull(bool isSameTimeOnEachDay)
    {
        //Arrange
        _mockRequestDistributedCache.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(new Core.Models.OrganisationViewModel { ServiceName = "Service Name", MinAge = 2, MaxAge = 15, Languages = new List<string>() { "English", "French" }, IsPayedFor = "Yes", PayUnit = "Hour", Cost = 2.50M, CostDetails = "Some Cost Details", HasSetDaysAndTimes = true, DaySelection = new List<string> { "Monday", "Tuesday" }, IsSameTimeOnEachDay = isSameTimeOnEachDay });
        int callbask = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callbask++);

        // Act
        await _timesEachDayModel.OnGet();

        // Assert
        callbask.Should().Be(1);
        _timesEachDayModel.IsSameTimeOnEachDay.Should().Be(isSameTimeOnEachDay);
    }

    [Fact]
    public async Task ThenWhenTimesEachDayOnGetWithExistingOpeningHoursIsSuccessfull()
    {
        //Arrange
        var expected = new List<Core.Models.OpeningHours> { new Core.Models.OpeningHours { Day = "Monday", Starts = "9", StartsTimeOfDay = "am", Finishes = "5", FinishesTimeOfDay = "pm" }, new Core.Models.OpeningHours { Day = "Tuesday", Starts = "9", StartsTimeOfDay = "am", Finishes = "4:30", FinishesTimeOfDay = "pm" } };
        _mockRequestDistributedCache.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(new Core.Models.OrganisationViewModel { ServiceName = "Service Name", MinAge = 2, MaxAge = 15, Languages = new List<string>() { "English", "French" }, IsPayedFor = "Yes", PayUnit = "Hour", Cost = 2.50M, CostDetails = "Some Cost Details", HasSetDaysAndTimes = true, DaySelection = new List<string> { "Monday", "Tuesday" }, IsSameTimeOnEachDay = true, OpeningHours = expected });
        int callbask = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callbask++);

        // Act
        await _timesEachDayModel.OnGet();

        // Assert
        callbask.Should().Be(1);
        _timesEachDayModel.IsSameTimeOnEachDay.Should().Be(true);
        _timesEachDayModel.OpeningHours.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task ThenWhenTimesEachDayOnGetJustReturnIfNoViewModel()
    {
        //Arrange
        _mockRequestDistributedCache.Setup(x => x.GetAsync(It.IsAny<string>()));
        int callbask = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callbask++);

        // Act
        await _timesEachDayModel.OnGet();

        // Assert
        callbask.Should().Be(1);
        _timesEachDayModel.OpeningHours.Should().BeNull();
    }

    [Fact]

    public async Task ThenWhenTimesEachDayOnPostIsInValidWhenNoRadioButtonSelection()
    {
        //Arrange
        _timesEachDayModel.OpeningHours = new List<Core.Models.OpeningHours> { new Core.Models.OpeningHours { Day = "Monday", Starts = default!, StartsTimeOfDay = "am", Finishes = "5", FinishesTimeOfDay = "pm" }, new Core.Models.OpeningHours { Day = "Tuesday", Starts = "9", StartsTimeOfDay = "am", Finishes = "4:30", FinishesTimeOfDay = "pm" } };
        _timesEachDayModel.IsSameTimeOnEachDay = true;
        _mockRequestDistributedCache.Setup(x => x.GetAsync(It.IsAny<string>()));
        int callbask = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callbask++);

        // Act
        await _timesEachDayModel.OnPost();

        // Assert
        callbask.Should().Be(1);
        _timesEachDayModel.ValidationValid.Should().BeFalse();

    }

    [Fact]

    public async Task ThenWhenTimesEachDayOnPostAddAnotherTime()
    {
        //Arrange
        var openingHours = new List<Core.Models.OpeningHours> { new Core.Models.OpeningHours { Day = "Monday", Starts = "9", StartsTimeOfDay = "am", Finishes = "5", FinishesTimeOfDay = "pm" }, new Core.Models.OpeningHours { Day = "Tuesday", Starts = "9", StartsTimeOfDay = "am", Finishes = "4:30", FinishesTimeOfDay = "pm" } };
        _mockRequestDistributedCache.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(new Core.Models.OrganisationViewModel { ServiceName = "Service Name", MinAge = 2, MaxAge = 15, Languages = new List<string>() { "English", "French" }, IsPayedFor = "Yes", PayUnit = "Hour", Cost = 2.50M, CostDetails = "Some Cost Details", HasSetDaysAndTimes = true, DaySelection = new List<string> { "Monday", "Tuesday" }, IsSameTimeOnEachDay = true, OpeningHours = openingHours });
        int callbask = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callbask++);

        // Act
        await _timesEachDayModel.OnPostAddAnotherTime("Wednesday");


        // Assert
        callbask.Should().Be(1);
        _timesEachDayModel.OpeningHours.Count.Should().Be(3);
    }

    [Fact]

    public async Task ThenWhenTimesEachDayOnPostRemoveTime()
    {
        //Arrange
        var openingHours = new List<Core.Models.OpeningHours> { new Core.Models.OpeningHours { Day = "Monday", Starts = "9", StartsTimeOfDay = "am", Finishes = "5", FinishesTimeOfDay = "pm" }, new Core.Models.OpeningHours { Day = "Tuesday", Starts = "9", StartsTimeOfDay = "am", Finishes = "4:30", FinishesTimeOfDay = "pm" } };
        _mockRequestDistributedCache.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(new Core.Models.OrganisationViewModel { ServiceName = "Service Name", MinAge = 2, MaxAge = 15, Languages = new List<string>() { "English", "French" }, IsPayedFor = "Yes", PayUnit = "Hour", Cost = 2.50M, CostDetails = "Some Cost Details", HasSetDaysAndTimes = true, DaySelection = new List<string> { "Monday", "Tuesday" }, IsSameTimeOnEachDay = true, OpeningHours = openingHours });
        int callbask = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callbask++);

        // Act
        await _timesEachDayModel.OnPostRemoveTime(1);


        // Assert
        callbask.Should().Be(1);
        _timesEachDayModel.OpeningHours.Count.Should().Be(1);
    }

    [Fact]

    public async Task ThenWhenTimesEachDayOnPostIsSuccessfull()
    {
        //Arrange
        _timesEachDayModel.OpeningHours = new List<Core.Models.OpeningHours> { new Core.Models.OpeningHours { Day = "Monday", Starts = "9", StartsTimeOfDay = "am", Finishes = "5", FinishesTimeOfDay = "pm" }, new Core.Models.OpeningHours { Day = "Tuesday", Starts = "9", StartsTimeOfDay = "am", Finishes = "4:30", FinishesTimeOfDay = "pm" } };
        _timesEachDayModel.IsSameTimeOnEachDay = true;
        _mockRequestDistributedCache.Setup(x => x.GetAsync(It.IsAny<string>()));
        int callbask = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callbask++);

        // Act
        var result = await _timesEachDayModel.OnPost() as RedirectToPageResult;

        // Assert
        callbask.Should().Be(1);
        ArgumentNullException.ThrowIfNull(result);
        result.PageName.Should().Be("ServiceDeliveryType");

    }

    [Fact]

    public async Task ThenWhenTimesEachDayOnPostIsSuccessfull_RetrunsToCheckServiceDetails()
    {
        //Arrange
        _timesEachDayModel.OpeningHours = new List<Core.Models.OpeningHours> { new Core.Models.OpeningHours { Day = "Monday", Starts = "9", StartsTimeOfDay = "am", Finishes = "5", FinishesTimeOfDay = "pm" }, new Core.Models.OpeningHours { Day = "Tuesday", Starts = "9", StartsTimeOfDay = "am", Finishes = "4:30", FinishesTimeOfDay = "pm" } };
        _timesEachDayModel.IsSameTimeOnEachDay = true;
        _mockRequestDistributedCache.Setup(x => x.GetAsync(It.IsAny<string>()));
        int callbask = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callbask++);
        _mockRequestDistributedCache.Setup(x => x.GetLastPageAsync(It.IsAny<string>())).ReturnsAsync("/CheckServiceDetails");

        // Act
        var result = await _timesEachDayModel.OnPost() as RedirectToPageResult;

        // Assert
        callbask.Should().Be(1);
        ArgumentNullException.ThrowIfNull(result);
        result.PageName.Should().Be("CheckServiceDetails");

    }
}
