using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using Xunit;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.UnitTests
{
    public class OrganisationAdminSignIn
    {
        [Fact]
        public void RedirectToOrganizationPage()
        {
            // Arrange
            var sut = new FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin.SignInModel();

            // Act
            var result = sut.OnPost() as RedirectToRouteResult;

            // Assert
            result.Should().BeNull();
        }
    }
}