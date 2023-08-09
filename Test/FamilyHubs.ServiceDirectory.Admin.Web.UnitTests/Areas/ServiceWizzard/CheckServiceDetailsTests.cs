using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.Areas.ServiceWizzard.Pages;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests.Areas.ServiceWizzard;

public class CheckServiceDetailsTests
{
    private readonly Mock<IRequestDistributedCache> _mockRequestDistributedCache;
    private readonly Mock<IServiceDirectoryClient> _mockServiceDirectoryClient;
    private readonly Mock<IViewModelToApiModelHelper> _mockViewModelToApiModelHelper;
    private readonly CheckServiceDetailsModel _checkServiceDetails;
    private readonly Shared.Models.PaginatedList<Shared.Dto.TaxonomyDto> _taxonomyList;
    private readonly List<KeyValuePair<TaxonomyDto, List<TaxonomyDto>>> _taxonomyKeyValues;


    public CheckServiceDetailsTests()
    {
        List<TaxonomyDto> taxonomies = new List<TaxonomyDto>()
        {
            new TaxonomyDto
            {
                Id = 1,
                Name = "First Category",
                TaxonomyType =  TaxonomyType.ServiceCategory,
                ParentId = null
            },
            new TaxonomyDto
            {
                Id = 2,
                Name = "Second Category",
                TaxonomyType =  TaxonomyType.ServiceCategory,
                ParentId = 1
            }
        };

        _taxonomyKeyValues = new List<KeyValuePair<TaxonomyDto, List<TaxonomyDto>>>
        {
            new KeyValuePair<TaxonomyDto, List<TaxonomyDto>>(
                new TaxonomyDto
            {
                Id = 1,
                Name = "First Category",
                TaxonomyType =  TaxonomyType.ServiceCategory,
                ParentId = null
            },new List<TaxonomyDto>()
            {
                new TaxonomyDto
                {
                    Id = 2,
                    Name = "Second Category",
                    TaxonomyType =  TaxonomyType.ServiceCategory,
                    ParentId = 1
                },
                new TaxonomyDto
                {
                    Id = 3,
                    Name = "Third Category",
                    TaxonomyType =  TaxonomyType.ServiceCategory,
                    ParentId = 1
                }
            }
            )
        };

        _taxonomyList = new Shared.Models.PaginatedList<Shared.Dto.TaxonomyDto>(taxonomies, 1, 1, 1);

        _mockViewModelToApiModelHelper = new Mock<IViewModelToApiModelHelper>();
        _mockServiceDirectoryClient  = new Mock<IServiceDirectoryClient>();
        _mockRequestDistributedCache = new Mock<IRequestDistributedCache>();

        _mockServiceDirectoryClient.Setup(x => x.GetTaxonomyList(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<TaxonomyType>())).ReturnsAsync(_taxonomyList);
        var settings = new Dictionary<string, string>
            {
                { "ShowSpreadsheetData", "True" }
            };
        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection((IEnumerable<KeyValuePair<string, string?>>)settings).Build();

        DefaultHttpContext httpContext = new DefaultHttpContext();
        _checkServiceDetails = new CheckServiceDetailsModel(_mockServiceDirectoryClient.Object, _mockViewModelToApiModelHelper.Object, configuration, _mockRequestDistributedCache.Object);
        _checkServiceDetails.PageContext.HttpContext = httpContext;
        CompiledPageActionDescriptor compiledPageActionDescriptor = new();
        _checkServiceDetails.PageContext.ActionDescriptor = compiledPageActionDescriptor;
        _checkServiceDetails.PageContext.ActionDescriptor.DisplayName = "/ServiceDescription";
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ThenCheckServiceDetailsOnGetIsSuccessfull(bool isSameTimeOnEachDay)
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
            IsSameTimeOnEachDay = isSameTimeOnEachDay,
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
            ServiceDescription = "Service Description",
            OpeningHours = new List<OpeningHours>
            {
                new OpeningHours
                {
                    Day = "Monday",
                    StartsTimeOfDay = "am",
                    Starts = "9",
                    FinishesTimeOfDay = "pm",
                    Finishes = "1",
                },
                new OpeningHours
                {
                    Day = "Monday",
                    StartsTimeOfDay = "am",
                    Starts = "2",
                    FinishesTimeOfDay = "pm",
                    Finishes = "5",
                }
            },
            TaxonomySelection = new List<long> { 2L }
        });
        int callback = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callback++);


        // Act
        await _checkServiceDetails.OnGet();

        // Assert
        callback.Should().Be(1);
        
    }

    [Fact]
    public async Task ThenCheckServiceDetailsOnGetWithoutViewModel()
    {
        //Arrange
        _mockRequestDistributedCache.Setup(x => x.GetAsync(It.IsAny<string>()));
        int callback = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callback++);


        // Act
        await _checkServiceDetails.OnGet();

        // Assert
        callback.Should().Be(1);

    }

    [Theory]
    [InlineData("/ServiceAdded")]
    [InlineData("/DetailsSaved")]
    public async Task ThenCheckServiceDetailsOnGetWithoutWhereLastPageIsServiceAddedOrDetailsSaved(string page)
    {
        //Arrange
        _mockRequestDistributedCache.Setup(x => x.GetAsync(It.IsAny<string>()));
        int callback = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callback++);
        _mockRequestDistributedCache.Setup(x => x.GetLastPageAsync(It.IsAny<string>())).ReturnsAsync(page);

        // Act
        var result = await _checkServiceDetails.OnGet() as RedirectToPageResult;
        

        // Assert
        callback.Should().Be(0);
        ArgumentNullException.ThrowIfNull(result);
        result.PageName.Should().Be("ErrorService");

    }

    [Fact]
    public void ThenCheckServiceDetailsOnGetRedirectToViewServicesPage()
    {
        // Act
        var result = _checkServiceDetails.OnGetRedirectToViewServicesPage("1") as RedirectToPageResult;


        // Assert
        ArgumentNullException.ThrowIfNull(result);
        result.PageName.Should().Be("/OrganisationAdmin/ViewServices");

    }

    [Theory]
    [InlineData(0,"ServiceAdded")]
    [InlineData(1,"DetailsSaved")]
    public async Task ThenCheckServiceDetailsOnPost(long id, string page)
    {
        //Arrange
        _mockRequestDistributedCache.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(new Core.Models.OrganisationViewModel
        {
            Id = id,
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
            IsSameTimeOnEachDay = false,
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
            ServiceDescription = "Service Description",
            OpeningHours = new List<OpeningHours>
            {
                new OpeningHours
                {
                    Day = "Monday",
                    StartsTimeOfDay = "am",
                    Starts = "9",
                    FinishesTimeOfDay = "pm",
                    Finishes = "1",
                },
                new OpeningHours
                {
                    Day = "Monday",
                    StartsTimeOfDay = "am",
                    Starts = "2",
                    FinishesTimeOfDay = "pm",
                    Finishes = "5",
                }
            },
            TaxonomySelection = new List<long> { 2L }
        });
        int callback = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callback++);
        _mockViewModelToApiModelHelper.Setup(x => x.GenerateUpdateServiceDto(It.IsAny<OrganisationViewModel>())).ReturnsAsync(new ServiceDto()
        {
            Id = 1,
            Name = "Test",
            ServiceOwnerReferenceId = "ABC1",
            ServiceType = ServiceType.FamilyExperience,
        });
        _mockServiceDirectoryClient.Setup(x => x.CreateService(It.IsAny<ServiceDto>())).ReturnsAsync(1L);
        _mockServiceDirectoryClient.Setup(x => x.UpdateService(It.IsAny<ServiceDto>())).ReturnsAsync(1L);


        // Act
        var result = await _checkServiceDetails.OnPost() as RedirectToPageResult;
        

        // Assert
        callback.Should().Be(1);
        // Assert
        ArgumentNullException.ThrowIfNull(result);
        result.PageName.Should().Be(page);

    }
}
