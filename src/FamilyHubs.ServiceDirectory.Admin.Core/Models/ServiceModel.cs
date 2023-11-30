
namespace FamilyHubs.ServiceDirectory.Admin.Core.Models;

public class ServiceModel
{
    public string? Name { get; set; }
    public int? MinimumAge { get; set; }
    public int? MaximumAge { get; set; }

    public ServiceErrorState<object>? ErrorState { get; set; }
}