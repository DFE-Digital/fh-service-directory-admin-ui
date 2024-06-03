using System.Net.Http.Json;
using FamilyHubs.SharedKernel.HealthCheck;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using FamilyHubs.Notification.Api.Client.Exceptions;

namespace FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;

public record AiRequest
{
    public string? Model { get; init; }
    public List<Message>? Messages { get; init; }
    public double Temperature { get; init; }
    public int MaxTokens { get; init; }
    public bool Stream { get; init; }
}

public record Message
{
    public string? Role { get; init; }
    public string? Content { get; init; }
}


public interface IAiClient
{
    //    Task<ContentCheckResponse> Call(IDictionary<string, string> content, CancellationToken cancellationToken = default);
    Task<ContentCheckResponse> Call(string content, CancellationToken cancellationToken = default);
}

public record Instance(string Reason, string Content);

public record Category(bool Flag, List<Instance> Instances);

public record ContentCheckResponse(
    int ReadingLevel,
    Category InappropriateLanguage,
    Category Security,
    Category PoliticisedSentiment,
    Category PII,
    Category GrammarAndSpelling,
    Category StyleViolations
);

public class AiClient : IAiClient //, IHealthCheckUrlGroup
{
    private readonly IHttpClientFactory _httpClientFactory;
    private static string? _endpoint;
    internal const string HttpClientName = "ai";

    public AiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    //todo: accept a collection of content for a service/location
    // use enum for ids
    public async Task<ContentCheckResponse> Call(string content, CancellationToken cancellationToken = default)//IDictionary<string, string> content, CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClient(HttpClientName);

        var request = new AiRequest
        {
            Model = "microsoft/Phi-3-mini-4k-instruct-gguf",
            Messages = new List<Message>
            {
                new Message
                {
                    Role = "system",
                    Content = """
 review the user content for suitability to be shown an a GOV.UK public site.
 reply with a json object only - do not add any pre or post amble.
 the json object should be in the following format:
 {
 "ReadingLevel": 9,
 "InappropriateLanguage": {
   "Flag": true,
   "Instances": [
     { 
       "Reason": "Swear words",
       "Content": "bloody stupid idiots",
     },
     { 
       "Reason": "Inappropriate slang",
       "Content": "OMFG this is fun",
     }
 ]
 },
 "Security": {
   "Flag": true,
   "Instances": [
     { 
       "Reason": "SQL injection",
       "Content": "' OR '1'='1'",
     },
     { 
       "Reason": "Cross-site scripting (XSS)",
       "Content": "<script>alert("not allowed")</script>",
     },
     { 
       "Reason": "Cross-site request forgery (CSRF)",
       "Content": "<img src='http://malicious.com/csrf'/>",
     },
     { 
       "Reason": "Sensitive data exposure",
       "Content": "The password for the admin account is 'password123'",
     }
 ]
 },
 "PoliticisedSentiment": {
   "Flag": true,
   "Instances": [
     { 
       "Reason": "Negative sentiment towards conservative party",
       "Content": "We help people the Tories couldn't care less about",
     }
 ]
 },
 "PII": {
   "Flag": true,
   "Instances": [
     { 
       "Reason": "Name of person not part of running the service",
       "Content": "we've helped famous alcoholics like Oliver Reed",
     }
 ]
 },
 "GrammarAndSpelling": {
   "Flag": true,
   "Instances": [
     { 
       "Reason": "Spelling",
       "Content": "eggselent",
     }
 ]
 },
 "StyleViolations": {
   "Flag": true,
   "Instances": [
     { 
       "Reason": "Abbreviations and acronyms",
       "Content": "as featured on the B.B.C.",
     }
 ]
 }
}
             
The ReadingLevel integer should be the reading age required to read and comprehend the content. Consider sentence complexity, vocabulary, content depth, paragraph length, and topic relevance.
             
InappropriateLanguage should flag whether the content contains inappropriate language.
If the flag is true, then the Instances array should contain objects with a Reason property and a Content property.
The Reason property should be a string describing why the content is inappropriate, and the Content property should be the text that is inappropriate.

Security is similar to InappropriateLanguage, but should flag whether the content contains security vulnerabilities.

PoliticisedSentiment is similar to InappropriateLanguage, but should flag whether the content contains politicised sentiment.

PII is similar to InappropriateLanguage, but should flag whether the content potentially contains personally identifiable information.
It is allowed to contain the name of a person if they are part of running the service, but not if they are a user of the service.

GrammarAndSpelling is similar to InappropriateLanguage, but should flag whether the content contains grammar or spelling mistakes.
The reason for a spelling mistake should be 'Spelling'. Grammatical mistakes should be flagged with the name of the type of grammatical mistake.
Example grammatical mistakes include:
  Its vs. It’s
  There vs. Their
  Your vs. You’re
  Affect vs. Effect
  Then vs. Than
  Lose vs. Loose
  Less vs. Fewer
  Farther vs. Further
  Complement vs. Compliment
  Principal vs. Principle

StyleViolations is similar to InappropriateLanguage, but should flag whether the content contains style violations.
Each instance should quote the name in the Reason instance field.
The style rules are:

Name: A*, A*s
Rule: The top grade in A levels. Use the symbol * not the word ‘star’. No apostrophe in the plural.

Name: A level
Rule: No hyphen. Lower case level.

Name: Abbreviations and acronyms
Rule: The first time you use an abbreviation or acronym explain it in full on each page unless it’s well known, like UK, DVLA, US, EU, VAT and MP. This includes government departments or schemes. Then refer to it by initials, and use acronym Markdown so the full explanation is available as hover text.
 Do not use full stops in abbreviations: BBC, not B.B.C.
"""
                },
                new Message
                {
                    Role = "user",
                    Content = content
                }
            },
            Temperature = 0.5,
            MaxTokens = 10000,
            Stream = false
        };
        //todo: param that forced output in json only

        using var response = await httpClient.PostAsJsonAsync("/v1/completions", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
            //throw new AiClientException(response, await response.Content.ReadAsStringAsync(cancellationToken));
            throw new InvalidOperationException("Error calling AI endpoint");

        var contentCheckResponse = await JsonSerializer.DeserializeAsync<ContentCheckResponse>(
            await response.Content.ReadAsStreamAsync(cancellationToken),
            cancellationToken: cancellationToken);

        if (contentCheckResponse is null)
        {
            // the only time it'll be null, is if the API returns "null"
            // (see https://stackoverflow.com/questions/71162382/why-are-the-return-types-of-nets-system-text-json-jsonserializer-deserialize-m)
            // unlikely, but possibly (pass new MemoryStream(Encoding.UTF8.GetBytes("null")) to see it actually return null)
            // note we hard-code passing "null", rather than messing about trying to rewind the stream, as this is such a corner case and we want to let the deserializer take advantage of the async stream (in the happy case)
            //throw new AiClientException(response, "null");
            throw new InvalidOperationException("Error calling AI endpoint");
        }

        return contentCheckResponse;
    }

    internal static string GetEndpoint(IConfiguration configuration)
    {
        const string endpointConfigKey = "FamilyHubsUi:Urls:Ai";

        // as long as the config isn't changed, the worst that can happen is we fetch more than once
        return _endpoint ??= ConfigurationException.ThrowIfNotUrl(
            endpointConfigKey,
            configuration[endpointConfigKey],
            "The AI URL", "http://localhost:1234");
    }

    //public static Uri HealthUrl(IConfiguration configuration)
    //{
    //    return new Uri(new Uri(GetEndpoint(configuration)), "SW1A2AA");
    //}
}