using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Web.Areas.ServiceWizzard.Pages;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
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

public class TypeOfSupportTests
{
    private readonly Mock<IRequestDistributedCache> _mockRequestDistributedCache;
    private readonly Mock<IServiceDirectoryClient> _mockServiceDirectory;
    private readonly Mock<ITaxonomyService> _mockTaxonomyService;
    private readonly Shared.Models.PaginatedList<Shared.Dto.TaxonomyDto> _taxonomyList;
    private readonly List<KeyValuePair<TaxonomyDto, List<TaxonomyDto>>> _taxonomyKeyValues;
    private readonly TypeOfSupportModel _typeOfSupport;

    public TypeOfSupportTests()
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

        _mockRequestDistributedCache = new Mock<IRequestDistributedCache>();
        _mockServiceDirectory = new Mock<IServiceDirectoryClient>();
        _mockTaxonomyService = new Mock<ITaxonomyService>();

        _mockTaxonomyService.Setup(x => x.GetCategories()).ReturnsAsync(_taxonomyKeyValues);
        _mockServiceDirectory.Setup(x => x.GetTaxonomyList(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<TaxonomyType>())).ReturnsAsync(_taxonomyList);
        

        DefaultHttpContext httpContext = new DefaultHttpContext();
        _typeOfSupport = new TypeOfSupportModel(_mockRequestDistributedCache.Object, _mockServiceDirectory.Object, _mockTaxonomyService.Object);
        _typeOfSupport.PageContext.HttpContext = httpContext;
        CompiledPageActionDescriptor compiledPageActionDescriptor = new();
        _typeOfSupport.PageContext.ActionDescriptor = compiledPageActionDescriptor;
        _typeOfSupport.PageContext.ActionDescriptor.DisplayName = "/ServiceName";
    }

    [Fact]
    public async Task ThenTypeOfSupportOnGetIsSuccessfull()
    {
        //Arrange
        int callback = 0;
        _mockRequestDistributedCache.Setup(x => x.SetPageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callback++);
        _mockRequestDistributedCache.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(new Core.Models.OrganisationViewModel
        {
            ServiceName = "Service Name",
            TaxonomySelection = new List<long>
            {
                2,3
            }
        });

        // Act
        await _typeOfSupport.OnGet();

        // Assert
        callback.Should().Be(1);
        _typeOfSupport.Categories.Should().HaveCount(1);
    }

    [Fact]
    public async Task ThenNoCategoriesSelect()
    {
        //Arrange
        _typeOfSupport.CategorySelection = new List<string>();
        _typeOfSupport.SubcategorySelection = new List<string> { "2" };


        //Act
        await _typeOfSupport.OnPost();

        //Assert
#pragma warning disable CS8602
        _typeOfSupport.ModelState.ElementAt(0).Value.ValidationState.Should().Be(Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid);
#pragma warning restore CS8602
    }

    [Fact]
    public async Task ThenOnPostIsSuccessfull_ReturnsNextPage()
    {
        //Arrange
        _typeOfSupport.CategorySelection = new List<string> { "1" };
        _typeOfSupport.SubcategorySelection = new List<string> { "2" };
        _mockRequestDistributedCache.Setup(x => x.GetAsync(It.IsAny<string>()));


        //Act
        var result = await _typeOfSupport.OnPost() as RedirectToPageResult;

        //Assert
        ArgumentNullException.ThrowIfNull(result);
        result.PageName.Should().Be("WhoFor");
    }

    [Fact]
    public async Task ThenOnPostIsSuccessfull_ReturnsCheckServiceDetailsPage()
    {
        //Arrange
        _typeOfSupport.CategorySelection = new List<string> { "1" };
        _typeOfSupport.SubcategorySelection = new List<string> { "2" };
        _mockRequestDistributedCache.Setup(x => x.GetLastPageAsync(It.IsAny<string>())).ReturnsAsync("/CheckServiceDetails");


        //Act
        var result = await _typeOfSupport.OnPost() as RedirectToPageResult;

        //Assert
        ArgumentNullException.ThrowIfNull(result);
        result.PageName.Should().Be("CheckServiceDetails");
    }


}
