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
    public class TypeOfUserLaTests
    {
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Fixture _fixture;

        public TypeOfUserLaTests()
        {
            _mockCacheService = new Mock<ICacheService>();
            _fixture = new Fixture();
        }

        [Theory]
        [InlineData(true, false, "Admin")]
        [InlineData(false, true, "Professional")]
        [InlineData(false, false, "")]
        public async Task OnGet_UserTypeForLa_SetToExpected(bool isLaAdmin, bool isLaProfessional, string expectedResult)
        {
            //  Arrange
            var permissionModel = _fixture.Create<PermissionModel>();
            permissionModel.LaAdmin= isLaAdmin;
            permissionModel.LaProfessional= isLaProfessional;

            _mockCacheService.Setup(m => m.GetPermissionModel()).ReturnsAsync(permissionModel);
            var sut = new TypeOfUserLa(_mockCacheService.Object) { UserTypeForLa = string.Empty };

            //  Act
            await sut.OnGet();

            //  Assert
            Assert.Equal(expectedResult, sut.UserTypeForLa);

        }

        [Fact]
        public async Task OnPost_ModelStateInvalid_ReturnsPageWithError()
        {
            //  Arrange
            var sut = new TypeOfUserLa(_mockCacheService.Object) { UserTypeForLa = string.Empty };
            sut.ModelState.AddModelError("SomeError", "SomeErrorMessage");

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
            var sut = new TypeOfUserLa(_mockCacheService.Object) { UserTypeForLa = "Admin" };

            //  Act
            var result = await sut.OnPost();

            //  Assert

            Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/WhichLocalAuthority", ((RedirectToPageResult)result).PageName);

        }

        [Theory]
        [InlineData("Admin", true, false)]
        [InlineData("Professional", false, true)]
        public async Task OnPost_Valid_SetsValueInCache(string userTypeForLa, bool expectedAdmin, bool expectedProfessional)
        {
            //  Arrange
            var permissionModel = _fixture.Create<PermissionModel>();
            _mockCacheService.Setup(m => m.GetPermissionModel()).ReturnsAsync(permissionModel);
            var sut = new TypeOfUserLa(_mockCacheService.Object) { UserTypeForLa = userTypeForLa };

            //  Act
            _ = await sut.OnPost();

            //  Assert
            _mockCacheService.Verify(m => m.StorePermissionModel(
                It.Is<PermissionModel>(arg => arg.LaAdmin == expectedAdmin && arg.LaProfessional == expectedProfessional)));

        }
    }
}
