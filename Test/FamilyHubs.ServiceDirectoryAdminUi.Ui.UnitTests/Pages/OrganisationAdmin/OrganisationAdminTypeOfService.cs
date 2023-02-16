﻿using System.Collections.Generic;
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
            typeOfServiceModel.CategorySelection = new List<string>();
            typeOfServiceModel.SubcategorySelection = new List<string>();

            // Act
            var result = (await typeOfServiceModel.OnPost()) as RedirectToPageResult;

            // Assert
            typeOfServiceModel.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task ValidationShouldNotFailWhenAnOptionSelected()
        {
            //Arrange
            typeOfServiceModel.CategorySelection = new List<string>();
            typeOfServiceModel.CategorySelection.Add("Transport");
            typeOfServiceModel.SubcategorySelection = new List<string>();
            typeOfServiceModel.SubcategorySelection.Add("Community transport");
            var parent = new TaxonomyDto { Id = "Transport", Name = "Transport", Parent = string.Empty, TaxonomyType = TaxonomyType.ServiceCategory };
            var child = new TaxonomyDto { Id = "Community transport", Name = "Community transport", Parent = "Transport", TaxonomyType = TaxonomyType.ServiceCategory };
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
