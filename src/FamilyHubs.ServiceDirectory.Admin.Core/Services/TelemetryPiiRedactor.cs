// define DEBUG_REDACTOR to enable debug checks for unredacted data
//#define DEBUG_REDACTOR

using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace FamilyHubs.ServiceDirectory.Admin.Core.Services;

/// <summary>
/// Redacts Personally Identifiable Information (PII) from telemetry data we send to App Insights.
/// What's redacted:
///     postcode: a postcode can map to a single residential address, so can be used to identify an individual
///     latitude & longitude: can be used to map to the postcode
///
/// Notes
/// * ExceptionTelemetry is left alone (no PII currently comes through ExceptionTelemetry).
///   We have to ensure we don't add any PII to exceptions.
/// * We don't redact the console log output as that doesn't get persisted anywhere.
/// </summary>
/// <remarks>
/// See
/// https://learn.microsoft.com/en-us/azure/azure-monitor/app/api-filtering-sampling
/// https://ico.org.uk/for-organisations/guide-to-data-protection/guide-to-the-general-data-protection-regulation-gdpr/key-definitions/what-is-personal-data/
/// </remarks>>
public class TelemetryPiiRedactor : ITelemetryInitializer
{
    // longtitude is due to the spelling error in the API. at some point, we should fix that (and all the consumers)
    private static readonly Regex SiteQueryStringRegex = new(@"(?<=(email|name)=)[^&]+", RegexOptions.Compiled);
    private static readonly Regex ApiQueryStringRegex = new(@"(?<=email=)([-+]?[0-9]*\.?[0-9]+)(?=&)|(?<=name=)([-+]?[0-9]*\.?[0-9]+)(?=&)");
    private static readonly Regex EmailRegex = new(@"(?<=email\/)[\w% ]+", RegexOptions.Compiled);
    private static readonly Regex NameRegex = new(@"(?<=email\/)[\w% ]+", RegexOptions.Compiled);
    private static readonly string[] TracePropertiesToRedact = { "Uri", "Scope", "QueryString", "HostingRequestStartingLog", "HostingRequestFinishedLog" };


    public void Initialize(ITelemetry telemetry)
    {
        switch (telemetry)
        {
            case DependencyTelemetry dependencyTelemetry:
                if (dependencyTelemetry.Name is "GET /api/services")
                {
                    // command name is obsolete and has been replaced by Data, but should contain the same as Data
#pragma warning disable CS0618
                    dependencyTelemetry.CommandName = dependencyTelemetry.Data =
                        Sanitize(ApiQueryStringRegex, dependencyTelemetry.Data);
#pragma warning restore CS0618
                }
                else if (dependencyTelemetry.Name.StartsWith("GET /api/AccountClaims/GetAccountClaimsByEmail/"))
                {
#pragma warning disable CS0618
                    dependencyTelemetry.CommandName =
                        dependencyTelemetry.Data = Sanitize(EmailRegex, dependencyTelemetry.Data);
                    dependencyTelemetry.CommandName =
                       dependencyTelemetry.Data = Sanitize(NameRegex, dependencyTelemetry.Data);
#pragma warning restore CS0618
                    dependencyTelemetry.Name = Sanitize(EmailRegex, dependencyTelemetry.Name);
                    dependencyTelemetry.Name = Sanitize(NameRegex, dependencyTelemetry.Name);
                    dependencyTelemetry.Data = Sanitize(ApiQueryStringRegex, dependencyTelemetry.Data);
                }
                else if (dependencyTelemetry.Name.StartsWith("GET /api/AccountClaims/GetAccountClaimsByEmail"))
                {
#pragma warning disable CS0618
                    dependencyTelemetry.CommandName =
                        dependencyTelemetry.Data = Sanitize(SiteQueryStringRegex, dependencyTelemetry.Data);
#pragma warning restore CS0618


                }
                else if (dependencyTelemetry.Name.StartsWith("DELETE /api/UserSession/DeleteAllUserSessions/"))
                {
                    if (dependencyTelemetry.Data.IndexOf("@") > -1)
                    {
                        var data = dependencyTelemetry.Data;
                        var redactedData = data.Substring(0, data.IndexOf("DeleteAllUserSessions/") + "DeleteAllUserSessions/".Length) + "REDACTED";
#pragma warning disable CS0618
                        dependencyTelemetry.CommandName =
                            dependencyTelemetry.Data = redactedData;
#pragma warning restore CS0618
                    }

                    if (dependencyTelemetry.Name.IndexOf("@") > -1)
                    {
                        var name = dependencyTelemetry.Name;
                        var redactedName = name.Substring(0, name.IndexOf("DeleteAllUserSessions/") + "DeleteAllUserSessions/".Length) + "REDACTED";
                        dependencyTelemetry.Name = redactedName;
                    }
                }
                else if (dependencyTelemetry.Name.StartsWith("GET /api/account"))
                {
#pragma warning disable CS0618
                    dependencyTelemetry.CommandName =
                        dependencyTelemetry.Data = Sanitize(SiteQueryStringRegex, dependencyTelemetry.Data);
#pragma warning restore CS0618
                }
                break;
            case TraceTelemetry traceTelemetry:
                if (traceTelemetry.Message.IndexOf("email") > -1)
                {
                    traceTelemetry.Message = Sanitize(EmailRegex, traceTelemetry.Message);
                    traceTelemetry.Message = Sanitize(NameRegex, traceTelemetry.Message);
                    traceTelemetry.Message = Sanitize(SiteQueryStringRegex, traceTelemetry.Message);
                }
                //todo consider further optimisation
                var list = traceTelemetry.Properties.Where(x => TracePropertiesToRedact.Contains(x.Key) && (x.Value.Contains("email")));
                if (list.Any())
                {
                    foreach (var key in list.Select(x => x.Key))
                    {
                        SanitizeProperty(SiteQueryStringRegex, traceTelemetry.Properties, key);
                        SanitizeProperty(EmailRegex, traceTelemetry.Properties, key);
                        SanitizeProperty(NameRegex, traceTelemetry.Properties, key);
                    }
                }
                var properties = traceTelemetry.Properties.Where(x => TracePropertiesToRedact.Contains(x.Key) && (x.Value.Contains("DeleteAllUserSessions")));
                if (properties.Any())
                {
                    foreach (var key in properties.Select(x => x.Key))
                    {
                        if (traceTelemetry.Properties.TryGetValue(key, out string? value)
                            && value.IndexOf("DeleteAllUserSessions") > -1)
                        {
                            var temp = value.Substring(0, value.IndexOf("DeleteAllUserSessions/") + "DeleteAllUserSessions/".Length) + "REDACTED";
                            traceTelemetry.Properties[key] = temp;
                        }
                    }
                }
                if ((traceTelemetry.Message.IndexOf("Sessions") > -1
                     || (traceTelemetry.Message.StartsWith("Executed DbCommand") && traceTelemetry.Message.IndexOf("[UserSessions]") > -1)
                     || traceTelemetry.Message.IndexOf("No session") > -1)
                    && traceTelemetry.Message.IndexOf("@") > -1)
                {
                    var message = traceTelemetry.Message;
                    var redactedMessage = message.Substring(0, message.IndexOf("DeleteAllUserSessions/") + "DeleteAllUserSessions/".Length) + "REDACTED" + message.Substring(message.IndexOf("@"), message.Length - message.IndexOf("@"));
                    traceTelemetry.Message = redactedMessage;
                }

                break;
            case RequestTelemetry requestTelemetry:
                if (requestTelemetry.Url.ToString().IndexOf("email") > -1 || requestTelemetry.Url.ToString().IndexOf("name") > -1)
                {
                    requestTelemetry.Url = Sanitize(SiteQueryStringRegex, requestTelemetry.Url);
                }
                if (requestTelemetry.Name.IndexOf("DeleteAllUserSessions") > -1)
                {
                    if (requestTelemetry.Name.IndexOf("@") > -1)
                    {
                        var name = requestTelemetry.Name;
                        var redactedName = name.Substring(0, name.IndexOf("DeleteAllUserSessions/") + "DeleteAllUserSessions/".Length) + "REDACTED";
                        requestTelemetry.Name = redactedName;
                    }

                    var url = requestTelemetry.Url.ToString();
                    var redactedUrl = url.Substring(0, url.IndexOf("DeleteAllUserSessions/") + "DeleteAllUserSessions/".Length) + "REDACTED";
                    requestTelemetry.Url = new Uri(redactedUrl);

                    var emailList = requestTelemetry.Properties.Where(x => x.Value.Contains("@"));
                    if (emailList.Any())
                    {
                        foreach (var key in emailList.Select(x => x.Key))
                        {
                            if (requestTelemetry.Properties.TryGetValue(key, out string? value)
                                && value.IndexOf("DeleteAllUserSessions") > -1)
                            {
                                var temp = value.Substring(0, value.IndexOf("DeleteAllUserSessions/") + "DeleteAllUserSessions/".Length) + "REDACTED";
                                requestTelemetry.Properties[key] = temp;
                            }
                        }
                    }
                }

                break;
        }

        DebugCheckForUnredactedData(telemetry);
    }

    private void SanitizeProperty(Regex regex, IDictionary<string, string> properties, string key)
    {
        if (properties.TryGetValue(key, out string? value))
        {
            properties[key] = Sanitize(regex, value);
        }
    }

    private string Sanitize(Regex regex, string value)
    {
        return regex.Replace(value, "REDACTED");
    }

    private Uri Sanitize(Regex regex, Uri uri)
    {
        // only create a uri if necessary (might be slower, but should stop memory churn)
        string unredactedUri = uri.ToString();
        string redactedUri = regex.Replace(unredactedUri, "REDACTED");
        return redactedUri != unredactedUri ? new Uri(redactedUri) : uri;
    }

    [Conditional("DEBUG_REDACTOR")]
    private void DebugCheckForUnredactedData(ITelemetry telemetry)
    {
        if (telemetry is AvailabilityTelemetry availabilityTelemetry)
        {
            DebugCheckForUnredactedData(availabilityTelemetry.Properties, availabilityTelemetry.Message);
        }
        if (telemetry is DependencyTelemetry dependencyTelemetry)
        {
#pragma warning disable CS0618
            DebugCheckForUnredactedData(dependencyTelemetry.Properties, dependencyTelemetry.Name, dependencyTelemetry.CommandName, dependencyTelemetry.Data);
#pragma warning restore CS0618
        }
        if (telemetry is EventTelemetry eventTelemetry)
        {
            DebugCheckForUnredactedData(eventTelemetry.Properties, eventTelemetry.Name);
        }
        if (telemetry is ExceptionTelemetry exceptionTelemetry)
        {
            DebugCheckForUnredactedData(exceptionTelemetry.Properties, exceptionTelemetry.Message, exceptionTelemetry.Exception?.Message);
        }
        if (telemetry is MetricTelemetry metricTelemetry)
        {
            DebugCheckForUnredactedData(metricTelemetry.Properties);
        }
        if (telemetry is PageViewPerformanceTelemetry pageViewPerformanceTelemetry)
        {
            DebugCheckForUnredactedData(pageViewPerformanceTelemetry.Properties);
        }
        if (telemetry is PageViewTelemetry pageViewTelemetry)
        {
            DebugCheckForUnredactedData(pageViewTelemetry.Properties);
        }
        if (telemetry is RequestTelemetry requestTelemetry)
        {
            DebugCheckForUnredactedData(requestTelemetry.Properties, requestTelemetry.Name, requestTelemetry.Url.ToString());
        }
        if (telemetry is TraceTelemetry traceTelemetry)
        {
            DebugCheckForUnredactedData(traceTelemetry.Properties, traceTelemetry.Message);
        }
    }

    [Conditional("DEBUG_REDACTOR")]
    private void DebugCheckForUnredactedData(IDictionary<string, string> properties, params string?[] rootPropertyValues)
    {
        foreach (string? rootProperty in rootPropertyValues.Where(v => v != null))
        {
            DebugCheckForUnredactedData(rootProperty!);
        }

        foreach (var property in properties)
        {
            DebugCheckForUnredactedData(property.Value);
        }
    }

    [Conditional("DEBUG_REDACTOR")]
    private void DebugCheckForUnredactedData(string value)
    {
        if ((value.Contains("email=") || value.Contains("email/"))
            && !(value.Contains("email=REDACTED") || value.Contains("emails/REDACTED")))
        {
            Debugger.Break();
        }     
    }
}
