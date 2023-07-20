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
    public class TypeOfUserVcsTests
    {
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Fixture _fixture;

        public TypeOfUserVcsTests()
        {
            _mockCacheService = new Mock<ICacheService>();
            _fixture = new Fixture();
        }

        [Theory]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task OnGet_ExpectedValuesSet(bool isVcsManager, bool isVcsProfessional)
        {
            //  Arrange
            var permissionModel = _fixture.Create<PermissionModel>();
            permissionModel.VcsManager = isVcsManager;
            permissionModel.VcsProfessional = isVcsProfessional;

            _mockCacheService.Setup(m => m.GetPermissionModel(It.IsAny<string>())).ReturnsAsync(permissionModel);
            var sut = new TypeOfUserVcs(_mockCacheService.Object);

            //  Act
            await sut.OnGet();

            //  Assert
            Assert.Equal(isVcsManager, sut.VcsManager);
            Assert.Equal(isVcsProfessional, sut.VcsProfessional);
        }

        [Fact]
        public async Task OnPost_ModelStateInvalid_ReturnsPageWithError()
        {
            //  Arrange
            var permissionModel = _fixture.Create<PermissionModel>();
            _mockCacheService.Setup(m => m.GetPermissionModel(It.IsAny<string>())).ReturnsAsync(permissionModel);
            
            var sut = new TypeOfUserVcs(_mockCacheService.Object);
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
            _mockCacheService.Setup(m => m.GetPermissionModel(It.IsAny<string>())).ReturnsAsync(permissionModel);
            var sut = new TypeOfUserVcs(_mockCacheService.Object) { VcsManager = true };

            //  Act
            var result = await sut.OnPost();

            //  Assert
            Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/WhichLocalAuthority", ((RedirectToPageResult)result).PageName);
        }

        [Theory]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task OnPost_Valid_SetsValueInCache(bool isVcsManager, bool isVcsProfessional)
        {
            //  Arrange
            var permissionModel = _fixture.Create<PermissionModel>();
            _mockCacheService.Setup(m => m.GetPermissionModel(It.IsAny<string>())).ReturnsAsync(permissionModel);
            var sut = new TypeOfUserVcs(_mockCacheService.Object) { VcsManager = isVcsManager, VcsProfessional = isVcsProfessional };

            //  Act
            _ = await sut.OnPost();

            //  Assert
            _mockCacheService.Verify(m => m.StorePermissionModel(
                It.Is<PermissionModel>(arg => arg.VcsManager == isVcsManager && arg.VcsProfessional == isVcsProfessional), It.IsAny<string>()));

        }
    }
}
