
namespace FamilyHubs.ServiceDirectory.Admin.Core.Models;

public enum JourneyFlow
{
    Add,
    AddRedo,
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
        ArgumentNullException.ThrowIfNullOrEmpty(urlString);

        if (!Enum.TryParse(urlString, true, out JourneyFlow flow))
        {
            //todo: throw here, or let consumer handle it?
            throw new InvalidOperationException($"Invalid {nameof(JourneyFlow)} string representation: {urlString}");
        }

        return flow;
    }
}