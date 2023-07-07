using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages.ManagePermissions;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests.Areas.AccountAdmin.ManagePermissions
{
    public class IndexTests
    {
        private readonly Mock<IIdamClient> _mockIdamClient;
        private readonly Mock<ICacheService> _mockCacheService;

        public IndexTests()
        {
            _mockIdamClient = new Mock<IIdamClient>();
            _mockCacheService = new Mock<ICacheService>();
        }

        [Theory]
        [InlineData(null, null, null, null, null, null, null)]
        [InlineData(2, "name", "email", "organisation", true, true, "sorby")]
        public async Task OnGet_SendsCorrectQueryParameters(
            int? pageNumber, string? name, string? email, string? organisation, bool? isLa, bool? isVcs, string? sortBy)
        {
            //  Arrange
            var organisationId = 1;
            var expectedPageNumber = pageNumber ?? 1;
            var sut = new IndexModel(_mockIdamClient.Object, _mockCacheService.Object);
            sut.PageContext.HttpContext = GetMockHttpContext(organisationId, RoleTypes.DfeAdmin).Object;

            //  Act
            await sut.OnGet(pageNumber, name, email, organisation, isLa, isVcs, sortBy);

            //  Assert
            _mockIdamClient.Verify(x => x.GetAccounts(
                organisationId,
                expectedPageNumber,
                name,
                email,
                organisation,
                isLa,
                isVcs,
                sortBy
                ), Times.Once());

        }

        [Fact]
        public void OnPost_Redirects()
        {
            //  Arrange
            var sut = new IndexModel(_mockIdamClient.Object, _mockCacheService.Object);

            //  Act
            var response = sut.OnPost();

            //  Assert
            Assert.IsType<RedirectToPageResult>(response);
        }


        private Mock<HttpContext> GetMockHttpContext(long organisationId, string userRole)
        {
            var mockUser = new Mock<ClaimsPrincipal>();
            var claims = new List<Claim>();
            claims.Add(new Claim(FamilyHubsClaimTypes.OrganisationId, organisationId.ToString()));
            claims.Add(new Claim(FamilyHubsClaimTypes.Role, userRole));

            mockUser.SetupGet(x => x.Claims).Returns(claims);


            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.SetupGet(x => x.User).Returns(mockUser.Object);

            return mockHttpContext;
        }

    }
}
