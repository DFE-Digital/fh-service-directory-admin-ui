using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.UnitTests.Pages.OrganisationAdmin
{
    public class OrganisationAdminWhoFor
    {
        private WhoForModel pageModel;

        public OrganisationAdminWhoFor()
        {
            var mockHttpContext = new Mock<HttpContext>();
            var mockISessionService = new Mock<ISessionService>();
            //var orgVm = mockISessionService.Object.RetrieveOrganisationWithService(mockHttpContext.Object);
            var orgVm = new OrganisationViewModel()
            {
                Id = new System.Guid(),
                Name = "Test Org",
                ServiceName = "Test Service"
            };
            mockISessionService.Setup(org => org.RetrieveOrganisationWithService(mockHttpContext.Object))
                               .Returns(orgVm);
            pageModel = new WhoForModel(mockISessionService.Object);
        }

        [Fact]
        public void OnPost_ValidationFails_WhenNoOptionSelected()
        {
            //Arrange
            pageModel.Children = string.Empty;

            // Act
            var result = pageModel.OnPost() as RedirectToPageResult;

            // Assert
            pageModel.ModelState.IsValid.Should().BeFalse();
        }

        [Theory]
        [InlineData("Yes", null, null)]
        [InlineData("Yes", null, "0")]
        [InlineData("Yes", "0", null)]
        [InlineData("Yes", "0", "0")]
        [InlineData("Yes", "1", "0")]
        [InlineData("Yes", "10", "9")]
        public void OnPost_ValidationFails_WhenInvalidAgeRangeSelected(string children, string minAge, string maxAge)
        {
            //Arrange
            pageModel.Children = children;
            pageModel.SelectedMinAge = minAge;
            pageModel.SelectedMaxAge = maxAge;

            // Act
            var result = pageModel.OnPost() as RedirectToPageResult;

            // Assert
            pageModel.ModelState.IsValid.Should().BeFalse();
        }


    }
}