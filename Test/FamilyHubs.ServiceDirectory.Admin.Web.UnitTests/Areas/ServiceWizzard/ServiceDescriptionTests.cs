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

public class ServiceDescriptionTests
{
    private readonly Mock<IRequestDistributedCache> _mockRequestDistributedCache;
    private readonly ServiceDescriptionModel _serviceDescriptionModel;

    public ServiceDescriptionTests()
    {
        _mockRequestDistributedCache = new Mock<IRequestDistributedCache>();
        DefaultHttpContext httpContext = new DefaultHttpContext();
        _serviceDescriptionModel = new ServiceDescriptionModel(_mockRequestDistributedCache.Object);
        _serviceDescriptionModel.PageContext.HttpContext = httpContext;
        CompiledPageActionDescriptor compiledPageActionDescriptor = new();
        _serviceDescriptionModel.PageContext.ActionDescriptor = compiledPageActionDescriptor;
        _serviceDescriptionModel.PageContext.ActionDescriptor.DisplayName = "/ServiceDescription";
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
            TextPhone = "Textphone",
            ServiceDescription = "Service Description"
        });
        int callback = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callback++);


        // Act
        await _serviceDescriptionModel.OnGet();

        // Assert
        callback.Should().Be(1);
        _serviceDescriptionModel.Description.Should().Be("Service Description");
    }

    [Fact]
    public async Task ThenContactDetailsOnGet_WithNoViewModel()
    {
        //Arrange
        _mockRequestDistributedCache.Setup(x => x.GetAsync(It.IsAny<string>()));
        int callback = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callback++);

        // Act
        await _serviceDescriptionModel.OnGet();

        // Assert
        callback.Should().Be(1);
        _serviceDescriptionModel.Description.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task ThenOnPost_WithDescriptionThatIsOver500CharactersIsInvalid()
    {
        //Arrange
        _serviceDescriptionModel.Description = new string('A', 501);

        // Act
        await _serviceDescriptionModel.OnPost();

        // Assert
        _serviceDescriptionModel.ModelState.IsValid.Should().BeFalse();
     
    }

    [Fact]
    public async Task ThenOnPost()
    {
        // Arrange
        int callback = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callback++);
        _serviceDescriptionModel.Description = "Service Description";
        

        // Act
        var result = await _serviceDescriptionModel.OnPost() as RedirectToPageResult;

        // Assert
        callback.Should().Be(1);
        ArgumentNullException.ThrowIfNull(result);
        result.PageName.Should().Be("CheckServiceDetails");

    }
}
