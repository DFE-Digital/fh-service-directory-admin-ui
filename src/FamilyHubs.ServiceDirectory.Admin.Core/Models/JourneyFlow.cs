
namespace FamilyHubs.ServiceDirectory.Admin.Core.Models;

public enum JourneyFlow
{
    /// <summary>
    /// Adding an entity.
    /// </summary>
    Add,
    /// <summary>
    /// Editing an existing entity.
    /// </summary>
    Edit
}

public enum ServiceJourneyChangeFlow
{
    SinglePage,
    Location,
    HowUse
}

//todo: could have this as the simple generic version
public enum LocationJourneyChangeFlow
{
    SinglePage
}
