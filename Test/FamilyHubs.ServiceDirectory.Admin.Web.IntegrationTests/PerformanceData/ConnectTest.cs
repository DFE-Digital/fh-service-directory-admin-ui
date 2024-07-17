using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.SharedKernel.Reports.ConnectionRequests;
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

    private void SetupClient(
        WeeklyReportBreakdown breakdown, int searchCount, int recentSearchCount,
        ConnectionRequestsBreakdown crBreakdown, ConnectionRequests crMetric, ConnectionRequests recentCrMetric
    ) {
        _reportingClient
            .Setup(r => r.GetServicesSearches4WeekBreakdown(TestServiceType, It.IsAny<long?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(breakdown);
        _reportingClient
            .Setup(r => r.GetServicesSearchesTotal(TestServiceType, It.IsAny<long?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchCount);
        _reportingClient
            .Setup(r => r.GetServicesSearchesPast7Days(TestServiceType, It.IsAny<long?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(recentSearchCount);

        _reportingClient
            .Setup(r => r.GetConnectionRequests4WeekBreakdown(TestServiceType, It.IsAny<long?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(crBreakdown);
        _reportingClient
            .Setup(r => r.GetConnectionRequestsTotal(TestServiceType, It.IsAny<long?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(crMetric);
        _reportingClient
            .Setup(r => r.GetConnectionRequestsPast7Days(TestServiceType, It.IsAny<long?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(recentCrMetric);
    }

    private ConnectionRequests GenCrMetric() => GenCrDated("");

    private ConnectionRequestsDated GenCrDated(string date) =>
        new()
        {
            Date = date, Accepted = Random.Next(0, 1000000), Declined = Random.Next(0, 1000000), Made = Random.Next(0, 1000000)
        };

    private void AssertPageContent(IHtmlDocument page, string selector, object? expected)
    {
        Assert.Equal(expected?.ToString(), page.QuerySelector(selector)?.TextContent);
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
        var searchCount = Random.Next(0, 1000000);
        var recentSearchCount = Random.Next(0, 1000000);

        var crBreakdown = new ConnectionRequestsBreakdown
        {
            WeeklyReports = new[]
            {
                GenCrDated("Week 1"),
                GenCrDated("Week 2"),
                GenCrDated("Week 3"),
                GenCrDated("Week 4")
            },
            Totals = GenCrMetric()
        };
        var crMetric = GenCrMetric();
        var recentCrMetric = GenCrMetric();

        SetupClient(breakdown, searchCount, recentSearchCount, crBreakdown, crMetric, recentCrMetric);

        // Login
        await Login(StubUser.DfeAdmin);

        // Act
        var page = await Navigate("performance-data/Connect");

        AssertPageContent(page, "[data-testid=\"overall-searches\"] td", searchCount);
        AssertPageContent(page, "[data-testid=\"overall-requests-sent\"] td", crMetric.Made);
        AssertPageContent(page, "[data-testid=\"overall-requests-accepted\"] td", crMetric.Accepted);

        AssertPageContent(page, "[data-testid=\"recent-searches\"] td", recentSearchCount);
        AssertPageContent(page, "[data-testid=\"recent-requests-sent\"] td", recentCrMetric.Made);
        AssertPageContent(page, "[data-testid=\"recent-requests-accepted\"] td", recentCrMetric.Accepted);

        foreach (var (report, idx) in breakdown.WeeklyReports.Reverse().Select((report, idx) => (report, idx)))
        {
            var crReport = crBreakdown.WeeklyReports.FirstOrDefault(x => x.Date == report.Date);
            AssertPageContent(page, $"[data-testid=\"breakdown-week{idx + 1}\"] th", report.Date);
            AssertPageContent(page, $"[data-testid=\"breakdown-week{idx + 1}\"] td:nth-child(2)", report.SearchCount);
            AssertPageContent(page, $"[data-testid=\"breakdown-week{idx + 1}\"] td:nth-child(3)", crReport?.Made);
            AssertPageContent(page, $"[data-testid=\"breakdown-week{idx + 1}\"] td:nth-child(4)", crReport?.Accepted);
        }

        AssertPageContent(page, "[data-testid=\"breakdown-total\"] td:nth-child(2)", breakdown.TotalSearchCount);
        AssertPageContent(page, "[data-testid=\"breakdown-total\"] td:nth-child(3)", crBreakdown.Totals.Made);
        AssertPageContent(page, "[data-testid=\"breakdown-total\"] td:nth-child(4)", crBreakdown.Totals.Accepted);
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
        var searchCount = Random.Next(0, 1000000);
        var recentSearchCount = Random.Next(0, 1000000);

        var crBreakdown = new ConnectionRequestsBreakdown
        {
            WeeklyReports = new[]
            {
                GenCrDated("Week 1"),
                GenCrDated("Week 2"),
                GenCrDated("Week 3"),
                GenCrDated("Week 4")
            },
            Totals = GenCrMetric()
        };
        var crMetric = GenCrMetric();
        var recentCrMetric = GenCrMetric();

        SetupClient(breakdown, searchCount, recentSearchCount, crBreakdown, crMetric, recentCrMetric);

        // Login
        await Login(StubUser.LaAdmin);

        // Act
        var page = await Navigate("performance-data/Connect");
        
        AssertPageContent(page, "[data-testid=\"overall-searches\"] td", searchCount);
        AssertPageContent(page, "[data-testid=\"overall-requests-sent\"] td", crMetric.Made);
        AssertPageContent(page, "[data-testid=\"overall-requests-accepted\"] td", crMetric.Accepted);

        AssertPageContent(page, "[data-testid=\"recent-searches\"] td", recentSearchCount);
        AssertPageContent(page, "[data-testid=\"recent-requests-sent\"] td", recentCrMetric.Made);
        AssertPageContent(page, "[data-testid=\"recent-requests-accepted\"] td", recentCrMetric.Accepted);

        foreach (var (report, idx) in breakdown.WeeklyReports.Reverse().Select((report, idx) => (report, idx)))
        {
            var crReport = crBreakdown.WeeklyReports.FirstOrDefault(x => x.Date == report.Date);
            AssertPageContent(page, $"[data-testid=\"breakdown-week{idx + 1}\"] th", report.Date);
            AssertPageContent(page, $"[data-testid=\"breakdown-week{idx + 1}\"] td:nth-child(2)", report.SearchCount);
            AssertPageContent(page, $"[data-testid=\"breakdown-week{idx + 1}\"] td:nth-child(3)", crReport?.Made);
            AssertPageContent(page, $"[data-testid=\"breakdown-week{idx + 1}\"] td:nth-child(4)", crReport?.Accepted);
        }

        
        AssertPageContent(page, "[data-testid=\"breakdown-total\"] td:nth-child(2)", breakdown.TotalSearchCount);
        AssertPageContent(page, "[data-testid=\"breakdown-total\"] td:nth-child(3)", crBreakdown.Totals.Made);
        AssertPageContent(page, "[data-testid=\"breakdown-total\"] td:nth-child(4)", crBreakdown.Totals.Accepted);
    }
}
