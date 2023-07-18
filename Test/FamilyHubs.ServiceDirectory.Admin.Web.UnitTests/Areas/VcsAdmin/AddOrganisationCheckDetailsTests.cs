using AutoFixture;
using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.Areas.VcsAdmin.Pages;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests.Areas.VcsAdmin
{
    public class AddOrganisationCheckDetailsTests
    {
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Mock<IServiceDirectoryClient> _mockServiceDirectoryClient;
        private readonly Fixture _fixture;
        private HttpContext _httpContext;

        public AddOrganisationCheckDetailsTests()
        {
            _mockCacheService = new Mock<ICacheService>();
            _mockServiceDirectoryClient = new Mock<IServiceDirectoryClient>();
            _fixture = new Fixture();

            _httpContext = new DefaultHttpContext();
            _httpContext.Request.Headers.Add("Host", "localhost:7216");
            _httpContext.Request.Headers.Add("Referer", "https://localhost:7216/Welcome");

        }

        [Fact]
        public async Task OnGet_Valid_SetsOrganisationNameFromCache()
        {
            //  Arrange
            var organisations = _fixture.Create<List<OrganisationDto>>();
            organisations[0].Id = 123;

            _mockServiceDirectoryClient.Setup(x => x.GetCachedLaOrganisations(It.IsAny<CancellationToken>())).ReturnsAsync(organisations);
            _mockCacheService.Setup(x => x.RetrieveString(It.IsAny<string>())).ReturnsAsync("123");

            var sut = new AddOrganisationCheckDetailsModel(_mockCacheService.Object, _mockServiceDirectoryClient.Object)
            {
                PageContext = { HttpContext = _httpContext }
            };

            //  Act
            await sut.OnGet();

            //  Assert
            Assert.Equal(sut.OrganisationName, "123");
        }

        [Fact]
        public async Task OnPost_Valid_CreatesOrganisation()
        {
            //  Arrange            
            _mockCacheService.Setup(x => x.RetrieveString(CacheKeyNames.AddOrganisationName)).ReturnsAsync("Name");
            _mockCacheService.Setup(x => x.RetrieveString(CacheKeyNames.AdminAreaCode)).ReturnsAsync("AdminCode");
            _mockCacheService.Setup(x => x.RetrieveString(CacheKeyNames.LaOrganisationId)).ReturnsAsync("123");
            var args = new List<OrganisationWithServicesDto>();
            _mockServiceDirectoryClient.Setup(x => x.CreateOrganisation(Capture.In<OrganisationWithServicesDto>(args)));
            var sut = new AddOrganisationCheckDetailsModel(_mockCacheService.Object, _mockServiceDirectoryClient.Object)
            {
                PageContext = { HttpContext = _httpContext }
            };

            //  Act
            await sut.OnPost();

            //  Assert            
            _mockServiceDirectoryClient.Verify(x => x.CreateOrganisation(It.IsAny<OrganisationWithServicesDto>()));
            Assert.Equal("Name", args[0].Name);
            Assert.Equal("Name", args[0].Description);
            Assert.Equal("AdminCode", args[0].AdminAreaCode);
            Assert.Equal(123, args[0].AssociatedOrganisationId);


        }

        [Fact]
        public async Task OnPost_Valid_RedirectsToExpectedPage()
        {
            //  Arrange
            _mockCacheService.Setup(x => x.RetrieveString(It.IsAny<string>())).ReturnsAsync("123");
            var sut = new AddOrganisationCheckDetailsModel(_mockCacheService.Object, _mockServiceDirectoryClient.Object)
            {
                PageContext = { HttpContext = _httpContext }
            };

            //  Act
            var result = await sut.OnPost();

            //  Assert
            Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/AddOrganisationResult", ((RedirectToPageResult)result).PageName);

        }


    }

}
