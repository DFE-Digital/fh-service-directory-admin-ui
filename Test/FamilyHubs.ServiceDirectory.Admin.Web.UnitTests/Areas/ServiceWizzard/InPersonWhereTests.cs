using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Areas.ServiceWizzard.Pages;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.ServiceDirectory.Shared.Models;
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

    public InPersonWhereTests()
    {
        Mock<IPostcodeLocationClientService> mockPostcodeLocationClientService = new Mock<IPostcodeLocationClientService>();

        var response = new PostcodesIoResponse();
        response.Result = new PostcodeInfo
        {
            Longitude = 1.0,
            Latitude = 2.0,
        };

        mockPostcodeLocationClientService.Setup(x => x.LookupPostcode(It.IsAny<string>())).ReturnsAsync(response);
        _mockRequestDistributedCache = new Mock<IRequestDistributedCache>();
        DefaultHttpContext httpContext = new DefaultHttpContext();
        _inPersonWhere = new InPersonWhereModel(_mockRequestDistributedCache.Object, mockPostcodeLocationClientService.Object);
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
        int callbask = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callbask++);

        // Act
        await _inPersonWhere.OnGet();

        // Assert
        callbask.Should().Be(1);
        _inPersonWhere.Address1.Should().Be("First Line");
        _inPersonWhere.Address2.Should().Be("Second Line");
        _inPersonWhere.City.Should().Be("City");
        _inPersonWhere.StateProvince.Should().Be("StateProvince");
        _inPersonWhere.PostalCode.Should().Be("PostCode");

    }

    [Fact]
    public async Task ThenServiceDeliveryTypeOnGet_WithNoViewModel()
    {
        //Arrange
        _mockRequestDistributedCache.Setup(x => x.GetAsync(It.IsAny<string>()));
        int callbask = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callbask++);

        // Act
        await _inPersonWhere.OnGet();

        // Assert
        callbask.Should().Be(1);
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
        _inPersonWhere.InPersonSelection = new List<string>();
        _inPersonWhere.Address1 = "Address1";
        _inPersonWhere.City = "City";
        _inPersonWhere.PostalCode = "Some Post Code";
        _mockRequestDistributedCache.Setup(x => x.GetAsync(It.IsAny<string>()));
        int callbask = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
           .Callback(() => callbask++);

        // Act
        var result = await _inPersonWhere.OnPost() as RedirectToPageResult;

        // Assert
        // Assert
        callbask.Should().Be(1);
        _inPersonWhere.ValidationValid.Should().BeTrue();
        ArgumentNullException.ThrowIfNull(result);
        result.PageName.Should().Be("ContactDetails");
    }

    [Fact]
    public async Task ThenServiceDeliveryTypeOnPost_ReturnCheckServiceDetailsPage()
    {
        //Arrange
        _inPersonWhere.InPersonSelection = new List<string>();
        _inPersonWhere.Address1 = "Address1";
        _inPersonWhere.City = "City";
        _inPersonWhere.PostalCode = "Some Post Code";
        _mockRequestDistributedCache.Setup(x => x.GetAsync(It.IsAny<string>()));
        int callbask = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
           .Callback(() => callbask++);
        _mockRequestDistributedCache.Setup(x => x.GetLastPageAsync(It.IsAny<string>())).ReturnsAsync("/CheckServiceDetails");

        // Act
        var result = await _inPersonWhere.OnPost() as RedirectToPageResult;

        // Assert
        // Assert
        callbask.Should().Be(1);
        _inPersonWhere.ValidationValid.Should().BeTrue();
        ArgumentNullException.ThrowIfNull(result);
        result.PageName.Should().Be("CheckServiceDetails");
    }

}
