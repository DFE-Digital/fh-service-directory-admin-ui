using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.SharedKernel.Identity;
using Moq;
using Xunit;
using FamilyHubs.ServiceDirectory.Shared.Models;
using FamilyHubs.SharedKernel.Razor.Dashboard;
using System.Security.Claims;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests.Manage;

//todo: need more unit tests (that don't test multiple things), but this (mostly) generated test does adds value
public class ServicesModelTests
{
    private readonly Mock<IServiceDirectoryClient> _serviceDirectoryClientMock = new();

    [Fact]
    public async Task OnGetAsync_WithValidUser_ReturnsExpectedValues()
    {
        // Arrange
        var services = new PaginatedList<ServiceNameDto>(new List<ServiceNameDto>
        {
            new() {Id = 111, Name = "Name 111"},
            new() {Id = 222, Name = "Name 222"},

        }, 2, 1, 1);

        _serviceDirectoryClientMock.Setup(x => x.GetServiceSummaries(null, null, 1, 10, SortOrder.ascending, CancellationToken.None))
            .ReturnsAsync(services);

        var claims = new List<Claim>
        {
            new(FamilyHubsClaimTypes.Role, RoleTypes.DfeAdmin),
            new(FamilyHubsClaimTypes.OrganisationId, "999")
        };
        var mockHttpContext = TestHelper.GetHttpContext(claims);

        var model = new ServicesModel(_serviceDirectoryClientMock.Object)
        {
            PageContext = { HttpContext = mockHttpContext.Object }
        };

        // Act
        await model.OnGetAsync(CancellationToken.None, null, null, SortOrder.ascending, 1);

        // Assert
        Assert.Equal("Services", model.Title);
        Assert.Equal(" for Local Authorities and VCS organisations", model.OrganisationTypeContent);
        Assert.Equal(2, model.Rows.Count());
    }
}