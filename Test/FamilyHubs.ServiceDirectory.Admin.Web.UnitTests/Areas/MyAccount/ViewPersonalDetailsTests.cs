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
    public class ViewPersonalDetailsTests
    {
        private readonly Mock<ICacheService> _mockCacheService;        
        
        private HttpContext _httpContext;

        public ViewPersonalDetailsTests()
        {
            _mockCacheService = new Mock<ICacheService>();            


        }

        [Fact]
        public async Task OnGet_Valid_SetFullNameFromCache()
        {
            //  Arrange
            var settings = new Dictionary<string, string>
            {
                { "GovUkLoginAccountPage", "testurl" }
            };
            IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

            _mockCacheService.Setup(x => x.RetrieveFamilyHubsUser()).ReturnsAsync(new FamilyHubsUser { FullName = "test name"});
            var sut = new ViewPersonalDetails(configuration, _mockCacheService.Object )
            {
                PageContext = { HttpContext = _httpContext }
            };

            //  Act
            await sut.OnGet();

            //  Assert
            Assert.Equal("test name", sut.FullName);
        }

        


    }

}
