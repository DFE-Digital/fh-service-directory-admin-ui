using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace FamilyHubs.ServiceDirectory.Admin.Web.IntegrationTests;

public class PerformanceDataTest : BaseTest
{
    private readonly Mock<IReportingClient> _reportingClient = new();

    public PerformanceDataTest(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    protected override void Configure(IServiceCollection services)
    {
        services.AddSingleton(_reportingClient.Object);
    }

    [Fact]
    public async Task Then_Find_Data_Should_Be_Correct()
    {
        var breakdown = new WeeklyReportBreakdownDto
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
            .Setup(r => r.GetServicesSearches4WeekBreakdown(ServiceSearchType.Find, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(breakdown);

        var searchCount = Random.Next(0, 1000000);
        _reportingClient
            .Setup(r => r.GetServicesSearchesTotal(ServiceSearchType.Find, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchCount);
        var recentSearchCount = Random.Next(0, 1000000);
        _reportingClient
            .Setup(r => r.GetServicesSearchesPast7Days(ServiceSearchType.Find, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recentSearchCount);

        // Login
        await Login(StubUser.DfeAdmin);

        // Act
        var page = await Navigate("performance-data/Find");

        var searches = page.QuerySelector("[data-testid=\"searches\"] td").TextContent;
        Assert.Equal(searchCount.ToString(), searches);

        var searchesLast7Days = page.QuerySelector("[data-testid=\"recent-searches\"] td").TextContent;
        Assert.Equal(recentSearchCount.ToString(), searchesLast7Days);

        foreach (var (report, idx) in breakdown.WeeklyReports.Reverse().Select((report, idx) => (report, idx)))
        {
            var heading = page.QuerySelector($"[data-testid=\"searches-week{idx + 1}\"] th").TextContent;
            Assert.Equal(report.Date, heading);

            var text = page.QuerySelector($"[data-testid=\"searches-week{idx + 1}\"] td").TextContent;
            Assert.Equal(report.SearchCount.ToString(), text);
        }

        var total = page.QuerySelector($"[data-testid=\"searches-total\"] td").TextContent;
        Assert.Equal(breakdown.TotalSearchCount.ToString(), total);
    }

    [Fact]
    public async Task Then_Connect_Data_Should_Be_Correct()
    {
        var breakdown = new WeeklyReportBreakdownDto
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
            .Setup(r => r.GetServicesSearches4WeekBreakdown(ServiceSearchType.Connect, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(breakdown);

        var searchCount = Random.Next(0, 1000000);
        _reportingClient
            .Setup(r => r.GetServicesSearchesTotal(ServiceSearchType.Connect, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchCount);
        var recentSearchCount = Random.Next(0, 1000000);
        _reportingClient
            .Setup(r => r.GetServicesSearchesPast7Days(ServiceSearchType.Connect, null, It.IsAny<CancellationToken>()))
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
}
