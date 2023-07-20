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
    public class WhichVcsOrganisationTests
    {
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Mock<IServiceDirectoryClient> _serviceDirectoryClient;
        private readonly Fixture _fixture;
        private const string ValidVcsOrganisation = "ValidLocalAuthority";
        private const long ValidVcsOrganisationId = 1234;
        private const string TooLong = "TooLongStringMoreThan255Characters12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";

        public WhichVcsOrganisationTests()
        {
            _fixture = new Fixture();
            var organisations = _fixture.Create<List<OrganisationDto>>();

            organisations[0].Id= ValidVcsOrganisationId;
            organisations[0].Name = ValidVcsOrganisation;
            for (var i = 1; i < organisations.Count; i++)
            {
                organisations[i].Id = i;
            }
            

            _mockCacheService = new Mock<ICacheService>();
            _mockCacheService.Setup(m => m.GetOrganisations()).ReturnsAsync(organisations);

            _serviceDirectoryClient = new Mock<IServiceDirectoryClient>();
            _serviceDirectoryClient.Setup(x => x.GetCachedVcsOrganisations(It.IsAny<long>(), CancellationToken.None))
                .ReturnsAsync(new List<OrganisationDto>(new [] { new OrganisationDto
                    {
                        OrganisationType = OrganisationType.LA,
                        Name = ValidVcsOrganisation,
                        Description = "Test",
                        AdminAreaCode = "Test",
                        Id = ValidVcsOrganisationId
                    }
                }));
        }

        [Fact]
        public async Task OnGet_VcsOrganisationName_Set()
        {
            //  Arrange
            var permissionModel = _fixture.Create<PermissionModel>();
            _mockCacheService.Setup(m => m.GetPermissionModel(It.IsAny<string>())).ReturnsAsync(permissionModel);
            var sut = new WhichVcsOrganisation(_mockCacheService.Object, _serviceDirectoryClient.Object) 
            { 
                VcsOrganisationName = string.Empty, 
                VcsOrganisations = new List<string>() 
            };

            //  Act
            await sut.OnGet();

            //  Assert
            Assert.Equal(permissionModel.VcsOrganisationName, sut.VcsOrganisationName);

        }

        [Fact]
        public async Task OnGet_BackLink_Set()
        {
            //  Arrange
            var permissionModel = _fixture.Create<PermissionModel>();
            permissionModel.VcsManager = true;
            permissionModel.VcsProfessional = true;
            _mockCacheService.Setup(m => m.GetPermissionModel(It.IsAny<string>())).ReturnsAsync(permissionModel);
            var sut = new WhichVcsOrganisation(_mockCacheService.Object, _serviceDirectoryClient.Object)
            {
                VcsOrganisationName = string.Empty,
                VcsOrganisations = new List<string>()
            };

            //  Act
            await sut.OnGet();

            //  Assert
            Assert.Equal("/WhichLocalAuthority", sut.PreviousPageLink);

        }

        [Fact]
        public async Task OnGet_PageHeading_Set()
        {
            //  Arrange
            var permissionModel = _fixture.Create<PermissionModel>();
            permissionModel.VcsManager = true;
            _mockCacheService.Setup(m => m.GetPermissionModel(It.IsAny<string>())).ReturnsAsync(permissionModel);
            var sut = new WhichVcsOrganisation(_mockCacheService.Object, _serviceDirectoryClient.Object)
            {
                VcsOrganisationName = string.Empty,
                VcsOrganisations = new List<string>()
            };

            //  Act
            await sut.OnGet();

            //  Assert
            Assert.Equal("Which organisation do they work for?", sut.PageHeading);

        }

        [Fact]
        public async Task OnPost_ModelStateInvalid_ReturnsPageWithError()
        {
            //  Arrange
            var permissionModel = _fixture.Create<PermissionModel>();
            permissionModel.VcsManager = true;
            _mockCacheService.Setup(m => m.GetPermissionModel(It.IsAny<string>())).ReturnsAsync(permissionModel);
            
            var sut = new WhichVcsOrganisation(_mockCacheService.Object, _serviceDirectoryClient.Object)
            {
                VcsOrganisationName = string.Empty,
                VcsOrganisations = new List<string>()
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
            _mockCacheService.Setup(m => m.GetPermissionModel(It.IsAny<string>())).ReturnsAsync(permissionModel);
            var sut = new WhichVcsOrganisation(_mockCacheService.Object, _serviceDirectoryClient.Object)
            {
                VcsOrganisationName = authorityName,
                VcsOrganisations = new List<string>()
            };
            
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
            permissionModel.VcsManager = true;
            permissionModel.VcsProfessional = true;
            _mockCacheService.Setup(m => m.GetPermissionModel(It.IsAny<string>())).ReturnsAsync(permissionModel);
            var sut = new WhichVcsOrganisation(_mockCacheService.Object, _serviceDirectoryClient.Object)
            {
                VcsOrganisationName = ValidVcsOrganisation,
                VcsOrganisations = new List<string>()
            };

            //  Act
            var result = await sut.OnPost();

            //  Assert
            Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/UserEmail", ((RedirectToPageResult)result).PageName);
        }

        [Fact]
        public async Task OnPost_Valid_SetsValueInCache()
        {
            //  Arrange
            var permissionModel = _fixture.Create<PermissionModel>();
            _mockCacheService.Setup(m => m.GetPermissionModel(It.IsAny<string>())).ReturnsAsync(permissionModel);
            var sut = new WhichVcsOrganisation(_mockCacheService.Object, _serviceDirectoryClient.Object)
            {
                VcsOrganisationName = ValidVcsOrganisation,
                VcsOrganisations = new List<string>()
            };
            
            //  Act
            _ = await sut.OnPost();

            //  Assert
            _mockCacheService.Verify(m => m.StorePermissionModel(
                It.Is<PermissionModel>(arg => arg.VcsOrganisationName == ValidVcsOrganisation 
                    && arg.VcsOrganisationId == ValidVcsOrganisationId), It.IsAny<string>()));
        }
    }
}
