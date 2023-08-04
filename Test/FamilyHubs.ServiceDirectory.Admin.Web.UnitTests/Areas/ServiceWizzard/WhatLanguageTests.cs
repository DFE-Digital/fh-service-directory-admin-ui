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

public class WhatLanguageTests
{
    private readonly Mock<IRequestDistributedCache> _mockRequestDistributedCache;
    private readonly WhatLanguageModel _whatLanguage;

    public WhatLanguageTests()
    {
        _mockRequestDistributedCache = new Mock<IRequestDistributedCache>();
        DefaultHttpContext httpContext = new DefaultHttpContext();
        _whatLanguage = new WhatLanguageModel(_mockRequestDistributedCache.Object);
        _whatLanguage.PageContext.HttpContext = httpContext;
        CompiledPageActionDescriptor compiledPageActionDescriptor = new();
        _whatLanguage.PageContext.ActionDescriptor = compiledPageActionDescriptor;
        _whatLanguage.PageContext.ActionDescriptor.DisplayName = "/WhoFor";
    }

    [Fact]
    public async Task ThenWhatLanguageOnGetIsSuccessfull()
    {
        //Arrange
        List<string> languages = new List<string>() { "English", "French" };
        _mockRequestDistributedCache.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(new Core.Models.OrganisationViewModel { ServiceName = "Service Name", MinAge = 2, MaxAge = 15, Languages = languages });
        int callbask = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callbask++);

        // Act
        await _whatLanguage.OnGet();

        // Assert
        callbask.Should().Be(1);
        _whatLanguage.LanguageCode.Should().BeEquivalentTo(languages);
        _whatLanguage.LanguageNumber.Should().Be(2);
       
    }

    [Fact]
    public async Task ThenWhoForOnGetIsSuccessfullWithNoViewModelSaved()
    {
        //Arrange
        _mockRequestDistributedCache.Setup(x => x.GetAsync(It.IsAny<string>()));
        int callbask = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callbask++);

        // Act
        await _whatLanguage.OnGet();

        // Assert
        callbask.Should().Be(1);
        _whatLanguage.LanguageNumber.Should().Be(1);

    }

    [Fact]
    public void ThenOnPostAddAnotherLanguage()
    {
        // Arrangle
        _whatLanguage.LanguageCode = new List<string> { "English" };

        //Act
        _whatLanguage.OnPostAddAnotherLanguage();

        //Assert
        _whatLanguage.LanguageNumber.Should().Be(2);
    }

    [Fact]
    public void ThenOnPostRemoveLanguage()
    {
        // Arrange
        _whatLanguage.LanguageCode = new List<string> { "English", "French" };
        _whatLanguage.LanguageNumber = _whatLanguage.LanguageCode.Count;

        //Act
        _whatLanguage.OnPostRemoveLanguage(1);

        //Assert
        _whatLanguage.LanguageNumber.Should().Be(1);
        _whatLanguage.LanguageCode.Contains("English").Should().BeTrue();
        _whatLanguage.LanguageCode.Contains("French").Should().BeFalse();
    }

    [Fact]
    public async Task ThenOnPostNextPage_WithNoLanguagesSelected()
    {
        //Act
        await _whatLanguage.OnPostNextPage();

        //Assert
        _whatLanguage.ValidationValid.Should().BeFalse();
    }

    [Fact]
    public async Task ThenOnPostNextPage_WithModelStateError()
    {
        // Arrange
        _whatLanguage.LanguageCode = new List<string> { "English", "French" };
        _whatLanguage.LanguageNumber = _whatLanguage.LanguageCode.Count;
        _whatLanguage.ModelState.AddModelError("key", "Some model error");

        //Act
        await _whatLanguage.OnPostNextPage();

        //Assert
        _whatLanguage.ValidationValid.Should().BeFalse();
    }

    [Fact]
    public async Task ThenOnPostNextPage_ReturnInValidForNullItemInLanguageList()
    {
        // Arrange
        List<string> languages = new List<string>() { "English", default!, "French" };
        _whatLanguage.LanguageCode = languages;
        _whatLanguage.LanguageNumber = _whatLanguage.LanguageCode.Count;
        int callbask = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callbask++);


        //Act
        await _whatLanguage.OnPostNextPage();

        //Assert
        _whatLanguage.ValidationValid.Should().BeFalse();
        callbask.Should().Be(1);
    }

    [Fact]
    public async Task ThenOnPostNextPage_ReturnInValidForDuplicateLanguages()
    {
        // Arrange
        List<string> languages = new List<string>() { "English", "French", "French" };
        _whatLanguage.LanguageCode = languages;
        _whatLanguage.LanguageNumber = _whatLanguage.LanguageCode.Count;
        int callbask = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callbask++);


        //Act
        await _whatLanguage.OnPostNextPage();

        //Assert
        _whatLanguage.ValidationValid.Should().BeFalse();
        callbask.Should().Be(1);
    }

    [Fact]
    public async Task ThenOnPostNextPage_ReturnsNextPage()
    {
        // Arrange
        List<string> languages = new List<string>() { "English", "French" };
        _whatLanguage.LanguageCode = languages;
        _whatLanguage.LanguageNumber = _whatLanguage.LanguageCode.Count;
        int callbask = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callbask++);


        //Act
        var result = await _whatLanguage.OnPostNextPage() as RedirectToPageResult;

        //Assert
        _whatLanguage.ValidationValid.Should().BeTrue();
        callbask.Should().Be(1);
        _whatLanguage.LanguageCode.Should().BeEquivalentTo(languages);
        _whatLanguage.LanguageNumber.Should().Be(languages.Count);
        ArgumentNullException.ThrowIfNull(result);
        result.PageName.Should().Be("PayForService");
    }

    [Fact]
    public async Task ThenOnPostNextPage_ReturnsCheckServiceDetailsPage()
    {
        // Arrange
        List<string> languages = new List<string>() { "English", "French" };
        _whatLanguage.LanguageCode = languages;
        _whatLanguage.LanguageNumber = _whatLanguage.LanguageCode.Count;
        int callbask = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callbask++);
        _mockRequestDistributedCache.Setup(x => x.GetLastPageAsync(It.IsAny<string>())).ReturnsAsync("/CheckServiceDetails");


        //Act
        var result = await _whatLanguage.OnPostNextPage() as RedirectToPageResult;

        //Assert
        _whatLanguage.ValidationValid.Should().BeTrue();
        callbask.Should().Be(1);
        _whatLanguage.LanguageCode.Should().BeEquivalentTo(languages);
        _whatLanguage.LanguageNumber.Should().Be(languages.Count);
        ArgumentNullException.ThrowIfNull(result);
        result.PageName.Should().Be("CheckServiceDetails");
    }


}
