using AutoFixture;
using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Web.Areas.VcsAdmin.Pages;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests.Areas.VcsAdmin
{
    public class ManageOrganisationsTests
    {
        private readonly Mock<IServiceDirectoryClient> _mockServiceDirectoryClient;
        private readonly Fixture _fixture;


        public ManageOrganisationsTests()
        {
            _mockServiceDirectoryClient = new Mock<IServiceDirectoryClient>();
            _fixture = new Fixture();
        }

        [Fact]
        public async Task OnGet_DfeAdmin_SetsPaginatedList()
        {
            //  Arrange
            var mockHttpContext = GetHttpContext(RoleTypes.DfeAdmin, -1);
            var organisations = GetTestOrganisations();

            _mockServiceDirectoryClient.Setup(x => x.GetOrganisations(It.IsAny<CancellationToken>())).Returns(Task.FromResult(organisations));

            var sut = new ManageOrganisationsModel(_mockServiceDirectoryClient.Object)
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

            var sut = new ManageOrganisationsModel(_mockServiceDirectoryClient.Object)
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

            organisations.Add(CreateTestOrganisation(1, null, OrganisationType.LA));
            organisations.Add(CreateTestOrganisation(2, 1, OrganisationType.VCFS));
            organisations.Add(CreateTestOrganisation(3, 1, OrganisationType.VCFS));
            organisations.Add(CreateTestOrganisation(4, 1, OrganisationType.VCFS));

            return organisations;
        }

        private OrganisationDto CreateTestOrganisation(long id, long? parentId, OrganisationType organisationType)
        {
            var organisation = _fixture.Create<OrganisationDto>();
            organisation.Id = id;
            organisation.AssociatedOrganisationId = parentId;
            organisation.OrganisationType = organisationType;
            return organisation;
        }
    }
}
