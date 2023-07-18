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
    }
}
