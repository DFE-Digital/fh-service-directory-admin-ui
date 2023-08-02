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
    public class AddOrganisationWhichLocalAuthorityTests
    {
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Mock<IServiceDirectoryClient> _serviceDirectoryClient;
        private readonly Fixture _fixture;
        private const string ValidLocalAuthority = "ValidLocalAuthority";
        private const long ValidLocalAuthorityId = 1234;

        public AddOrganisationWhichLocalAuthorityTests()
        {
            _fixture = new Fixture();
            var organisations = _fixture.Create<List<OrganisationDto>>();

            organisations[0].Id = ValidLocalAuthorityId;
            organisations[0].Name = ValidLocalAuthority;
            for (var i = 1; i < organisations.Count; i++)
            {
                organisations[i].Id = i;
            }


            _mockCacheService = new Mock<ICacheService>();
            _mockCacheService.Setup(m => m.GetOrganisations()).ReturnsAsync(organisations);

            _serviceDirectoryClient = new Mock<IServiceDirectoryClient>();
            _serviceDirectoryClient.Setup(x => x.GetCachedLaOrganisations(CancellationToken.None))
                .ReturnsAsync(new List<OrganisationDto>(new[] { new OrganisationDto
                    {
                        OrganisationType = OrganisationType.LA,
                        Name = ValidLocalAuthority,
                        Description = "Test",
                        AdminAreaCode = "Test",
                        Id = ValidLocalAuthorityId
                    }
                }));
        }


        [Fact]
        public async Task OnGet_LaOrganisationName_Set()
        {
            //  Arrange
            var mockHttpContext = GetHttpContext(RoleTypes.DfeAdmin, -1);
            _mockCacheService.Setup(m => m.RetrieveString(CacheKeyNames.LaOrganisationId)).ReturnsAsync(ValidLocalAuthorityId.ToString());
            var sut = new AddOrganisationWhichLocalAuthorityModel(_mockCacheService.Object, _serviceDirectoryClient.Object)
            {
                LaOrganisationName = string.Empty,
                LocalAuthorities = new List<string>(),
                PageContext = { HttpContext = mockHttpContext.Object }
            };

            //  Act
            await sut.OnGet();

            //  Assert
            Assert.Equal(ValidLocalAuthority, sut.LaOrganisationName);
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
    }
}
