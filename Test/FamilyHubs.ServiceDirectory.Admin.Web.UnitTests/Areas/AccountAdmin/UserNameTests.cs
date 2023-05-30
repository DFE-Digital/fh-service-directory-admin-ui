using System.Threading.Tasks;
using AutoFixture;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests.Areas.AccountAdmin
{
    public class UserNameTests
    {
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Fixture _fixture;
        private const string TooLong = "TooLongStringMoreThan255Characters12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";

        public UserNameTests()
        {
            _mockCacheService = new Mock<ICacheService>();
            _fixture = new Fixture();
        }

        [Fact]
        public async Task OnGet_FullName_Set()
        {
            //  Arrange
            var permissionModel = _fixture.Create<PermissionModel>();
            _mockCacheService.Setup(m => m.GetPermissionModel()).ReturnsAsync(permissionModel);
            var sut = new UserName(_mockCacheService.Object) { FullName = string.Empty };

            //  Act
            await sut.OnGet();

            //  Assert
            Assert.Equal(permissionModel.FullName, sut.FullName);

        }

        [Fact]
        public async Task OnPost_ModelStateInvalid_ReturnsPageWithError()
        {
            //  Arrange
            var sut = new UserName(_mockCacheService.Object) { FullName = string.Empty };
            sut.ModelState.AddModelError("SomeError", "SomeErrorMessage");

            //  Act
            await sut.OnPost();

            //  Assert
            Assert.True(sut.HasValidationError);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(TooLong)]
        public async Task OnPost_InvalidName_ReturnsPageWithError(string name)
        {
            //  Arrange
            var permissionModel = _fixture.Create<PermissionModel>();
            _mockCacheService.Setup(m => m.GetPermissionModel()).ReturnsAsync(permissionModel);
            var sut = new UserName(_mockCacheService.Object) { FullName = name };

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
            _mockCacheService.Setup(m => m.GetPermissionModel()).ReturnsAsync(permissionModel);
            var sut = new UserName(_mockCacheService.Object) { FullName = "Someones Name" };

            //  Act
            var result = await sut.OnPost();

            //  Assert

            Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/AddPermissionCheckAnswer", ((RedirectToPageResult)result).PageName);

        }

        [Fact]
        public async Task OnPost_Valid_SetsValueInCache()
        {
            //  Arrange
            var permissionModel = _fixture.Create<PermissionModel>();
            _mockCacheService.Setup(m => m.GetPermissionModel()).ReturnsAsync(permissionModel);
            var sut = new UserName(_mockCacheService.Object) { FullName = "Someones Name" };

            //  Act
            _ = await sut.OnPost();

            //  Assert
            _mockCacheService.Verify(m => m.StorePermissionModel(
                It.Is<PermissionModel>(arg => arg.FullName == "Someones Name")));

        }

    }
}
