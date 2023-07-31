using AutoFixture;
using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.Areas.VcsAdmin.Pages;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests.Areas.VcsAdmin
{
    public class ManageOrganisationsTests
    {
        private readonly Mock<IServiceDirectoryClient> _mockServiceDirectoryClient;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Fixture _fixture;


        public ManageOrganisationsTests()
        {
            _mockServiceDirectoryClient = new Mock<IServiceDirectoryClient>();
            _mockCacheService = new Mock<ICacheService>();
            _fixture = new Fixture();
        }

        [Fact]
        public async Task OnGet_DfeAdmin_SetsPaginatedList()
        {
            //  Arrange
            var mockHttpContext = GetHttpContext(RoleTypes.DfeAdmin, -1);
            var organisations = GetTestOrganisations();

            _mockServiceDirectoryClient.Setup(x => x.GetOrganisations(It.IsAny<CancellationToken>())).Returns(Task.FromResult(organisations));

            var sut = new ManageOrganisationsModel(_mockServiceDirectoryClient.Object, _mockCacheService.Object)
            {
                PageContext = { HttpContext = mockHttpContext.Object }
            };

            //  Act
            await sut.OnGet(null, null);

            //  Assert
            Assert.Equal(3, sut.PaginatedOrganisations.Items.Count);
        }

        [Fact]
        public async Task OnGet_LaAdmin_SetsPaginatedList()
        {
            //  Arrange
            const long organisationId = 1;
            var mockHttpContext = GetHttpContext(RoleTypes.LaManager, organisationId);
            var organisations = GetTestOrganisations();

            _mockServiceDirectoryClient.Setup(x => x.GetOrganisationByAssociatedOrganisation(organisationId)).Returns(Task.FromResult(organisations));

            var sut = new ManageOrganisationsModel(_mockServiceDirectoryClient.Object, _mockCacheService.Object)
            {
                PageContext = { HttpContext = mockHttpContext.Object }
            };

            //  Act
            await sut.OnGet(null, null);

            //  Assert
            Assert.Equal(3, sut.PaginatedOrganisations.Items.Count);
        }

        private Mock<HttpContext> GetHttpContext(string role, long organisationId)
        {
            var claims = new List<Claim> {
                new Claim(FamilyHubsClaimTypes.FullName, "any") ,
                new Claim(FamilyHubsClaimTypes.AccountId, "1"),
                new Claim(FamilyHubsClaimTypes.OrganisationId, organisationId.ToString()),
                new Claim(FamilyHubsClaimTypes.Role, role),
            };

            return TestHelper.GetHttpContext(claims);
        }

        private List<OrganisationDto> GetTestOrganisations()
        {
            var organisations = new List<OrganisationDto>();

            organisations.Add(TestHelper.CreateTestOrganisation(1, null, OrganisationType.LA, _fixture));
            organisations.Add(TestHelper.CreateTestOrganisation(2, 1, OrganisationType.VCFS, _fixture));
            organisations.Add(TestHelper.CreateTestOrganisation(3, 1, OrganisationType.VCFS, _fixture));
            organisations.Add(TestHelper.CreateTestOrganisation(4, 1, OrganisationType.VCFS, _fixture));

            return organisations;
        }
    }
}
