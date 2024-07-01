namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.performance_data;

public class ReportingNavigationDataModel
{
    public enum Page
    {
        Find, Connect, Manage
    }

    public bool IsDfeAdmin { get; set; }
    public Page ActivePage { get; set; }
}