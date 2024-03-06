using AutoFixture;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Collections.Generic;
using System.Security.Claims;

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests
{
    internal static class TestHelper
    {
        public static Mock<HttpContext> GetHttpContext(List<Claim> claims)
        {
            var mockUser = new Mock<ClaimsPrincipal>();
            mockUser.SetupGet(x=>x.Claims).Returns(claims);

            var mockCookies = new Mock<IResponseCookies>();

            var mockResponse = new Mock<HttpResponse>();
            mockResponse.SetupGet(x=>x.Cookies).Returns(mockCookies.Object);

            var mock = new Mock<HttpContext>();
            mock.SetupGet(x => x.User).Returns(mockUser.Object);
            mock.SetupGet(x=>x.Response).Returns(mockResponse.Object);

            return mock;
        }

        public static OrganisationDto CreateTestOrganisation(long id, long? parentId, OrganisationType organisationType, Fixture fixture)
        {
            var organisation = fixture.Create<OrganisationDto>();
            organisation.Id = id;
            organisation.AssociatedOrganisationId = parentId;
            organisation.OrganisationType = organisationType;
            return organisation;
        }

        public static OrganisationDetailsDto CreateTestOrganisationWithServices(long id, long? parentId, OrganisationType organisationType, Fixture fixture)
        {
            var organisation = fixture.Create<OrganisationDetailsDto>();
            organisation.Id = id;
            organisation.AssociatedOrganisationId = parentId;
            organisation.OrganisationType = organisationType;
            return organisation;
        }
    }
}
