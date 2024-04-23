
namespace FamilyHubs.ServiceDirectory.Admin.Core.Models;

public enum LocationJourneyFlow
{
    /// <summary>
    /// Adding a service (creating from scratch), retrieving from and setting in the cache.
    /// </summary>
    Add,
    /// <summary>
    /// Redoing when adding a service.
    /// </summary>
    AddRedo,
    //AddRedoLocation,
    //AddRedoHowUse,
    /// <summary>
    /// Editing a service, retrieving from and setting in the API.
    /// </summary>
    Edit
}

public enum ServiceJourneyFlow
{
    /// <summary>
    /// Adding a service (creating from scratch), retrieving from and setting in the cache.
    /// </summary>
    Add,
    /// <summary>
    /// Redoing when adding a service.
    /// </summary>
    //AddRedo,
    //AddRedoLocation,
    //AddRedoHowUse,
    /// <summary>
    /// Editing a service, retrieving from and setting in the API.
    /// </summary>
    Edit
}

public enum ServiceJourneyChangeFlow
{
    SinglePage,
    Location,
    HowUse
}