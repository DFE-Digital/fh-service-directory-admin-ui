using AutoFixture;
using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests.Areas.AccountAdmin
{
    public class UserEmailTests
    {
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Mock<IIdamClient> _mockIIdamClient;
        private readonly Fixture _fixture;

        public UserEmailTests()
        {
            _mockCacheService = new Mock<ICacheService>();
            _mockIIdamClient = new Mock<IIdamClient>();
            _fixture = new Fixture();
        }

        [Fact]
        public async Task OnGet_EmailAddress_Set()
        {
            //  Arrange
            var permissionModel = _fixture.Create<PermissionModel>();
            _mockCacheService.Setup(m => m.GetPermissionModel(It.IsAny<string>())).ReturnsAsync(permissionModel);
            var sut = new UserEmail(_mockCacheService.Object, _mockIIdamClient.Object) { EmailAddress = string.Empty };

            //  Act
            await sut.OnGet();

            //  Assert
            Assert.Equal(permissionModel.EmailAddress, sut.EmailAddress);
        }

        [Fact]
        public async Task OnPost_ModelStateInvalid_ReturnsPageWithError()
        {
            //  Arrange
            var permissionModel = _fixture.Create<PermissionModel>();
            _mockCacheService.Setup(m => m.GetPermissionModel(It.IsAny<string>())).ReturnsAsync(permissionModel);
            var sut = new UserEmail(_mockCacheService.Object, _mockIIdamClient.Object) { EmailAddress = string.Empty };
            sut.ModelState.AddModelError("SomeError", "SomeErrorMessage");

            //  Act
            await sut.OnPost();

            //  Assert
            Assert.True(sut.HasValidationError);
        }

        [Theory]
        [InlineData("invalidemail")]
        [InlineData("nodomain@i")]
        [InlineData("noAt.i")]
        public async Task OnPost_InvalidEmail_ReturnsPageWithError(string email)
        {
            //  Arrange
            var permissionModel = _fixture.Create<PermissionModel>();
            _mockCacheService.Setup(m => m.GetPermissionModel(It.IsAny<string>())).ReturnsAsync(permissionModel);
            var sut = new UserEmail(_mockCacheService.Object, _mockIIdamClient.Object) { EmailAddress = email };

            //  Act
            await sut.OnPost();

            //  Assert
            Assert.True(sut.HasValidationError);
        }

        [Fact]
        public async Task OnPost_Valid_RedirectsToExpectedPage()
        {
            //  Arrange
            var permissionModel = _fixture.Create<PermissionModel>();
            _mockCacheService.Setup(m => m.GetPermissionModel(It.IsAny<string>())).ReturnsAsync(permissionModel);
            var sut = new UserEmail(_mockCacheService.Object, _mockIIdamClient.Object) { EmailAddress = "someone@domain.com" };

            //  Act
            var result = await sut.OnPost();

            //  Assert

            Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/UserName", ((RedirectToPageResult)result).PageName);
        }

        [Fact]
        public async Task OnPost_Valid_SetsValueInCache()
        {
            //  Arrange
            var permissionModel = _fixture.Create<PermissionModel>();
            _mockCacheService.Setup(m => m.GetPermissionModel(It.IsAny<string>())).ReturnsAsync(permissionModel);
            var sut = new UserEmail(_mockCacheService.Object, _mockIIdamClient.Object) { EmailAddress = "someone@domain.com" };

            //  Act
            _ = await sut.OnPost();

            //  Assert
            _mockCacheService.Verify(m => m.StorePermissionModel(
                It.Is<PermissionModel>(arg => arg.EmailAddress == "someone@domain.com"), It.IsAny<string>()));
        }
    }
}
