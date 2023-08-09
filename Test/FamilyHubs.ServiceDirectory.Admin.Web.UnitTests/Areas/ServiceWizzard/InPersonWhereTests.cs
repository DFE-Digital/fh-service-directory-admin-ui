using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
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

public class InPersonWhereTests
{
    private readonly Mock<IRequestDistributedCache> _mockRequestDistributedCache;
    private readonly InPersonWhereModel _inPersonWhere;
    private readonly Mock<IPostcodeLocationClientService> _mockPostcodeLocationClientService;

    public InPersonWhereTests()
    {
        _mockPostcodeLocationClientService = new Mock<IPostcodeLocationClientService>();

        
        _mockRequestDistributedCache = new Mock<IRequestDistributedCache>();
        DefaultHttpContext httpContext = new DefaultHttpContext();
        _inPersonWhere = new InPersonWhereModel(_mockRequestDistributedCache.Object, _mockPostcodeLocationClientService.Object);
        _inPersonWhere.PageContext.HttpContext = httpContext;
        CompiledPageActionDescriptor compiledPageActionDescriptor = new();
        _inPersonWhere.PageContext.ActionDescriptor = compiledPageActionDescriptor;
        _inPersonWhere.PageContext.ActionDescriptor.DisplayName = "/InPersonWhere";
    }

    [Fact]
    public async Task ThenServiceDeliveryTypeOnGetIsSuccessfull()
    {
        //Arrange
        _mockRequestDistributedCache.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(new Core.Models.OrganisationViewModel 
        { 
            ServiceName = "Service Name", 
            MinAge = 2, 
            MaxAge = 15, 
            Languages = new List<string>() { "English", "French" }, 
            IsPayedFor = "Yes", 
            PayUnit = "Hour", 
            Cost = 2.50M, 
            CostDetails = "Some Cost Details", 
            HasSetDaysAndTimes = true, 
            DaySelection = new List<string> { "Monday", "Tuesday" }, 
            IsSameTimeOnEachDay = true, 
            ServiceDeliverySelection = new List<string> { "1" },
            Address1 = "First Line|Second Line",
            City = "City",
            StateProvince = "StateProvince",
            PostalCode = "PostCode",
            InPersonSelection = new List<string> { "1" }
        });
        int callback = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callback++);

        // Act
        await _inPersonWhere.OnGet();

        // Assert
        callback.Should().Be(1);
        _inPersonWhere.Address1.Should().Be("First Line");
        _inPersonWhere.Address2.Should().Be("Second Line");
        _inPersonWhere.City.Should().Be("City");
        _inPersonWhere.StateProvince.Should().Be("StateProvince");
        _inPersonWhere.PostalCode.Should().Be("PostCode");

    }

    [Fact]
    public async Task ThenServiceDeliveryTypeOnGetIsSuccessfull_WithSingleAddressLine()
    {
        //Arrange
        _mockRequestDistributedCache.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(new Core.Models.OrganisationViewModel
        {
            ServiceName = "Service Name",
            MinAge = 2,
            MaxAge = 15,
            Languages = new List<string>() { "English", "French" },
            IsPayedFor = "Yes",
            PayUnit = "Hour",
            Cost = 2.50M,
            CostDetails = "Some Cost Details",
            HasSetDaysAndTimes = true,
            DaySelection = new List<string> { "Monday", "Tuesday" },
            IsSameTimeOnEachDay = true,
            ServiceDeliverySelection = new List<string> { "1" },
            Address1 = "First Line",
            City = "City",
            StateProvince = "StateProvince",
            PostalCode = "PostCode",
            InPersonSelection = new List<string> { "1" }
        });
        int callback = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callback++);

        // Act
        await _inPersonWhere.OnGet();

        // Assert
        callback.Should().Be(1);
        _inPersonWhere.Address1.Should().Be("First Line");
        _inPersonWhere.Address2.Should().BeNull();
        _inPersonWhere.City.Should().Be("City");
        _inPersonWhere.StateProvince.Should().Be("StateProvince");
        _inPersonWhere.PostalCode.Should().Be("PostCode");

    }

    [Fact]
    public async Task ThenServiceDeliveryTypeOnGet_WithNoViewModel()
    {
        //Arrange
        _mockRequestDistributedCache.Setup(x => x.GetAsync(It.IsAny<string>()));
        int callback = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callback++);

        // Act
        await _inPersonWhere.OnGet();

        // Assert
        callback.Should().Be(1);
        _inPersonWhere.Address1.Should().BeNullOrEmpty();
        _inPersonWhere.Address2.Should().BeNullOrEmpty();
        _inPersonWhere.City.Should().BeNullOrEmpty();
        _inPersonWhere.StateProvince.Should().BeNullOrEmpty();
        _inPersonWhere.PostalCode.Should().BeNullOrEmpty();

    }

    [Fact]
    public async Task ThenServiceDeliveryTypeOnPost_WithMissingAddressNoViewModel()
    {
        //Arrange
        _inPersonWhere.PostalCode = "Some Post Code";

        // Act
        await _inPersonWhere.OnPost();

        // Assert
        _inPersonWhere.ValidationValid.Should().BeFalse();
        _inPersonWhere.Address1Valid.Should().BeFalse();
        _inPersonWhere.TownCityValid.Should().BeFalse();
        _inPersonWhere.PostcodeValid.Should().BeFalse();
    }

    [Fact]
    public async Task ThenServiceDeliveryTypeOnPost_ReturnNextPage()
    {
        //Arrange
        var response = new PostcodesIoResponse();
        response.Result = new PostcodeInfo
        {
            Longitude = 1.0,
            Latitude = 2.0,
        };

        _mockPostcodeLocationClientService.Setup(x => x.LookupPostcode(It.IsAny<string>())).ReturnsAsync(response);
        _inPersonWhere.InPersonSelection = new List<string>();
        _inPersonWhere.Address1 = "Address1";
        _inPersonWhere.City = "City";
        _inPersonWhere.PostalCode = "Some Post Code";
        _mockRequestDistributedCache.Setup(x => x.GetAsync(It.IsAny<string>()));
        int callback = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
           .Callback(() => callback++);

        // Act
        var result = await _inPersonWhere.OnPost() as RedirectToPageResult;

        // Assert
        // Assert
        callback.Should().Be(1);
        _inPersonWhere.ValidationValid.Should().BeTrue();
        ArgumentNullException.ThrowIfNull(result);
        result.PageName.Should().Be("ContactDetails");
    }

    [Fact]
    public async Task ThenServiceDeliveryTypeOnPost_ReturnCheckServiceDetailsPage()
    {
        //Arrange
        var response = new PostcodesIoResponse();
        response.Result = new PostcodeInfo
        {
            Longitude = 1.0,
            Latitude = 2.0,
        };

        _mockPostcodeLocationClientService.Setup(x => x.LookupPostcode(It.IsAny<string>())).ReturnsAsync(response);
        _inPersonWhere.InPersonSelection = new List<string>();
        _inPersonWhere.Address1 = "Address1";
        _inPersonWhere.City = "City";
        _inPersonWhere.PostalCode = "Some Post Code";
        _mockRequestDistributedCache.Setup(x => x.GetAsync(It.IsAny<string>()));
        int callback = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
           .Callback(() => callback++);
        _mockRequestDistributedCache.Setup(x => x.GetLastPageAsync(It.IsAny<string>())).ReturnsAsync("/CheckServiceDetails");

        // Act
        var result = await _inPersonWhere.OnPost() as RedirectToPageResult;

        // Assert
        callback.Should().Be(1);
        _inPersonWhere.ValidationValid.Should().BeTrue();
        ArgumentNullException.ThrowIfNull(result);
        result.PageName.Should().Be("CheckServiceDetails");
    }

    [Fact]
    public async Task ThenServiceDeliveryTypeOnPost_ReturnInValidPostCode()
    {
        // Arrange
        _mockPostcodeLocationClientService.Setup(x => x.LookupPostcode(It.IsAny<string>())).Throws<Exception>();
        _inPersonWhere.InPersonSelection = new List<string>();
        _inPersonWhere.Address1 = "Address1";
        _inPersonWhere.City = "City";
        _inPersonWhere.PostalCode = "Some Post Code";

        // Act
        await _inPersonWhere.OnPost();

        // Assert
        _inPersonWhere.PostcodeApiValid.Should().BeFalse();
    }
}
