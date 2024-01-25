namespace FamilyHubs.ServiceDirectory.Admin.Core.Models;

//todo: common base for Location and Service models

public class LocationModel : LocationModel<object>
{
}

public class LocationModel<T>
{
    public string? Description { get; set; }

    public LocationErrorState? ErrorState { get; set; }

    public string? UserInputType { get; set; }
    public T? UserInput { get; set; }
}