using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.Areas.MyAccount.Pages;
using FamilyHubs.SharedKernel.Identity.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests.Areas.MyAccount
{
    public class ChangeNameConfirmationTests
    {
        private readonly Mock<ICacheService> _mockCacheService;        
        
        private HttpContext _httpContext;

        public ChangeNameConfirmationTests()
        {
            _mockCacheService = new Mock<ICacheService>();            
        }

        [Fact]
        public async Task OnGet_Valid_SetNewNameFromCache()
        {
            //  Arrange            
            _mockCacheService.Setup(x => x.RetrieveFamilyHubsUser()).ReturnsAsync(new FamilyHubsUser { FullName = "test name"});
            var sut = new ChangeNameConfirmationModel( _mockCacheService.Object )
            {
                PageContext = { HttpContext = _httpContext }
            };

            //  Act
            await sut.OnGet();

            //  Assert
            Assert.Equal("test name", sut.NewName);
        }

        


    }

}
