using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages.ManagePermissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests.Areas.AccountAdmin.ManagePermissions
{
    public class DeleteUserTests
    {
        private readonly Mock<IIdamClient> _mockIdamClient;
        private readonly Mock<ICacheService> _mockCacheService;
        public DeleteUserTests()
        {
            _mockIdamClient = new Mock<IIdamClient>();
            _mockCacheService = new Mock<ICacheService>();
        }

        [Fact]
        public async Task OnGet_BackPathSet()
        {
            //  Arrange
            const long accountId = 1;
            var account = new AccountDto
            {
                Name = "TestUser"
            };
#pragma warning disable CS8620 
            _mockIdamClient.Setup(x => x.GetAccountById(It.IsAny<long>())).Returns(Task.FromResult(account));
#pragma warning restore CS8620 
            _mockCacheService.Setup(x => x.RetrieveLastPageName()).Returns(Task.FromResult("testurl"));
            
            var sut = new DeleteUserModel(_mockIdamClient.Object, _mockCacheService.Object);

            //  Act
            await sut.OnGet(accountId);

            //  Assert
            Assert.Equal("testurl", sut.BackButtonPath);
        }

        [Fact]
        public async Task OnGet_AccountRetrievedFromIdams()
        {
            //  Arrange
            const long accountId = 1;
            var account = new AccountDto
            {
                Name = "TestUser"                
            };
#pragma warning disable CS8620 
            _mockIdamClient.Setup(x => x.GetAccountById(It.IsAny<long>())).Returns(Task.FromResult(account));
#pragma warning restore CS8620 

            var sut = new DeleteUserModel(_mockIdamClient.Object, _mockCacheService.Object);

            //  Act
            await sut.OnGet(accountId);

            //  Assert
            _mockIdamClient.Verify(x=>x.GetAccountById(accountId), Times.Once());
        }

        [Fact]
        public async Task OnGet_UserNameStoredInCache()
        {
            //  Arrange
            const long accountId = 1;
            const string userName = "TestUser";
            var account = new AccountDto
            {
                Name = userName                
            };
#pragma warning disable CS8620 
            _mockIdamClient.Setup(x => x.GetAccountById(It.IsAny<long>())).Returns(Task.FromResult(account));
#pragma warning restore CS8620 
            
            var sut = new DeleteUserModel(_mockIdamClient.Object, _mockCacheService.Object);

            //  Act
            await sut.OnGet(accountId);

            //  Assert
            _mockCacheService.Verify(x => x.StoreString("DeleteUserName", userName), Times.Once());
        }        

        [Fact]
        public async Task OnPost_NoActionSelected_ReturnsPage()
        {
            //  Arrange
            const long accountId = 1;                        
            var sut = new DeleteUserModel(_mockIdamClient.Object, _mockCacheService.Object) ;
            sut.ModelState.AddModelError("test", "test");
            
            //  Act
            var result = await sut.OnPost(accountId);

            //  Assert
            Assert.IsType<PageResult>(result);
            Assert.True(sut.HasValidationError);
        }

        [Fact]
        public async Task OnPost_InvokesDeleteAccountMethodWhenYesSelected()
        {
            //  Arrange
            const long accountId = 1;
            
            _mockIdamClient.Setup(x => x.DeleteAccount(It.IsAny<long>()));
            var sut = new DeleteUserModel(_mockIdamClient.Object, _mockCacheService.Object) ;
            sut.DeleteUser = true;
           
            //  Act
            var result = await sut.OnPost(accountId);

            //  Assert
            Assert.IsType<RedirectToPageResult>(result);
            Assert.False(sut.HasValidationError);
            _mockIdamClient.Verify(m => m.DeleteAccount(accountId), Times.Once);
        }

        [Fact]
        public async Task OnPost_NotInvokesDeleteAccountMethodWhenNoSelected()
        {
            //  Arrange
            const long accountId = 1;            
            var sut = new DeleteUserModel(_mockIdamClient.Object, _mockCacheService.Object);
            sut.DeleteUser = false;
            
            //  Act
            var result = await sut.OnPost(accountId);

            //  Assert
            Assert.IsType<RedirectToPageResult>(result);
            Assert.False(sut.HasValidationError);
            _mockIdamClient.Verify(m => m.DeleteAccount(It.IsAny<long>()), Times.Never);
        }
    }
}
