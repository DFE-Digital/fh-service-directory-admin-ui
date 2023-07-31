namespace FamilyHubs.ServiceDirectory.Admin.Core.Models;

public class ConnectionRequestModel
{
    public string ServiceName { get; set; } = default!;
    public List<string>? TaxonomySelection { get; set; } = default!;
}
