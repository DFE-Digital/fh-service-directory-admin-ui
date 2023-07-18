using AutoFixture;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.Areas.VcsAdmin.Pages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests.Areas.VcsAdmin
{
    public class AddOrganisationTests
    {
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Fixture _fixture;
        private const string TooLong = "TooLongStringMoreThan255Characters12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";
        private HttpContext _httpContext;

        public AddOrganisationTests()
        {
            _mockCacheService = new Mock<ICacheService>();
            _fixture = new Fixture();

            _httpContext = new DefaultHttpContext();
            _httpContext.Request.Headers.Add("Host", "localhost:7216");
            _httpContext.Request.Headers.Add("Referer", "https://localhost:7216/Welcome");
                
        }

        [Fact]
        public async Task OnPost_ModelStateInvalid_ReturnsPageWithError()
        {
            //  Arrange
            var sut = new AddOrganisationModel(_mockCacheService.Object) 
            { 
                PageContext = { HttpContext = _httpContext } 
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
        public async Task OnPost_InvalidName_ReturnsPageWithError(string organisationName)
        {
            //  Arrange
            var sut = new AddOrganisationModel(_mockCacheService.Object)
            {
                OrganisationName = organisationName,
                PageContext = { HttpContext = _httpContext }
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
            var sut = new AddOrganisationModel(_mockCacheService.Object)
            {
                OrganisationName = "Some Name",
                PageContext = { HttpContext = _httpContext }
            };

            //  Act
            var result = await sut.OnPost();

            //  Assert

            Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/AddOrganisationCheckDetails", ((RedirectToPageResult)result).PageName);

        }

        [Fact]
        public async Task OnPost_Valid_SetsValueInCache()
        {
            //  Arrange
            var permissionModel = _fixture.Create<PermissionModel>();
            _mockCacheService.Setup(m => m.GetPermissionModel()).ReturnsAsync(permissionModel);
            var sut = new AddOrganisationModel(_mockCacheService.Object)
            {
                OrganisationName = "Some Name",
                PageContext = { HttpContext = _httpContext }
            };

            //  Act
            _ = await sut.OnPost();

            //  Assert
            _mockCacheService.Verify(m => m.StoreString(It.IsAny<string>(), It.Is<string>(arg => arg == "Some Name")));

        }
    }

}
