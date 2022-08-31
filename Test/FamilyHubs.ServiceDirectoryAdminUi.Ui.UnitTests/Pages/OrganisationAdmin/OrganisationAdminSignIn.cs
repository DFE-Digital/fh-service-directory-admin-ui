using Microsoft.AspNetCore.Mvc;
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
            Assert.Equal(result, null);
        }
    }
}