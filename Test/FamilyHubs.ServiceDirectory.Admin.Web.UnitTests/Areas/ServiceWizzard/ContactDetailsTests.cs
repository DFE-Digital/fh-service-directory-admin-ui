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

public class ContactDetailsTests
{
    private readonly Mock<IRequestDistributedCache> _mockRequestDistributedCache;
    private readonly ContactDetailsModel _contactDetails;
    
    public ContactDetailsTests()
    {
        _mockRequestDistributedCache = new Mock<IRequestDistributedCache>();
        DefaultHttpContext httpContext = new DefaultHttpContext();
        _contactDetails = new ContactDetailsModel(_mockRequestDistributedCache.Object);
        _contactDetails.PageContext.HttpContext = httpContext;
        CompiledPageActionDescriptor compiledPageActionDescriptor = new();
        _contactDetails.PageContext.ActionDescriptor = compiledPageActionDescriptor;
        _contactDetails.PageContext.ActionDescriptor.DisplayName = "/ContactDetails";
    }

    [Fact]
    public async Task ThenContactDetailsOnGetIsSuccessfull()
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
            InPersonSelection = new List<string> { "1" },
            Email = "someone@email.com",
            Telephone = "Telephone",
            Website = "Website",
            TextPhone = "Textphone"
        });
        int callbask = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callbask++);
        _contactDetails.ContactSelection = new List<string>();

        // Act
        await _contactDetails.OnGet();

        // Assert
        callbask.Should().Be(1);
        _contactDetails.Email.Should().Be("someone@email.com");
        _contactDetails.Telephone.Should().Be("Telephone");
        _contactDetails.Website.Should().Be("Website");
        _contactDetails.Textphone.Should().Be("Textphone");

    }

    [Fact]
    public async Task ThenContactDetailsOnGet_WithNoViewModel()
    {
        //Arrange
        _mockRequestDistributedCache.Setup(x => x.GetAsync(It.IsAny<string>()));
        int callbask = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callbask++);

        // Act
        await _contactDetails.OnGet();

        // Assert
        callbask.Should().Be(1);
        _contactDetails.Email.Should().BeNullOrEmpty();
        _contactDetails.Telephone.Should().BeNullOrEmpty();
        _contactDetails.Website.Should().BeNullOrEmpty();
        _contactDetails.Textphone.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task ThenContactDetailsOnPost_WithMissingContactData()
    {
        // Arrange
        _contactDetails.ContactSelection = new List<string>();
        

        // Act
        await _contactDetails.OnPost();

        // Assert
        _contactDetails.OneOptionSelected.Should().BeFalse();
        _contactDetails.ValidationValid.Should().BeFalse();
    }

    [Fact]
    public async Task ThenContactDetailsOnPost_WithMissingEmail()
    {
        // Arrange
        _contactDetails.ContactSelection = new List<string>(){ "email", "phone", "website", "textphone" };


        // Act
        await _contactDetails.OnPost();

        // Assert
        _contactDetails.ValidationValid.Should().BeFalse();
        _contactDetails.EmailValid.Should().BeFalse();
    }

    [Fact]
    public async Task ThenContactDetailsOnPost_WithMissingPhone()
    {
        // Arrange
        _contactDetails.Email = "someone@email.com";
        _contactDetails.ContactSelection = new List<string>() { "email", "phone", "website", "textphone" };


        // Act
        await _contactDetails.OnPost();

        // Assert
        _contactDetails.ValidationValid.Should().BeFalse();
        _contactDetails.PhoneValid.Should().BeFalse();
    }

    [Fact]
    public async Task ThenContactDetailsOnPost_WithMissingWebsite()
    {
        // Arrange
        _contactDetails.Email = "someone@email.com";
        _contactDetails.Telephone = "01211112222";
        _contactDetails.ContactSelection = new List<string>() { "email", "phone", "website", "textphone" };


        // Act
        await _contactDetails.OnPost();

        // Assert
        _contactDetails.ValidationValid.Should().BeFalse();
        _contactDetails.WebsiteValid.Should().BeFalse();
    }

    [Fact]
    public async Task ThenContactDetailsOnPost_WithMissingTextPhone()
    {
        // Arrange
        _contactDetails.Email = "someone@email.com";
        _contactDetails.Telephone = "01211112222";
        _contactDetails.Website = "www.someurl.com";
        _contactDetails.ContactSelection = new List<string>() { "email", "phone", "website", "textphone" };


        // Act
        await _contactDetails.OnPost();

        // Assert
        _contactDetails.ValidationValid.Should().BeFalse();
        _contactDetails.TextValid.Should().BeFalse();
    }

    [Fact]
    public async Task ThenContactDetailsOnPost()
    {
        // Arrange
        int callbask = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callbask++);
        _contactDetails.Email = "someone@email.com";
        _contactDetails.Telephone = "01211112222";
        _contactDetails.Website = "www.someurl.com";
        _contactDetails.Textphone = "01211112222";
        _contactDetails.ContactSelection = new List<string>() { "email", "phone", "website", "textphone" };

        // Act
        var result = await _contactDetails.OnPost() as RedirectToPageResult;

        // Assert
        callbask.Should().Be(1);
        _contactDetails.ValidationValid.Should().BeTrue();
        ArgumentNullException.ThrowIfNull(result);
        result.PageName.Should().Be("ServiceDescription");
        
    }

    [Fact]
    public async Task ThenContactDetailsOnPost_ShouldReturnToCheckServiceDetails()
    {
        // Arrange
        int callbask = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callbask++);
        _contactDetails.Email = "someone@email.com";
        _contactDetails.Telephone = "01211112222";
        _contactDetails.Website = "www.someurl.com";
        _contactDetails.Textphone = "01211112222";
        _contactDetails.ContactSelection = new List<string>() { "email", "phone", "website", "textphone" };
        _mockRequestDistributedCache.Setup(x => x.GetLastPageAsync(It.IsAny<string>())).ReturnsAsync("/CheckServiceDetails");

        // Act
        var result = await _contactDetails.OnPost() as RedirectToPageResult;

        // Assert
        callbask.Should().Be(1);
        _contactDetails.ValidationValid.Should().BeTrue();
        ArgumentNullException.ThrowIfNull(result);
        result.PageName.Should().Be("CheckServiceDetails");

    }
}
