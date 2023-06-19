using AutoFixture;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages;
using Microsoft.AspNetCore.Mvc;
using Moq;
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
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public async Task OnGet_UserTypeForLa_SetToExpected(bool isLaManager, bool isLaProfessional)
        {
            //  Arrange
            var permissionModel = _fixture.Create<PermissionModel>();
            permissionModel.LaManager = isLaManager;
            permissionModel.LaProfessional = isLaProfessional;

            _mockCacheService.Setup(m => m.GetPermissionModel()).ReturnsAsync(permissionModel);
            var sut = new TypeOfUserLa(_mockCacheService.Object) { LaProfessional = false, LaManager = false };

            //  Act
            await sut.OnGet();

            //  Assert
            Assert.Equal(isLaManager, sut.LaManager);
            Assert.Equal(isLaProfessional, sut.LaProfessional);
        }

        [Fact]
        public async Task OnPost_ModelStateInvalid_ReturnsPageWithError()
        {
            //  Arrange
            var permissionModel = _fixture.Create<PermissionModel>();
            _mockCacheService.Setup(m => m.GetPermissionModel()).ReturnsAsync(permissionModel);
            
            var sut = new TypeOfUserLa(_mockCacheService.Object) { LaProfessional = false, LaManager = false };
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
            var sut = new TypeOfUserLa(_mockCacheService.Object) { LaManager = true };

            //  Act
            var result = await sut.OnPost();

            //  Assert

            Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/WhichLocalAuthority", ((RedirectToPageResult) result).PageName);
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public async Task OnPost_Valid_SetsValueInCache(bool expectedManager, bool expectedProfessional)
        {
            //  Arrange
            var permissionModel = _fixture.Create<PermissionModel>();
            _mockCacheService.Setup(m => m.GetPermissionModel()).ReturnsAsync(permissionModel);
            var sut = new TypeOfUserLa(_mockCacheService.Object)
                { LaProfessional = expectedProfessional, LaManager = expectedManager };

            //  Act
            _ = await sut.OnPost();

            //  Assert
            _mockCacheService.Verify(m => m.StorePermissionModel(
                It.Is<PermissionModel>(arg =>
                    arg.LaManager == expectedManager && arg.LaProfessional == expectedProfessional &&
                    arg.LaManager == expectedManager)));
        }
    }
}