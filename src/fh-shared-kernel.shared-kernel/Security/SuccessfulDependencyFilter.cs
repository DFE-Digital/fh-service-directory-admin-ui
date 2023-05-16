using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace FamilyHubs.SharedKernel.Security;

/*
 Taken From: https://learn.microsoft.com/en-us/azure/azure-monitor/app/api-filtering-sampling

 Usage:
        services.AddApplicationInsightsTelemetry();
        services.AddApplicationInsightsTelemetryProcessor<SuccessfulDependencyFilter>(); <-- Add this line
*/

public class SuccessfulDependencyFilter : ITelemetryProcessor
{
    private ITelemetryProcessor Next { get; set; }

    // next will point to the next TelemetryProcessor in the chain.
    public SuccessfulDependencyFilter(ITelemetryProcessor next)
    {
        this.Next = next;
    }

    public void Process(ITelemetry item)
    {
        // To filter out an item, return without calling the next processor.
        if (!OKtoSend(item)) { return; }

        this.Next.Process(item);
    }

    // Example: replace with your own criteria.
    private bool OKtoSend(ITelemetry item)
    {
        if (item != null)
        {
            var traceTelemetry = item as TraceTelemetry;
            if (traceTelemetry != null && traceTelemetry.Message.Contains("api.postcodes.io"))
            {
                return false;
            }

            var dependency = item as DependencyTelemetry;
            if (dependency != null && dependency.Data.Contains("api.postcodes.io"))
            {
                return false;
            }
        }

        return true;
    }
}
