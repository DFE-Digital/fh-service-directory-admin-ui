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

public class PayForServiceTests
{
    private readonly Mock<IRequestDistributedCache> _mockRequestDistributedCache;
    private readonly PayForServiceModel _payForService;

    public PayForServiceTests()
    {
        _mockRequestDistributedCache = new Mock<IRequestDistributedCache>();
        DefaultHttpContext httpContext = new DefaultHttpContext();
        _payForService = new PayForServiceModel(_mockRequestDistributedCache.Object);
        _payForService.PageContext.HttpContext = httpContext;
        CompiledPageActionDescriptor compiledPageActionDescriptor = new();
        _payForService.PageContext.ActionDescriptor = compiledPageActionDescriptor;
        _payForService.PageContext.ActionDescriptor.DisplayName = "/PayForService";
    }

    [Fact]
    public async Task ThenPayForServiceOnGetIsSuccessfull()
    {
        //Arrange
        _mockRequestDistributedCache.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(new Core.Models.OrganisationViewModel { ServiceName = "Service Name", MinAge = 2, MaxAge = 15, Languages = new List<string>() { "English", "French" }, IsPayedFor = "Yes", PayUnit = "Hour", Cost = 2.50M, CostDetails = "Some Cost Details" });
        int callback = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callback++);

        // Act
        await _payForService.OnGet();

        // Assert
        callback.Should().Be(1);
        _payForService.IsPayedFor.Should().Be("Yes");
        _payForService.PayUnit.Should().Be("Hour");
        _payForService.Cost.Should().Be(2.50M);
        _payForService.CostDetails.Should().Be("Some Cost Details");

    }

    [Fact]
    public async Task ThenPayForServiceOnGetShouldJustReturnWhenNoViewModel()
    {
        //Arrange
        _mockRequestDistributedCache.Setup(x => x.GetAsync(It.IsAny<string>()));
        int callback = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callback++);

        // Act
        await _payForService.OnGet();

        // Assert
        callback.Should().Be(1);
        _payForService.IsPayedFor.Should().BeNull();
        _payForService.PayUnit.Should().BeNull();
        _payForService.Cost.Should().Be(0.00M);
        _payForService.CostDetails.Should().BeNull();

    }

    [Fact]
    public async Task ThenPayForServiceOnPost_ReturnsInValidWhenIsPayedForNotSelected()
    {
        // Act
        await _payForService.OnPost();

        // Assert
        _payForService.ValidationValid.Should().BeFalse();
        _payForService.OneOptionSelected.Should().BeFalse();
    }

    [Fact]
    public async Task ThenPayForServiceOnPost_ReturnsInValidWhenYes_CircumstancesAndCostDetailsEmpty()
    {
        // Arrange
        _payForService.IsPayedFor = "Yes-Circumstances";

        // Act
        await _payForService.OnPost();

        // Assert
        _payForService.ValidationValid.Should().BeFalse();
        _payForService.OneOptionSelected.Should().BeFalse();
    }

    [Fact]
    public async Task ThenPayForServiceOnPost_ReturnsInValidWhenYes_PayUnitIsEmptyAndCostIsZero()
    {
        // Arrange
        _payForService.IsPayedFor = "Yes";
        _payForService.Cost = 0.0M;
        _payForService.PayUnit = "Hour";

        // Act
        await _payForService.OnPost();

        // Assert
        _payForService.ValidationValid.Should().BeFalse();
        _payForService.CostValid.Should().BeFalse();
    }

    [Fact]
    public async Task ThenPayForServiceOnPost_ReturnsInValidWhenYes_PayUnitIsEmpty()
    {
        // Arrange
        _payForService.IsPayedFor = "Yes";
        _payForService.Cost = 2.5M;

        // Act
        await _payForService.OnPost();

        // Assert
        _payForService.ValidationValid.Should().BeFalse();
        _payForService.UnitSelected.Should().BeFalse();
    }

    [Fact]
    public async Task ThenPayForServiceOnPost_ReturnsNextPage()
    {
        // Arrange
        int callback = 0;
        _payForService.IsPayedFor = "Yes";
        _payForService.Cost = 2.5M;
        _payForService.PayUnit = "Hour";
        _payForService.CostDetails = "Some Cost Details";
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
           .Callback(() => callback++);

        // Act
        var result = await _payForService.OnPost() as RedirectToPageResult;

        // Assert
        callback.Should().Be(1);
        _payForService.ValidationValid.Should().BeTrue();
        ArgumentNullException.ThrowIfNull(result);
        result.PageName.Should().Be("ServiceTimes");
    }

    [Fact]
    public async Task ThenPayForServiceOnPost_ReturnsCheckServiceDetailsPage()
    {
        // Arrange
        int callback = 0;
        _payForService.IsPayedFor = "Yes";
        _payForService.Cost = 2.5M;
        _payForService.PayUnit = "Hour";
        _payForService.CostDetails = "Some Cost Details";
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
           .Callback(() => callback++);
        _mockRequestDistributedCache.Setup(x => x.GetLastPageAsync(It.IsAny<string>())).ReturnsAsync("/CheckServiceDetails");

        // Act
        var result = await _payForService.OnPost() as RedirectToPageResult;

        // Assert
        callback.Should().Be(1);
        _payForService.ValidationValid.Should().BeTrue();
        ArgumentNullException.ThrowIfNull(result);
        result.PageName.Should().Be("CheckServiceDetails");
    }
}
