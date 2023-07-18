using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages.ManagePermissions;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests.Areas.AccountAdmin.ManagePermissions
{
    public class DeleteUserConfirmationTests
    {
        
        private readonly Mock<ICacheService> _mockCacheService;
        public DeleteUserConfirmationTests()
        {            
            _mockCacheService = new Mock<ICacheService>();
        }
            
        [Fact]
        public async Task OnGet_UserNameRetrivedFromCache()
        {
            //  Arrange            
            const string userName = "TestUser";
            const bool isDeleted = true;
            _mockCacheService.Setup(x=>x.RetrieveString(It.IsAny<string>())).Returns(Task.FromResult(userName));
            var sut = new DeleteUserConfirmationModel( _mockCacheService.Object);

            //  Act
            await sut.OnGet(isDeleted);

            //  Assert
            _mockCacheService.Verify(x => x.RetrieveString("DeleteUserName"), Times.Once());
            Assert.Equal(userName, sut.UserName);
        }               
    }
}
