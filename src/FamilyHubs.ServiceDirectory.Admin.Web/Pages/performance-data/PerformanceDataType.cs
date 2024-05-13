namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.performance_data;

public class PerformanceDataType
{
    public string Name { get; private set; }
    private PerformanceDataType(string name)
    {
        Name = name;
    }

    public static readonly PerformanceDataType LocalServices = new("Local authority services");
    public static readonly PerformanceDataType VcsServices = new("VCS services");
    public static readonly PerformanceDataType VcsOrganisations = new("VCS organisations");

    public static readonly PerformanceDataType SearchesTotal = new("Searches");
    public static readonly PerformanceDataType SearchesLast7Days = new("Searches in the last 7 days");

    public static readonly PerformanceDataType ConnectionRequests = new("Connection requests made");
    public static readonly PerformanceDataType ConnectionAccepted = new("Connection requests accepted");
    public static readonly PerformanceDataType ConnectionDeclined = new("Connection requests declined");
}