
namespace FamilyHubs.ServiceDirectory.Admin.Core.Models;

//public class ServiceModel<T> : ServiceModel
//{
//    public new T? UserInput { get; set; }
//}

//todo: derived versiom defaulting to object

public class ServiceModel<T>
{
    public string? Name { get; set; }
    public int? MinimumAge { get; set; }
    public int? MaximumAge { get; set; }

    public ServiceErrorState? ErrorState { get; set; }

    public string? UserInputType { get; set; }
    public T? UserInput { get; set; }
}