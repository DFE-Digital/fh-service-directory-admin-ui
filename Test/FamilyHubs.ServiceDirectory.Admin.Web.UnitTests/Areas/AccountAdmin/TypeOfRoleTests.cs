﻿using AutoFixture;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests.Areas.AccountAdmin
{
    public class TypeOfRoleTests
    {
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Fixture _fixture;

        public TypeOfRoleTests()
        {
            _mockCacheService = new Mock<ICacheService>();
            _fixture = new Fixture();
        }

        [Fact]
        public void OnGet_OrganisationType_Set()
        {
            //  Arrange
            var permissionModel = _fixture.Create<PermissionModel>();
            _mockCacheService.Setup(m => m.GetPermissionModel()).Returns(permissionModel);
            var sut = new TypeOfRole(_mockCacheService.Object) { OrganisationType = string.Empty};

            //  Act
            sut.OnGet();

            //  Assert
            Assert.Equal(permissionModel.OrganisationType, sut.OrganisationType);

        }

        [Fact]
        public void OnPost_ModelStateInvalid_ReturnsPageWithError()
        {
            //  Arrange
            var sut = new TypeOfRole(_mockCacheService.Object) { OrganisationType = string.Empty };
            sut.ModelState.AddModelError("SomeError", "SomeErrorMessage");

            //  Act
            sut.OnPost();

            //  Assert
            Assert.True(sut.HasValidationError);
        }

        [Theory]
        [InlineData("LA", "/TypeOfUserLa")]
        [InlineData("VCS", "/TypeOfUserVcs")]
        public void OnPost_Valid_RedirectsToExpectedPage(string organisationType, string expectedRoute)
        {
            //  Arrange
            var sut = new TypeOfRole(_mockCacheService.Object) { OrganisationType = organisationType };

            //  Act
            var result = sut.OnPost();

            //  Assert
            Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal(expectedRoute, ((RedirectToPageResult)result).PageName);

        }

        [Fact]
        public void OnPost_Valid_SetsValueInCache()
        {
            //  Arrange
            var sut = new TypeOfRole(_mockCacheService.Object) { OrganisationType = "LA" };

            //  Act
            var result = sut.OnPost();

            //  Assert
            _mockCacheService.Verify(m=>m.StorePermissionModel(It.Is<PermissionModel>(arg => arg.OrganisationType == "LA")));

        }
    }
}