
namespace FamilyHubs.ServiceDirectory.Admin.Core.Models;

public class ServiceModel : ServiceModel<object>
{
}

public class ServiceModel<T>
{
    public string? Name { get; set; }
    public int? MinimumAge { get; set; }
    public int? MaximumAge { get; set; }

    public ServiceErrorState? ErrorState { get; set; }

    public string? UserInputType { get; set; }
    public T? UserInput { get; set; }

    public List<long?> SelectedCategories { get; set; } = new();
    public List<long> SelectedSubCategories { get; set; } = new();
}