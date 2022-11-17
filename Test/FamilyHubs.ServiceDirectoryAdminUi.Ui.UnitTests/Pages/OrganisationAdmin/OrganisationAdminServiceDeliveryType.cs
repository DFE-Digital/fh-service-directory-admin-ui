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
            var mockIRedisCacheService = new Mock<IRedisCacheService>();
            serviceDeliveryTypeModel = new ServiceDeliveryTypeModel(mockISessionService.Object, mockIRedisCacheService.Object);
            serviceDeliveryTypeModel.ServiceDeliverySelection = new List<string>();
        }

        [Fact]
        public void ValidationShouldFail_WhenNoOptionSelected()
        {
            // Act
            var result = serviceDeliveryTypeModel.OnPost() as RedirectToPageResult;

            // Assert
            serviceDeliveryTypeModel.ValidationValid.Should().BeFalse();
        }

        [Fact]
        public void ValidationShouldNotFail_WhenAnOptionSelected()
        {
            //Arrange
            serviceDeliveryTypeModel.ServiceDeliverySelection.Add("Online");

            // Act
            var result = serviceDeliveryTypeModel.OnPost() as RedirectToPageResult;

            // Assert
            serviceDeliveryTypeModel.ValidationValid.Should().BeTrue();
        }
    }
}
