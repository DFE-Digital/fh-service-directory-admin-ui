using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.SharedKernel.Reports.WeeklyBreakdown;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace FamilyHubs.ServiceDirectory.Admin.Web.IntegrationTests.PerformanceData;

public class ConnectTest : BaseTest
{
    private readonly Mock<IReportingClient> _reportingClient = new();
    private readonly Mock<IServiceDirectoryClient> _serviceDirectoryClient = new();

    private const ServiceType TestServiceType = ServiceType.InformationSharing;

    protected override void Configure(IServiceCollection services)
    {
        services.AddSingleton(_reportingClient.Object);
        services.AddSingleton(_serviceDirectoryClient.Object);
    }

    [Fact]
    public async Task As_DfeAdmin_Then_Connect_Data_Should_Be_Correct()
    {
        var breakdown = new WeeklyReportBreakdown
        {
            WeeklyReports = new[]
            {
                new WeeklyReport { Date = "Week 1", SearchCount = Random.Next(0, 1000000) },
                new WeeklyReport { Date = "Week 2", SearchCount = Random.Next(0, 1000000) },
                new WeeklyReport { Date = "Week 3", SearchCount = Random.Next(0, 1000000) },
                new WeeklyReport { Date = "Week 4", SearchCount = Random.Next(0, 1000000) }
            },
            TotalSearchCount = Random.Next(0, 1000000)
        };

        _reportingClient
            .Setup(r => r.GetServicesSearches4WeekBreakdown(TestServiceType, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(breakdown);

        var searchCount = Random.Next(0, 1000000);
        _reportingClient
            .Setup(r => r.GetServicesSearchesTotal(TestServiceType, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchCount);
        var recentSearchCount = Random.Next(0, 1000000);
        _reportingClient
            .Setup(r => r.GetServicesSearchesPast7Days(TestServiceType, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recentSearchCount);

        // Login
        await Login(StubUser.DfeAdmin);

        // Act
        var page = await Navigate("performance-data/Connect");

        var searches = page.QuerySelector("[data-testid=\"overall-searches\"] td").TextContent;
        Assert.Equal(searchCount.ToString(), searches);

        var searchesLast7Days = page.QuerySelector("[data-testid=\"recent-searches\"] td").TextContent;
        Assert.Equal(recentSearchCount.ToString(), searchesLast7Days);

        foreach (var (report, idx) in breakdown.WeeklyReports.Reverse().Select((report, idx) => (report, idx)))
        {
            var heading = page.QuerySelector($"[data-testid=\"breakdown-week{idx + 1}\"] th").TextContent;
            Assert.Equal(report.Date, heading);

            var text = page.QuerySelector($"[data-testid=\"breakdown-week{idx + 1}\"] td").TextContent;
            Assert.Equal(report.SearchCount.ToString(), text);
        }

        var total = page.QuerySelector($"[data-testid=\"breakdown-total\"] td").TextContent;
        Assert.Equal(breakdown.TotalSearchCount.ToString(), total);
    }

    [Fact]
    public async Task As_LaManager_Then_Connect_Data_Should_Be_Correct()
    {
        _serviceDirectoryClient
            .Setup(sd => sd.GetOrganisationById(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganisationDetailsDto
            {
                Name = "Test Org",
                OrganisationType = OrganisationType.LA,
                Description = "Test Description",
                AdminAreaCode = "D123"
            });

        var breakdown = new WeeklyReportBreakdown
        {
            WeeklyReports = new[]
            {
                new WeeklyReport { Date = "Week 1", SearchCount = Random.Next(0, 1000000) },
                new WeeklyReport { Date = "Week 2", SearchCount = Random.Next(0, 1000000) },
                new WeeklyReport { Date = "Week 3", SearchCount = Random.Next(0, 1000000) },
                new WeeklyReport { Date = "Week 4", SearchCount = Random.Next(0, 1000000) }
            },
            TotalSearchCount = Random.Next(0, 1000000)
        };

        _reportingClient
            .Setup(r => r.GetServicesSearches4WeekBreakdown(TestServiceType, It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(breakdown);

        var searchCount = Random.Next(0, 1000000);
        _reportingClient
            .Setup(r => r.GetServicesSearchesTotal(TestServiceType, It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchCount);
        var recentSearchCount = Random.Next(0, 1000000);
        _reportingClient
            .Setup(r => r.GetServicesSearchesPast7Days(TestServiceType, It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(recentSearchCount);

        // Login
        await Login(StubUser.LaAdmin);

        // Act
        var page = await Navigate("performance-data/Connect");

        var searches = page.QuerySelector("[data-testid=\"overall-searches\"] td").TextContent;
        Assert.Equal(searchCount.ToString(), searches);

        var searchesLast7Days = page.QuerySelector("[data-testid=\"recent-searches\"] td").TextContent;
        Assert.Equal(recentSearchCount.ToString(), searchesLast7Days);

        foreach (var (report, idx) in breakdown.WeeklyReports.Reverse().Select((report, idx) => (report, idx)))
        {
            var heading = page.QuerySelector($"[data-testid=\"breakdown-week{idx + 1}\"] th").TextContent;
            Assert.Equal(report.Date, heading);

            var text = page.QuerySelector($"[data-testid=\"breakdown-week{idx + 1}\"] td").TextContent;
            Assert.Equal(report.SearchCount.ToString(), text);
        }

        var total = page.QuerySelector($"[data-testid=\"breakdown-total\"] td").TextContent;
        Assert.Equal(breakdown.TotalSearchCount.ToString(), total);
    }
}
