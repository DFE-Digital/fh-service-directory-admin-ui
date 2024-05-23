using FamilyHubs.ServiceDirectory.Admin.Core.Models;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Journeys;

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

    public static JourneyFlow? FromOptionalUrlString(string? urlString)
    {
        if (!Enum.TryParse(urlString, true, out JourneyFlow flow))
        {
            return null;
        }

        return flow;
    }
}