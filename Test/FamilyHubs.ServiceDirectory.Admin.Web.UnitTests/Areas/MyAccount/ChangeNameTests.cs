using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.Areas.MyAccount.Pages;
using FamilyHubs.SharedKernel.Identity.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests.Areas.MyAccount
{
    public class ChangeNameTests
    {
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Mock<IIdamClient> _mockIdamClient;
        
        private HttpContext _httpContext;

        public ChangeNameTests()
        {
            _mockCacheService = new Mock<ICacheService>(); 
            _mockIdamClient = new Mock<IIdamClient>();

        }

        [Fact]
        public async Task OnGet_Valid_SetFullNameFromCache()
        {
            //  Arrange           
            _mockCacheService.Setup(x => x.RetrieveFamilyHubsUser()).ReturnsAsync(new FamilyHubsUser { FullName = "test name"});
            var sut = new ChangeNameModel(_mockIdamClient.Object, _mockCacheService.Object )
            {
                PageContext = { HttpContext = _httpContext }
            };

            //  Act
            await sut.OnGet();

            //  Assert
            Assert.Equal("test name", sut.FullName);
        }


        [Fact]
        public async Task OnPost_Valid_UpdateAccountAndCache()
        {
            //  Arrange
            var user = new FamilyHubsUser { FullName = "oldName", AccountId = "1" };
            _mockCacheService.Setup(x => x.RetrieveFamilyHubsUser()).ReturnsAsync(user);
            _mockIdamClient.Setup(x => x.UpdateAccount(It.IsAny<UpdateAccountDto>()));
            var sut = new ChangeNameModel(_mockIdamClient.Object, _mockCacheService.Object)
            {
                PageContext = { HttpContext = _httpContext },
                FullName = "newName"
            };
            

            //  Act
            var result = await sut.OnPost();

            //  Assert
            _mockIdamClient.Verify(x=>x.UpdateAccount(It.IsAny<UpdateAccountDto>()), Times.Once);
            _mockCacheService.Verify(x=> x.ResetFamilyHubsUser(), Times.Once);
            _mockCacheService.Verify(x => x.StoreFamilyHubsUser(It.IsAny<FamilyHubsUser>()), Times.Once);
            Assert.Equal("ChangeNameConfirmation", ((RedirectToPageResult)result).PageName);
        }

        [Fact]
        public async Task OnPost_NotValid_NoNameHasValidationError()
        {
            //  Arrange
            var user = new FamilyHubsUser { FullName = "oldName", AccountId = "1" };
            _mockCacheService.Setup(x => x.RetrieveFamilyHubsUser()).ReturnsAsync(user);
            _mockIdamClient.Setup(x => x.UpdateAccount(It.IsAny<UpdateAccountDto>()));
            var sut = new ChangeNameModel(_mockIdamClient.Object, _mockCacheService.Object)
            {
                PageContext = { HttpContext = _httpContext },                
            };

            //  Act
            var result = await sut.OnPost();

            //  Assert
            Assert.Equal(true, sut.HasValidationError);            
        }



    }

}
