using FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.UnitTests.Pages.OrganisationAdmin
{
    public class OrganisationAdminTypeOfService
    {
        private TypeOfServiceModel typeOfServiceModel;

        public OrganisationAdminTypeOfService()
        {
            var mockOpenReferralOrganisationAdminCLientService = new Mock<IOpenReferralOrganisationAdminClientService>();
            var mockTaxonomyService = new Mock<ITaxonomyService>();
            var mockISessionService = new Mock<ISessionService>();
            var mockIRedisCacheService = new Mock<IRedisCacheService>();
            typeOfServiceModel = new TypeOfServiceModel(mockOpenReferralOrganisationAdminCLientService.Object, mockTaxonomyService.Object, mockISessionService.Object, mockIRedisCacheService.Object);
        }

        [Fact]
        public async Task ValidationShouldFailWhenNoOptionSelected()
        {
            //Arrange
            typeOfServiceModel.TaxonomySelection = new List<string>();

            // Act
            var result = (await typeOfServiceModel.OnPost()) as RedirectToPageResult;

            // Assert
            typeOfServiceModel.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task ValidationShouldNotFailWhenAnOptionSelected()
        {
            //Arrange
            typeOfServiceModel.TaxonomySelection = new List<string>();
            typeOfServiceModel.TaxonomySelection.Add("Children");

            // Act
            var result = (await typeOfServiceModel.OnPost()) as RedirectToPageResult;

            // Assert
            typeOfServiceModel.ModelState.IsValid.Should().BeTrue();
        }
    }
}
