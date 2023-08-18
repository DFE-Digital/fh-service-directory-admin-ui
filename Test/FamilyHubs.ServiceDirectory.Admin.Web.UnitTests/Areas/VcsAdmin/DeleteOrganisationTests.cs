using AutoFixture;
using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages.ManagePermissions;
using FamilyHubs.ServiceDirectory.Admin.Web.Areas.VcsAdmin.Pages;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.ServiceDirectory.Shared.Models;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests.Areas.VcsAdmin
{
    public class DeleteOrganisationTests
    {
        private readonly Mock<IServiceDirectoryClient> _mockServiceDirectoryClient;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Mock<IIdamClient> _mockIdamClient;
        
        
        public DeleteOrganisationTests()
        {
            _mockServiceDirectoryClient = new Mock<IServiceDirectoryClient>();
            _mockCacheService = new Mock<ICacheService>();
            _mockIdamClient = new Mock<IIdamClient>();                       
        }

        [Fact]
        public async Task OnGet_BackPathSet()
        {
            //  Arrange
            var organisationId = 1;
            _mockCacheService.Setup(x => x.RetrieveLastPageName()).Returns(Task.FromResult("testurl/abc"));

            var sut = new DeleteOrganisationModel(_mockServiceDirectoryClient.Object, _mockCacheService.Object, _mockIdamClient.Object) { DeleteOrganisation = null , BackButtonPath = "testurl"};

            //  Act
            await sut.OnGet(organisationId);

            //  Assert
            Assert.Equal("testurl/abc", sut.BackButtonPath);
        }

        [Fact]
        public async Task OnGet_OranisationNameIsSavedInCache()
        {
            //  Arrange
            var organisationId = 1;
            var organisation = new OrganisationWithServicesDto{ Id= organisationId, Name = "TestOrg", Description="description", AdminAreaCode="code",OrganisationType=OrganisationType.VCFS };
            _mockServiceDirectoryClient.Setup(x => x.GetOrganisationById(It.IsAny<long>())).ReturnsAsync(organisation);
            _mockCacheService.Setup(x => x.StoreString(It.IsAny<string>(), It.IsAny<string>()));
            var sut = new DeleteOrganisationModel(_mockServiceDirectoryClient.Object, _mockCacheService.Object, _mockIdamClient.Object) { DeleteOrganisation = null };

            //  Act
            await sut.OnGet(organisationId);

            //  Assert
            _mockCacheService.Verify(x => x.StoreString("DeleteOrganisationName", organisation.Name), Times.Once);           
        }


        [Fact]
        public async Task OnPost_ErrorIsDisplayedWhenNoSelection()
        {
            //  Arrange
            var organisationId = 1;
            
            var sut = new DeleteOrganisationModel(_mockServiceDirectoryClient.Object, _mockCacheService.Object, _mockIdamClient.Object) { DeleteOrganisation = null };

            //  Act
            var result = await sut.OnPost(organisationId);

            //  Assert
            Assert.IsType<PageResult>(result);
            Assert.True(sut.HasValidationError);
        }

        [Fact]
        public async Task OnPost_UserRedirectedToResultPageWhenNoSelected()
        {
            //  Arrange
            var organisationId = 1;
            
            var sut = new DeleteOrganisationModel(_mockServiceDirectoryClient.Object, _mockCacheService.Object, _mockIdamClient.Object) { DeleteOrganisation = false };

            //  Act
            var result = await sut.OnPost(organisationId);

            //  Assert
            Assert.IsType<RedirectToPageResult>(result);
            Assert.False(sut.HasValidationError);
            Assert.Equal("DeleteOrganisationResult", ((RedirectToPageResult)result).PageName);

        }

        [Fact]
        public async Task OnPost_OrganisationAndAccountsDeletedWhenYesSelected()
        {
            //  Arrange
            var organisationId = 1;
            _mockIdamClient.Setup(x => x.DeleteOrganisationAccounts(It.IsAny<long>()));
            _mockServiceDirectoryClient.Setup(x => x.DeleteOrganisation(It.IsAny<long>()));

            var sut = new DeleteOrganisationModel(_mockServiceDirectoryClient.Object, _mockCacheService.Object, _mockIdamClient.Object) { DeleteOrganisation = true };

            //  Act
            var result = await sut.OnPost(organisationId);

            //  Assert
            _mockIdamClient.Verify(x=>x.DeleteOrganisationAccounts(organisationId), Times.Once);
            _mockServiceDirectoryClient.Verify(x=>x.DeleteOrganisation(organisationId), Times.Once);

            Assert.IsType<RedirectToPageResult>(result);
            Assert.False(sut.HasValidationError);
            Assert.Equal("DeleteOrganisationResult", ((RedirectToPageResult)result).PageName);

        }
    }
}
