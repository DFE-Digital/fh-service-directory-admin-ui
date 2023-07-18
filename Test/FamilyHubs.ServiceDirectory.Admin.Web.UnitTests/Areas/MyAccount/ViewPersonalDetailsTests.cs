using FamilyHubs.ServiceDirectory.Admin.Web.Areas.MyAccount.Pages;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Security.Claims;
using Xunit;

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests.Areas.MyAccount
{
    public class ViewPersonalDetailsTests
    {
        [Fact]
        public void OnGet_Valid_SetFullNameFromCache()
        {
            //  Arrange
            const string expectedName = "test name";
            var claims = new List<Claim> {
                new Claim(FamilyHubsClaimTypes.FullName, expectedName) ,
                new Claim(FamilyHubsClaimTypes.AccountId, "1")
            };
            var mockHttpContext = TestHelper.GetHttpContext(claims);

            var settings = new Dictionary<string, string>
            {
                { "GovUkLoginAccountPage", "testurl" }
            };
            IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection((IEnumerable<KeyValuePair<string, string?>>)settings).Build();

            var sut = new ViewPersonalDetails(configuration)
            {
                PageContext = { HttpContext = mockHttpContext.Object }
            };

            //  Act
            sut.OnGet();

            //  Assert
            Assert.Equal(expectedName, sut.FullName);
        }

        


    }

}
