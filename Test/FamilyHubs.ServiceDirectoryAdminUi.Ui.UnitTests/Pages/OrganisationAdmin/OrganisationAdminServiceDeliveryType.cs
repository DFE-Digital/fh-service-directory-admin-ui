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
    public class OrganisationAdminServiceDeliveryType
    {
        private ServiceDeliveryTypeModel serviceDeliveryTypeModel;

        public OrganisationAdminServiceDeliveryType()
        {
            var mockISessionService = new Mock<ISessionService>();
            serviceDeliveryTypeModel = new ServiceDeliveryTypeModel(mockISessionService.Object);
            serviceDeliveryTypeModel.ServiceDeliverySelection = new List<string>();
        }

        [Fact]
        public void ValidationShouldFailWhenNoOptionSelected()
        {
            // Act
            var result = serviceDeliveryTypeModel.OnPost() as RedirectToPageResult;

            // Assert
            serviceDeliveryTypeModel.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public void ValidationShouldNotFailWhenAnOptionSelected()
        {
            //Arrange
            serviceDeliveryTypeModel.ServiceDeliverySelection.Add("Online");

            // Act
            var result = serviceDeliveryTypeModel.OnPost() as RedirectToPageResult;

            // Assert
            serviceDeliveryTypeModel.ModelState.IsValid.Should().BeTrue();
        }
    }
}
