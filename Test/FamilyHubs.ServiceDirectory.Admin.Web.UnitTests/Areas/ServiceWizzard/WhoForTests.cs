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
using System.Threading.Tasks;
using Xunit;

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests.Areas.ServiceWizzard;

public class WhoForTests
{
    private readonly Mock<IRequestDistributedCache> _mockRequestDistributedCache;
    private readonly WhoForModel _whoFor;

    public WhoForTests()
    {
        _mockRequestDistributedCache = new Mock<IRequestDistributedCache>();
        DefaultHttpContext httpContext = new DefaultHttpContext();
        _whoFor = new WhoForModel(_mockRequestDistributedCache.Object);
        _whoFor.PageContext.HttpContext = httpContext;
        CompiledPageActionDescriptor compiledPageActionDescriptor = new();
        _whoFor.PageContext.ActionDescriptor = compiledPageActionDescriptor;
        _whoFor.PageContext.ActionDescriptor.DisplayName = "/WhoFor";
    }

    [Fact]
    public async Task ThenWhoForOnGetIsSuccessfull()
    {
        //Arrange
        _mockRequestDistributedCache.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(new Core.Models.OrganisationViewModel { ServiceName = "Service Name", MinAge = 2, MaxAge = 15 });
        int callback = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callback++);

        // Act
        await _whoFor.OnGet();

        // Assert
        callback.Should().Be(1);
        _whoFor.SelectedMinAge.Should().Be("2");
        _whoFor.SelectedMaxAge.Should().Be("15");
    }

    [Fact]
    public async Task ThenOnPostFails_WhenChildrenNotSelected()
    {
        //Act
        await _whoFor.OnPost();

        //Assert
#pragma warning disable CS8602
        _whoFor.ModelState.ElementAt(0).Value.ValidationState.Should().Be(Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid);
#pragma warning restore CS8602
        _whoFor.ValidationValid.Should().BeFalse();
        _whoFor.OneOptionSelected.Should().BeFalse();
    }

    [Fact]
    public async Task ThenOnPostFails_WhenAgeRangeNotSelected()
    {
        //Arrange
        _whoFor.Children = "Yes";
        //Act
        await _whoFor.OnPost();

        //Assert
#pragma warning disable CS8602
        _whoFor.ModelState.ElementAt(0).Value.ValidationState.Should().Be(Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid);
#pragma warning restore CS8602
        _whoFor.ValidationValid.Should().BeFalse();
        _whoFor.AgeRangeSelected.Should().BeFalse();
    }

    [Fact]
    public async Task ThenOnPostFails_WhenMinAgeGreaterThanMaxAge()
    {
        //Arrange
        _whoFor.Children = "Yes";
        _whoFor.SelectedMinAge = "10";
        _whoFor.SelectedMaxAge = "9";

        //Act
        await _whoFor.OnPost();

        //Assert
#pragma warning disable CS8602
        _whoFor.ModelState.ElementAt(0).Value.ValidationState.Should().Be(Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid);
#pragma warning restore CS8602
        _whoFor.ValidationValid.Should().BeFalse();
        _whoFor.ValidAgeRange.Should().BeFalse();
    }

    [Fact]
    public async Task ThenOnPostIsSuccessFull_AndReturnPageWhatLanguage()
    {
        //Arrange
        _whoFor.Children = "Yes";
        _whoFor.SelectedMinAge = "2";
        _whoFor.SelectedMaxAge = "9";
        _mockRequestDistributedCache.Setup(x => x.GetAsync(It.IsAny<string>()));
        int callback = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callback++);

        //Act
        var result = await _whoFor.OnPost() as RedirectToPageResult;

        //Assert
        callback.Should().Be(1);
        ArgumentNullException.ThrowIfNull(result);
        result.PageName.Should().Be("WhatLanguage");
    }

    [Fact]
    public async Task ThenOnPostIsSuccessFull_AndReturnPageWhatLanguageWhenWhoForContainsChildren()
    {
        //Arrange
        _whoFor.Children = "No";
        _whoFor.SelectedMinAge = "2";
        _whoFor.SelectedMaxAge = "9";
        _mockRequestDistributedCache.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(new Core.Models.OrganisationViewModel { ServiceName = "Service Name", MinAge = 2, MaxAge = 15, WhoForSelection = new List<string> { "Children" } });
        int callback = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callback++);

        //Act
        var result = await _whoFor.OnPost() as RedirectToPageResult;

        //Assert
        callback.Should().Be(1);
        ArgumentNullException.ThrowIfNull(result);
        result.PageName.Should().Be("WhatLanguage");
    }

    [Fact]
    public async Task ThenOnPostIsSuccessFull_AndReturnPageWhatLanguageWhenWhoForDoesNotContainsChildren()
    {
        //Arrange
        _whoFor.Children = "Yes";
        _whoFor.SelectedMinAge = "2";
        _whoFor.SelectedMaxAge = "9";
        _mockRequestDistributedCache.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(new Core.Models.OrganisationViewModel { ServiceName = "Service Name", MinAge = 2, MaxAge = 15, WhoForSelection = new List<string> { "Test" } });
        int callback = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callback++);

        //Act
        var result = await _whoFor.OnPost() as RedirectToPageResult;

        //Assert
        callback.Should().Be(1);
        ArgumentNullException.ThrowIfNull(result);
        result.PageName.Should().Be("WhatLanguage");
    }

    [Fact]
    public async Task ThenOnPostIsSuccessFull_AndReturnPageCheckServiceDetails()
    {
        //Arrange
        _whoFor.Children = "Yes";
        _whoFor.SelectedMinAge = "2";
        _whoFor.SelectedMaxAge = "9";
        _mockRequestDistributedCache.Setup(x => x.GetAsync(It.IsAny<string>()));
        int callback = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callback++);
        _mockRequestDistributedCache.Setup(x => x.GetLastPageAsync(It.IsAny<string>())).ReturnsAsync("/CheckServiceDetails");

        //Act
        var result = await _whoFor.OnPost() as RedirectToPageResult;

        //Assert
        callback.Should().Be(1);
        ArgumentNullException.ThrowIfNull(result);
        result.PageName.Should().Be("CheckServiceDetails");
    }

}
