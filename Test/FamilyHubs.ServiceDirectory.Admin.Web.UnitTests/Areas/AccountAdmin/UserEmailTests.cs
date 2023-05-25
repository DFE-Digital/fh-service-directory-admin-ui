using AutoFixture;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests.Areas.AccountAdmin
{
    public class UserEmailTests
    {
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Fixture _fixture;

        public UserEmailTests()
        {
            _mockCacheService = new Mock<ICacheService>();
            _fixture = new Fixture();
        }

        [Fact]
        public void OnGet_EmailAddress_Set()
        {
            //  Arrange
            var permissionModel = _fixture.Create<PermissionModel>();
            _mockCacheService.Setup(m => m.GetPermissionModel()).Returns(permissionModel);
            var sut = new UserEmail(_mockCacheService.Object) { EmailAddress = string.Empty };

            //  Act
            sut.OnGet();

            //  Assert
            Assert.Equal(permissionModel.EmailAddress, sut.EmailAddress);

        }

        [Fact]
        public void OnPost_ModelStateInvalid_ReturnsPageWithError()
        {
            //  Arrange
            var sut = new UserEmail(_mockCacheService.Object) { EmailAddress = string.Empty };
            sut.ModelState.AddModelError("SomeError", "SomeErrorMessage");

            //  Act
            sut.OnPost();

            //  Assert
            Assert.True(sut.HasValidationError);
        }

        [Theory]
        [InlineData("invalidemail")]
        [InlineData("nodomain@i")]
        [InlineData("noAt.i")]
        public void OnPost_InvalidEmail_ReturnsPageWithError(string email)
        {
            //  Arrange
            var permissionModel = _fixture.Create<PermissionModel>();
            _mockCacheService.Setup(m => m.GetPermissionModel()).Returns(permissionModel);
            var sut = new UserEmail(_mockCacheService.Object) { EmailAddress = email };

            //  Act
            sut.OnPost();

            //  Assert
            Assert.True(sut.HasValidationError);
        }

        [Fact]
        public void OnPost_Valid_RedirectsToExpectedPage()
        {
            //  Arrange
            var permissionModel = _fixture.Create<PermissionModel>();
            _mockCacheService.Setup(m => m.GetPermissionModel()).Returns(permissionModel);
            var sut = new UserEmail(_mockCacheService.Object) { EmailAddress = "someone@domain.com" };

            //  Act
            var result = sut.OnPost();

            //  Assert

            Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/UserName", ((RedirectToPageResult)result).PageName);

        }

        [Fact]
        public void OnPost_Valid_SetsValueInCache()
        {
            //  Arrange
            var permissionModel = _fixture.Create<PermissionModel>();
            _mockCacheService.Setup(m => m.GetPermissionModel()).Returns(permissionModel);
            var sut = new UserEmail(_mockCacheService.Object) { EmailAddress = "someone@domain.com" };

            //  Act
            var result = sut.OnPost();

            //  Assert
            _mockCacheService.Verify(m => m.StorePermissionModel(
                It.Is<PermissionModel>(arg => arg.EmailAddress == "someone@domain.com")));

        }


    }
}
