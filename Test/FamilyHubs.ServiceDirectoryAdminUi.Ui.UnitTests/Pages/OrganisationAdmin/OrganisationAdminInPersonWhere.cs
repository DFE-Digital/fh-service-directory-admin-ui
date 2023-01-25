using FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using FluentAssertions;
using Moq;
using Xunit;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.UnitTests.Pages.OrganisationAdmin
{
    public class OrganisationAdminInPersonWhere
    {
        private readonly InPersonWhereModel inPersonWhereModel;

        public OrganisationAdminInPersonWhere()
        {
            var mockIPostcodeLocationClientService = new Mock<IPostcodeLocationClientService>();
            var mockISessionService = new Mock<ISessionService>();
            var mockIRedisCacheService = new Mock<IRedisCacheService>();
            inPersonWhereModel = new InPersonWhereModel(mockIPostcodeLocationClientService.Object, mockISessionService.Object, mockIRedisCacheService.Object);
        }

        [Fact]
        public void AddressEmpty()
        {
            // Arrange
            inPersonWhereModel.Address1 = "";
            inPersonWhereModel.City = "London";
            inPersonWhereModel.PostalCode = "TW3 2DL";

            //Act
            var result = inPersonWhereModel.OnPost();

            // Assert
            inPersonWhereModel.Address1Valid.Should().BeFalse();
         }

        [Fact]
        public void CityEmpty()
        {
            // Arrange
            inPersonWhereModel.Address1 = "ABCD";
            inPersonWhereModel.City = "";
            inPersonWhereModel.PostalCode = "TW3 2DL";

            // Act
            var result = inPersonWhereModel.OnPost();

            // Assert
            inPersonWhereModel.TownCityValid.Should().BeFalse();
        }

        [Fact]
        public void PostcodeEmpty()
        {
            // Arrange
            inPersonWhereModel.Address1 = "ABCD";
            inPersonWhereModel.City = "London";
            inPersonWhereModel.PostalCode = "";
            
            //Act
            var result = inPersonWhereModel.OnPost();

            // Assert
            inPersonWhereModel.PostcodeValid.Should().BeFalse();
        }

       

        [Fact]
        public void Addressvalid()
        {
            // Arrange
            inPersonWhereModel.Address1 = "ABCD";
            inPersonWhereModel.City = "London";
            inPersonWhereModel.PostalCode = "TW3 2DL";

            // Act
            var result = inPersonWhereModel.OnPost();

            // Assert
            inPersonWhereModel.PostcodeApiValid.Should().BeTrue();
        }
    }
}
