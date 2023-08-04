using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Web.Areas.ServiceWizzard.Pages;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests.Areas.ServiceWizzard;

public class WhenServiceTakesPlaceTests
{
    private readonly Mock<IRequestDistributedCache> _mockRequestDistributedCache;
    private readonly WhenServiceTakesPlaceModel _whenServiceTakesPlace;

    public WhenServiceTakesPlaceTests()
    {
        _mockRequestDistributedCache = new Mock<IRequestDistributedCache>();
        DefaultHttpContext httpContext = new DefaultHttpContext();
        _whenServiceTakesPlace = new WhenServiceTakesPlaceModel(_mockRequestDistributedCache.Object);
        _whenServiceTakesPlace.PageContext.HttpContext = httpContext;
        CompiledPageActionDescriptor compiledPageActionDescriptor = new();
        _whenServiceTakesPlace.PageContext.ActionDescriptor = compiledPageActionDescriptor;
        _whenServiceTakesPlace.PageContext.ActionDescriptor.DisplayName = "/WhenServiceTakesPlace";
    }

    [Theory]
    [InlineData(true, "Yes")]
    [InlineData(false, "No")]
    public async Task ThenWhenServiceTakesPlaceOnGetIsSuccessfull(bool isSameTimeOnEachDay, string expected)
    {
        //Arrange
        _mockRequestDistributedCache.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(new Core.Models.OrganisationViewModel { ServiceName = "Service Name", MinAge = 2, MaxAge = 15, Languages = new List<string>() { "English", "French" }, IsPayedFor = "Yes", PayUnit = "Hour", Cost = 2.50M, CostDetails = "Some Cost Details", HasSetDaysAndTimes = true, DaySelection = new List<string> { "Monday", "Tuesday" }, IsSameTimeOnEachDay = isSameTimeOnEachDay });
        int callbask = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callbask++);

        // Act
        await _whenServiceTakesPlace.OnGet();

        // Assert
        callbask.Should().Be(1);
        _whenServiceTakesPlace.RadioSelection.Should().Be(expected);
    }

    [Fact]
    public async Task ThenWhenServiceTakesPlaceOnGetJustReturnIfNoViewModel()
    {
        //Arrange
        _mockRequestDistributedCache.Setup(x => x.GetAsync(It.IsAny<string>()));
        int callbask = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callbask++);

        // Act
        await _whenServiceTakesPlace.OnGet();

        // Assert
        callbask.Should().Be(1);
        _whenServiceTakesPlace.RadioSelection.Should().BeNull();
    }

    [Fact]
    
    public async Task ThenWhenServiceTakesPlaceOnPostIsInValidWhenNoRadioButtonSelection()
    {
        //Arrange
        _mockRequestDistributedCache.Setup(x => x.GetAsync(It.IsAny<string>()));
        int callbask = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callbask++);

        // Act
        await _whenServiceTakesPlace.OnPost();

        // Assert
        callbask.Should().Be(1);
        _whenServiceTakesPlace.RadioSelection.Should().BeNull();
        _whenServiceTakesPlace.ValidationValid.Should().BeFalse();
        
    }

    [Theory]
    [InlineData(true, "Yes")]
    [InlineData(false, "No")]
    public async Task ThenWhenServiceTakesPlaceOnPostIsSuccessfull(bool isSameTimeOnEachDay, string expected)
    {
        //Arrange
        _whenServiceTakesPlace.RadioSelection = expected;
        _mockRequestDistributedCache.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(new Core.Models.OrganisationViewModel { ServiceName = "Service Name", MinAge = 2, MaxAge = 15, Languages = new List<string>() { "English", "French" }, IsPayedFor = "Yes", PayUnit = "Hour", Cost = 2.50M, CostDetails = "Some Cost Details", HasSetDaysAndTimes = true, DaySelection = new List<string> { "Monday", "Tuesday" }, IsSameTimeOnEachDay = isSameTimeOnEachDay });
        int callbask = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callbask++);

        // Act
        var result = await _whenServiceTakesPlace.OnPost() as RedirectToPageResult;

        // Assert
        callbask.Should().Be(1);
        _whenServiceTakesPlace.RadioSelection.Should().Be(expected);
        ArgumentNullException.ThrowIfNull(result);
        result.PageName.Should().Be("TimesEachDay");
    }
}
