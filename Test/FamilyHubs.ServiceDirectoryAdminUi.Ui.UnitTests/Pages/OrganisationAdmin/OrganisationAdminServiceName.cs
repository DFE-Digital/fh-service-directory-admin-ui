using FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.UnitTests.Pages.OrganisationAdmin
{
    public class OrganisationAdminServiceName
    {
        private ServiceNameModel serviceName;
      
        public OrganisationAdminServiceName()
        {
            var mock = new Mock<IOpenReferralOrganisationAdminClientService>();
            serviceName = new ServiceNameModel(mock.Object);
        }


        [Fact]
        public void NullServiceName()
        {
            // Arrange
            serviceName.ServiceName = null;

            // Act
            var result = serviceName.OnPost() as RedirectToPageResult;

            // Assert
            Assert.Equal(serviceName.validationValid, false);
        }

        [Fact]
        public void EmptyServiceName()
        {
            // Arrange
            serviceName.ServiceName = "";

            // Act
            var result = serviceName.OnPost() as RedirectToPageResult;

            // Assert
            Assert.Equal(serviceName.validationValid, false);
        }

        [Fact]
        public void MoreThan255CharServiceName()
        {
            // Arrange
            serviceName.ServiceName = "ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ" +
                "ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ" +
                "ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ";

            // Act
            var result = serviceName.OnPost() as RedirectToPageResult;

            // Assert
            Assert.Equal(serviceName.validationValid, false);
        }

        [Fact]
        public void ValidServiceName()
        {
            // Arrange
            serviceName.ServiceName = "ASDFGHJKLMNOPQRSTUVWXYZ";

            // Act
            var result = serviceName.OnPost() as RedirectToPageResult;

            // Assert
            Assert.Equal(serviceName.validationValid, true);
        }
    }
}
