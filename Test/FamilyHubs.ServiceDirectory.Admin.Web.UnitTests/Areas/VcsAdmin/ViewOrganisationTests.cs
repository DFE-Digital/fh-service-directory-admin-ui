using AutoFixture;
using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.Areas.VcsAdmin.Pages;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.ServiceDirectory.Shared.Models;
using FamilyHubs.SharedKernel.Identity;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
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
    public class ViewOrganisationTests
    {
        private readonly Mock<IServiceDirectoryClient> _mockServiceDirectoryClient;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Mock<ILogger<ViewOrganisationModel>> _mockLogger;
        private readonly Fixture _fixture;

        public ViewOrganisationTests()
        {
            _mockServiceDirectoryClient = new Mock<IServiceDirectoryClient>();
            _mockCacheService= new Mock<ICacheService>();
            _mockLogger = new Mock<ILogger<ViewOrganisationModel>>();
            _fixture = new Fixture();
            ConfigureMockServiceClient();
        }

        [Fact]
        public async Task OnGet_ReturnsPage()
        {
            //  Arrange
            var mockHttpContext = GetHttpContext(RoleTypes.DfeAdmin, -1);
            var sut = new ViewOrganisationModel(_mockServiceDirectoryClient.Object, _mockCacheService.Object, _mockLogger.Object)
            {
                PageContext = { HttpContext = mockHttpContext.Object },
                OrganisationId = "2"
            };

            //  Act
            var response = await sut.OnGet();

            //  Assert
            Assert.IsType<PageResult>(response);
        }

        [Theory]
        [InlineData("5", "Organisation 5 not found")]
        [InlineData("1", "Organisation 1 is not a VCS organisation")]
        [InlineData("3", "Organisation 3 has no parent")]
        [InlineData("4", "User testuser@test.com cannot view 4")]
        public async Task OnGet_InvalidOrganisation_RedirectsToError(string organisationId, string expectedLogMessage)
        {
            //  Arrange
            var mockHttpContext = GetHttpContext(RoleTypes.LaManager, 1);
            var sut = new ViewOrganisationModel(_mockServiceDirectoryClient.Object, _mockCacheService.Object, _mockLogger.Object)
            {
                PageContext = { HttpContext = mockHttpContext.Object },
                OrganisationId = organisationId
            };

            //  Act
            var response = await sut.OnGet();

            //  Assert
            Assert.IsType<RedirectToPageResult>(response);
            AssertLoggerWarning(expectedLogMessage);
        }

        [Fact]
        public async Task OnPost_UpdatesOrganisation()
        {
            //  Arrange
            const long organisationId = 2;
            const string updatedName = "updatedName";
            _mockCacheService.Setup(x => x.RetrieveString(CacheKeyNames.UpdateOrganisationName)).Returns(Task.FromResult(updatedName));
            _mockServiceDirectoryClient.Setup(x => x.UpdateOrganisation(It.IsAny<OrganisationDetailsDto>())).ReturnsAsync(organisationId);
            var mockHttpContext = GetHttpContext(RoleTypes.DfeAdmin, -1);
            var sut = new ViewOrganisationModel(_mockServiceDirectoryClient.Object, _mockCacheService.Object, _mockLogger.Object)
            {
                PageContext = { HttpContext = mockHttpContext.Object },
                OrganisationId = organisationId.ToString()
            };

            //  Act
            await sut.OnPost();

            //  Assert
            var arg = new ArgumentCaptor<OrganisationDetailsDto>();
            _mockServiceDirectoryClient.Verify(x=>x.UpdateOrganisation(arg!.Capture()));
            ArgumentNullException.ThrowIfNull(arg.Value);
            arg.Value.Name.Should().Be(updatedName);
        }

        private Mock<HttpContext> GetHttpContext(string role, long organisationId)
        {
            var claims = new List<Claim> {
                new Claim(ClaimTypes.Email, "testuser@test.com") ,
                new Claim(FamilyHubsClaimTypes.FullName, "any") ,
                new Claim(FamilyHubsClaimTypes.AccountId, "1"),
                new Claim(FamilyHubsClaimTypes.OrganisationId, organisationId.ToString()),
                new Claim(FamilyHubsClaimTypes.Role, role),
            };

            return TestHelper.GetHttpContext(claims);
        }

        private void ConfigureMockServiceClient()
        {
            var la = TestHelper.CreateTestOrganisationWithServices(1, null, OrganisationType.LA, _fixture);
            var laResponse = Task.FromResult((OrganisationDetailsDto?)la);

            var vcs = TestHelper.CreateTestOrganisationWithServices(2, 1, OrganisationType.VCFS, _fixture);
            var vcsResponse = Task.FromResult((OrganisationDetailsDto?)vcs);

            var vcsNoParent = TestHelper.CreateTestOrganisationWithServices(3, null, OrganisationType.VCFS, _fixture);
            var vcsNoParentResponse = Task.FromResult((OrganisationDetailsDto?)vcsNoParent);

            var vcsUnauthorisedUser = TestHelper.CreateTestOrganisationWithServices(4, 99, OrganisationType.VCFS, _fixture);
            var vcsUnauthorisedUserResponse = Task.FromResult((OrganisationDetailsDto?)vcsUnauthorisedUser);

            _mockServiceDirectoryClient.Setup(x => x.GetOrganisationById(It.Is<long>(x => x == 1), It.IsAny<CancellationToken>())).Returns(laResponse);
            _mockServiceDirectoryClient.Setup(x => x.GetOrganisationById(It.Is<long>(x => x == 2), It.IsAny<CancellationToken>())).Returns(vcsResponse);
            _mockServiceDirectoryClient.Setup(x => x.GetOrganisationById(It.Is<long>(x => x == 3), It.IsAny<CancellationToken>())).Returns(vcsNoParentResponse);
            _mockServiceDirectoryClient.Setup(x => x.GetOrganisationById(It.Is<long>(x => x == 4), It.IsAny<CancellationToken>())).Returns(vcsUnauthorisedUserResponse);
        }

        private void AssertLoggerWarning(string message)
        {
            _mockLogger.Verify(logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == message && @type.Name == "FormattedLogValues"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        }

        public class ArgumentCaptor<T>
        {
            public T Capture()
            {
                return It.Is<T>(t => SaveValue(t));
            }

            private bool SaveValue(T t)
            {
                Value = t;
                return true;
            }

            public T? Value { get; private set; }
        }
    }
}
