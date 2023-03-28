using System.Collections.Generic;
using System.Threading.Tasks;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.UnitTests.Pages.OrganisationAdmin
{
    public class OrganisationAdminTypeOfService
    {
        private readonly TypeOfServiceModel typeOfServiceModel;

        public OrganisationAdminTypeOfService()
        {
            var mockOrganisationAdminCLientService = new Mock<IOrganisationAdminClientService>();
            var mockTaxonomyService = new Mock<ITaxonomyService>();
            var mockISessionService = new Mock<ISessionService>();
            var mockIRedisCacheService = new Mock<IRedisCacheService>();
            typeOfServiceModel = new TypeOfServiceModel(mockOrganisationAdminCLientService.Object, mockTaxonomyService.Object, mockISessionService.Object, mockIRedisCacheService.Object);
        }

        [Fact]
        public async Task ValidationShouldFailWhenNoCategorySelected()
        {
            //Arrange
            typeOfServiceModel.CategorySelection = new List<long>();
            typeOfServiceModel.SubcategorySelection = new List<long>();

            // Act
            var result = (await typeOfServiceModel.OnPost()) as RedirectToPageResult;

            // Assert
            typeOfServiceModel.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task ValidationShouldNotFailWhenAnOptionSelected()
        {
            //Arrange
            const long TRANSPORT_ID = 1;
            const long COMMUNITY_TRANSPORT_ID = 2;


            typeOfServiceModel.CategorySelection = new List<long>();
            typeOfServiceModel.CategorySelection.Add(TRANSPORT_ID);
            typeOfServiceModel.SubcategorySelection = new List<long>();
            typeOfServiceModel.SubcategorySelection.Add(COMMUNITY_TRANSPORT_ID);
            var parent = new TaxonomyDto { Id = TRANSPORT_ID, Name = "Transport", TaxonomyType = TaxonomyType.ServiceCategory };
            var child = new TaxonomyDto { Id = COMMUNITY_TRANSPORT_ID, Name = "Community transport", ParentId = TRANSPORT_ID, TaxonomyType = TaxonomyType.ServiceCategory };
            List<TaxonomyDto> children= new List<TaxonomyDto>();
            children.Add(child);
            var pair = new KeyValuePair<TaxonomyDto, List<TaxonomyDto>>(parent, children);
            typeOfServiceModel.Categories = new List<KeyValuePair<TaxonomyDto, List<TaxonomyDto>>>();
            typeOfServiceModel.Categories.Add(pair);

            // Act
            var result = (await typeOfServiceModel.OnPost()) as RedirectToPageResult;

            // Assert
            typeOfServiceModel.ModelState.IsValid.Should().BeTrue();
        }
    }
}
