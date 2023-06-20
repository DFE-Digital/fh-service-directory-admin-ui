using AutoFixture;
using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using Xunit;

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests.Areas.AccountAdmin
{
    public class WhichLocalAuthorityTests
    {
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Mock<IServiceDirectoryClient> _serviceDirectoryClient;
        private readonly Fixture _fixture;
        private const string ValidLocalAuthority = "ValidLocalAuthority";
        private const long ValidLocalAuthorityId = 1234;
        private const string TooLong = "TooLongStringMoreThan255Characters12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";

        public WhichLocalAuthorityTests()
        {
            _fixture = new Fixture();
            var organisations = _fixture.Create<List<OrganisationDto>>();

            organisations[0].Id= ValidLocalAuthorityId;
            organisations[0].Name = ValidLocalAuthority;
            for (var i = 1; i < organisations.Count; i++)
            {
                organisations[i].Id = i;
            }
            

            _mockCacheService = new Mock<ICacheService>();
            _mockCacheService.Setup(m => m.GetOrganisations()).ReturnsAsync(organisations);

            _serviceDirectoryClient = new Mock<IServiceDirectoryClient>();
            _serviceDirectoryClient.Setup(x => x.GetCachedLaOrganisations(CancellationToken.None))
                .ReturnsAsync(new List<OrganisationDto>(new [] { new OrganisationDto
                    {
                        OrganisationType = OrganisationType.LA,
                        Name = ValidLocalAuthority,
                        Description = "Test",
                        AdminAreaCode = "Test",
                        Id = ValidLocalAuthorityId
                    }
                }));
        }

        [Fact]
        public async Task OnGet_LaOrganisationName_Set()
        {
            //  Arrange
            var permissionModel = _fixture.Create<PermissionModel>();
            _mockCacheService.Setup(m => m.GetPermissionModel()).ReturnsAsync(permissionModel);
            var sut = new WhichLocalAuthority(_mockCacheService.Object, _serviceDirectoryClient.Object) 
            { 
                LaOrganisationName = string.Empty, 
                LocalAuthorities = new List<string>() 
            };

            //  Act
            await sut.OnGet();

            //  Assert
            Assert.Equal(permissionModel.LaOrganisationName, sut.LaOrganisationName);

        }

        [Theory]
        [InlineData(true, "/TypeOfUserVcs")]
        [InlineData(false, "/TypeOfUserLa")]
        public async Task OnGet_BackLink_Set(bool vcsJourney, string expectedPath)
        {
            //  Arrange
            var permissionModel = _fixture.Create<PermissionModel>();
            permissionModel.VcsManager = vcsJourney;
            permissionModel.VcsProfessional = vcsJourney;
            _mockCacheService.Setup(m => m.GetPermissionModel()).ReturnsAsync(permissionModel);
            var sut = new WhichLocalAuthority(_mockCacheService.Object, _serviceDirectoryClient.Object)
            {
                LaOrganisationName = string.Empty,
                LocalAuthorities = new List<string>()
            };

            //  Act
            await sut.OnGet();

            //  Assert
            Assert.Equal(expectedPath, sut.PreviousPageLink);

        }

        [Theory]
        [InlineData(true, "Which local authority area do they work in?")]
        [InlineData(false, "Which local authority do they work for?")]
        public async Task OnGet_PageHeading_Set(bool vcsJourney, string expectedHeading)
        {
            //  Arrange
            var permissionModel = _fixture.Create<PermissionModel>();
            permissionModel.VcsManager = vcsJourney;
            _mockCacheService.Setup(m => m.GetPermissionModel()).ReturnsAsync(permissionModel);
            var sut = new WhichLocalAuthority(_mockCacheService.Object, _serviceDirectoryClient.Object)
            {
                LaOrganisationName = string.Empty,
                LocalAuthorities = new List<string>()
            };

            //  Act
            await sut.OnGet();

            //  Assert
            Assert.Equal(expectedHeading, sut.PageHeading);

        }

        [Fact]
        public async Task OnPost_ModelStateInvalid_ReturnsPageWithError()
        {
            //  Arrange
            var permissionModel = _fixture.Create<PermissionModel>();
            _mockCacheService.Setup(m => m.GetPermissionModel()).ReturnsAsync(permissionModel);
            
            var sut = new WhichLocalAuthority(_mockCacheService.Object, _serviceDirectoryClient.Object)
            {
                LaOrganisationName = string.Empty,
                LocalAuthorities = new List<string>()
            };

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
        public async Task OnPost_InvalidName_ReturnsPageWithError(string authorityName)
        {
            //  Arrange
            var permissionModel = _fixture.Create<PermissionModel>();
            _mockCacheService.Setup(m => m.GetPermissionModel()).ReturnsAsync(permissionModel);
            var sut = new WhichLocalAuthority(_mockCacheService.Object, _serviceDirectoryClient.Object)
            {
                LaOrganisationName = authorityName,
                LocalAuthorities = new List<string>()
            };


            //  Act
            await sut.OnPost();

            //  Assert
            Assert.True(sut.HasValidationError);
        }

        [Theory]
        [InlineData(true, "/WhichVcsOrganisation")]
        [InlineData(false, "/UserEmail")]
        public async Task OnPost_Valid_RedirectsToExpectedPage(bool isVcsJourney, string redirectPage)
        {
            //  Arrange
            var permissionModel = _fixture.Create<PermissionModel>();
            permissionModel.VcsManager = isVcsJourney;
            permissionModel.VcsProfessional = isVcsJourney;
            _mockCacheService.Setup(m => m.GetPermissionModel()).ReturnsAsync(permissionModel);
            var sut = new WhichLocalAuthority(_mockCacheService.Object, _serviceDirectoryClient.Object)
            {
                LaOrganisationName = ValidLocalAuthority,
                LocalAuthorities = new List<string>()
            };


            //  Act
            var result = await sut.OnPost();

            //  Assert

            Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal(redirectPage, ((RedirectToPageResult)result).PageName);

        }

        [Fact]
        public async Task OnPost_Valid_SetsValueInCache()
        {
            //  Arrange
            var permissionModel = _fixture.Create<PermissionModel>();
            _mockCacheService.Setup(m => m.GetPermissionModel()).ReturnsAsync(permissionModel);
            var sut = new WhichLocalAuthority(_mockCacheService.Object, _serviceDirectoryClient.Object)
            {
                LaOrganisationName = ValidLocalAuthority,
                LocalAuthorities = new List<string>()
            };


            //  Act
            _ = await sut.OnPost();

            //  Assert
            _mockCacheService.Verify(m => m.StorePermissionModel(
                It.Is<PermissionModel>(arg => arg.LaOrganisationName == ValidLocalAuthority && arg.LaOrganisationId == ValidLocalAuthorityId)));

        }

    }
}
