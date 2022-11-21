using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using Xunit;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using Moq;
using System.Threading.Tasks;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.UnitTests
{
    public class OrganisationAdminSignIn
    {
        [Fact]
        public async Task RedirectToOrganizationPage()
        {
            // Arrange
            var mockSessionService = new Mock<ISessionService>();
            var mockIRedisCacheService = new Mock<IRedisCacheService>();
            var mockIAuthService = new Mock<IAuthService>();
            var mockITokenService = new Mock<ITokenService>();

            var sut = new FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin.SignInModel(mockSessionService.Object, mockIRedisCacheService.Object, mockIAuthService.Object, mockITokenService.Object);

            // Act
            var result = await sut.OnPost() as RedirectToRouteResult;

            // Assert
            result.Should().BeNull();
        }
    }
}