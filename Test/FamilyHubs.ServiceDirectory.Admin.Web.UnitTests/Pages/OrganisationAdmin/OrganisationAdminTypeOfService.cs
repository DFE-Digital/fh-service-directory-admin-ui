using System.Collections.Generic;
using System.Threading.Tasks;
using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.OrganisationAdmin;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests.Pages.OrganisationAdmin;

public class OrganisationAdminTypeOfService
{
    private readonly TypeOfServiceModel _typeOfServiceModel;
    private readonly Mock<ITaxonomyService> _mockTaxonomyService;

    public OrganisationAdminTypeOfService()
    {
        var mockOrganisationAdminClientService = new Mock<IOrganisationAdminClientService>();
        _mockTaxonomyService = new Mock<ITaxonomyService>();
        var mockICacheService = new Mock<ICacheService>();
        _typeOfServiceModel = new TypeOfServiceModel(mockOrganisationAdminClientService.Object, _mockTaxonomyService.Object, mockICacheService.Object);
    }

    [Fact]
    public async Task ValidationShouldFailWhenNoCategorySelected()
    {
        //Arrange
        _typeOfServiceModel.CategorySelection = new List<long>();
        _typeOfServiceModel.SubcategorySelection = new List<long>();
        _mockTaxonomyService.Setup(x => x.GetCategories())
            .ReturnsAsync(new List<KeyValuePair<TaxonomyDto, List<TaxonomyDto>>>());

        // Act
        await _typeOfServiceModel.OnPost();

        // Assert
        _typeOfServiceModel.ModelState.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidationShouldNotFailWhenAnOptionSelected()
    {
        //Arrange
        const long TransportId = 1;
        const long CommunityTransportId = 2;


        _typeOfServiceModel.CategorySelection = new List<long> { TransportId };
        _typeOfServiceModel.SubcategorySelection = new List<long> { CommunityTransportId };
        var parent = new TaxonomyDto
        {
            Id = TransportId,
            Name = "Transport",
            TaxonomyType = TaxonomyType.ServiceCategory
        };
        var child = new TaxonomyDto
        {
            Id = CommunityTransportId,
            Name = "Community transport",
            ParentId = TransportId,
            TaxonomyType = TaxonomyType.ServiceCategory
        };
        var children = new List<TaxonomyDto> { child };
        var pair = new KeyValuePair<TaxonomyDto, List<TaxonomyDto>>(parent, children);

        _mockTaxonomyService.Setup(x => x.GetCategories())
            .ReturnsAsync(new List<KeyValuePair<TaxonomyDto, List<TaxonomyDto>>> { pair });
        // Act
        await _typeOfServiceModel.OnPost();
            
        // Assert
        _typeOfServiceModel.ModelState.IsValid.Should().BeTrue();
    }
}