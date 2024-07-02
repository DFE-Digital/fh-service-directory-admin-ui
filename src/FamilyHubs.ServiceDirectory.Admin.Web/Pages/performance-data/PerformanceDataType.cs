namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.performance_data;

public class PerformanceDataType
{
    public string Name { get; private init; }
    public string TestId { get; private init; }
    private PerformanceDataType(string name, string testId)
    {
        Name = name;
        TestId = testId;
    }

    public static readonly PerformanceDataType LocalServices = new("Local authority services", "la-svc");
    public static readonly PerformanceDataType VcsServices = new("VCS services", "vcs-svc");
    public static readonly PerformanceDataType VcsOrganisations = new("VCS organisations", "vcs-org");

    public static readonly PerformanceDataType SearchesTotal = new("Searches", "searches");
    public static readonly PerformanceDataType SearchesLast7Days = new("Searches in the last 7 days", "recent-searches");

    public static readonly PerformanceDataType ConnectionRequests = new("Connection requests made", "requests-made");
    public static readonly PerformanceDataType ConnectionAccepted = new("Connection requests accepted", "requests-accepted");
    public static readonly PerformanceDataType ConnectionDeclined = new("Connection requests declined", "requests-declined");
}