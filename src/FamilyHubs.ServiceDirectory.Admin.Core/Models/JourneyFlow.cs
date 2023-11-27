
namespace FamilyHubs.ServiceDirectory.Admin.Core.Models;

public enum JourneyFlow
{
    /// <summary>
    /// Adding a service (creating from scratch), retrieving from and setting in the cache.
    /// </summary>
    Add,
    /// <summary>
    /// Redoing when adding a service.
    /// </summary>
    AddRedo,
    /// <summary>
    /// Editing a service, retrieving from and setting in the API.
    /// </summary>
    Edit
}

//todo: move to web, or keep together?
public static class JourneyFlowExtensions
{
    public static string ToUrlString(this JourneyFlow flow)
    {
        return flow.ToString().ToLowerInvariant();
    }

    public static JourneyFlow FromUrlString(string? urlString)
    {
        //todo: have a default for when flow is not specified?
        ArgumentNullException.ThrowIfNullOrEmpty(urlString);

        if (!Enum.TryParse(urlString, true, out JourneyFlow flow))
        {
            //todo: throw here, or let consumer handle it?
            throw new InvalidOperationException($"Invalid {nameof(JourneyFlow)} string representation: {urlString}");
        }

        return flow;
    }
}