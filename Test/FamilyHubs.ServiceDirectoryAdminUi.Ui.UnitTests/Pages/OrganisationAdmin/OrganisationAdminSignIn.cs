using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using Xunit;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using Moq;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.UnitTests
{
    public class OrganisationAdminSignIn
    {
        [Fact]
        public void RedirectToOrganizationPage()
        {
            // Arrange
            var mockSessionService = new Mock<ISessionService>();
            var mockIRedisCacheService = new Mock<IRedisCacheService>();
            var sut = new FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin.SignInModel(mockSessionService.Object, mockIRedisCacheService.Object);

            // Act
            var result = sut.OnPost() as RedirectToRouteResult;

            // Assert
            result.Should().BeNull();
        }
    }
}