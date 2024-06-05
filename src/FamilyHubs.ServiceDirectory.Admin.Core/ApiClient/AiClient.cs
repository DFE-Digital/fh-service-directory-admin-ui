using System.Net.Http.Json;
using FamilyHubs.SharedKernel.HealthCheck;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using FamilyHubs.Notification.Api.Client.Exceptions;
using FamilyHubs.SharedKernel.Services.PostcodesIo;
using System.Text.Json.Serialization;

namespace FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;

//todo: use spellcheck component to check spelling and grammer, rather than AI

//todo: use browser tool to check contact details/opening times or anything out of date

//todo: to productionise, use Azure OpenAI Service and this client: https://learn.microsoft.com/en-us/dotnet/api/overview/azure/ai.openai.assistants-readme?view=azure-dotnet-preview

public record AiRequest(
    string? model,
    ResponseFormat response_format,
    List<Message>? messages,
    double temperature,
    int max_tokens,
    bool stream)
{
}

public record Message(string role, string content)
{
}

//public enum ResponseFormatType
//{
//    [JsonConverter(typeof(JsonStringEnumConverter))]
//    json_object
//}

//public record ResponseFormat(ResponseFormatType type);
public record ResponseFormat(string type);

public interface IAiClient
{
    //    Task<ContentCheckResponse> Call(IDictionary<string, string> content, CancellationToken cancellationToken = default);
    Task<ContentCheckResponse> Call(string content, CancellationToken cancellationToken = default);
}

public record Instance(string Reason, string Content, string SuggestedReplacement);

public record Category(bool Flag, List<Instance> Instances);

//todo: add to prompt checking e.g. service. pass data as a json object representing a service
//todo: add importance enum to some/all. informational (shouldn't block publication, but possible improvement), marginal (may or may not need to block publication), important (should block publication)
// use to colour block according to highest importance of instance in category

// PoliticisedSentiment or PoliticalBias?

//todo: change to Category array
public record ContentCheckResponse(
    int ReadingLevel,
    Category InappropriateLanguage,
    Category Security,
    Category PoliticisedSentiment,
    Category PII,
    //todo: don't use category. add a field so that the suggested replacement can be shown
    //todo: have button to make suggested replacement
    Category GrammarAndSpelling,
    Category StyleViolations,
    string? Notes
)
{
    //public IEnumerable<string> Flags =>
    //new[] { InappropriateLanguage, Security, PoliticisedSentiment, PII, GrammarAndSpelling, StyleViolations }
    //        .Where(c => c.Flag)
    //        .SelectMany(c => c.Instances.Select(i => i.Reason));
}

public record ChatCompletionResponse(
    string id,
    [property: JsonPropertyName("object")]
    string Object,
    long created,
    string model,
    List<Choice> choices,
    Usage usage
);

public record Choice(
    int index,
    Message message,
    string finish_reason
);

public record Usage(
    int prompt_tokens,
    int completion_tokens,
    int tota1l_tokens
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

        var request = new AiRequest(
            //model: "microsoft/Phi-3-mini-4k-instruct-gguf",
            //model: "SanctumAI/Meta-Llama-3-8B-Instruct-GGUF",
            //model: "microsoft/Phi-3-mini-4k-instruct-gguf",
            model: "RichardErkhov/microsoft_-_Phi-3-medium-4k-instruct-gguf",
            response_format: new("json_object"),//ResponseFormatType.json_object),
            messages: new List<Message>
            {
                new Message(
                    role: "system",
                    content: """
Review the user content for suitability to be shown an a GOV.UK public site, which is a service directory.
As the site is a GOV.UK site, it should follow all the GOV.UK design principles and content design guidelines.
It is critical that your response should only contain a json object, and no other text.
Don't wrap the json object in markdown formatting.
Do not add any explanation of the contents of the json object, either before or after the json object (there is a field in the json object called 'Notes' which you can use to add any additional information that may be relevant to the review).
Do not add any additional text for any reason before or after the json object.
Also, do not include comments in the json object, e.g. '//'.

 the json object should be in the following format:
 {
 "ReadingLevel": 9,
 "InappropriateLanguage": {
   "Flag": true,
   "Instances": [
     { 
       "Reason": "Swear words",
       "Content": "bloody stupid idiots",
       "SuggestedReplacement": "mentally disadvantaged people"
     },
     { 
       "Reason": "Inappropriate slang",
       "Content": "OMFG you're going to have the time of your life",
       "SuggestedReplacement": "you're going to have the time of your life"
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
       "Content": "usually due to the Tory's disastrous and counterproductive policy of austerity",
       "SuggestedReplacement": "usually due to challenging policies regarding public spending"
     }
 ]
 },
 "PII": {
   "Flag": true,
   "Instances": [
     { 
       "Reason": "Name of person not part of running the service",
       "Content": "we've helped famous alcoholics like Oliver Reed",
       "SuggestedReplacement": "we've helped many alcoholics overcome their addiction"
     }
 ]
 },
 "GrammarAndSpelling": {
   "Flag": true,
   "Instances": [
     { 
       "Reason": "Spelling",
       "Content": "eggselent",
       "SuggestedReplacement": "excellent"
     }
 ]
 },
 "StyleViolations": {
   "Flag": true,
   "Instances": [
     { 
       "Reason": "Abbreviations and acronyms",
       "Content": "as featured on the B.B.C.",
       "SuggestedReplacement": "as featured on the BBC"
     }
 ]
 },
 "Notes": "A suggestion was made to remove the negative sentiment towards a political party, which could alienate readers and compromise neutrality. Additionally, stylistic recommendations were made for clarity."
}
             
The ReadingLevel integer should be the reading age required to read and comprehend the content. Consider sentence complexity, vocabulary, content depth, paragraph length, and topic relevance.
             
InappropriateLanguage should flag whether the content contains inappropriate language.
If the flag is true, then the Instances array should contain objects with a Reason property and a Content property.
The Reason property should be a string describing why the content is inappropriate, and the Content property should be the text that is inappropriate.
The SuggestedReplacement property is optional and should contain a string with a suggested replacement for the inappropriate content.

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

Notes should contain any additional information that may be relevant to the review.

StyleViolations is similar to InappropriateLanguage, but should flag whether the content contains style violations.
Each instance should quote the name in the Reason instance field.
The style rules are:

Name: Abbreviations and acronyms
Rule: The first time you use an abbreviation or acronym explain it in full on each page unless it’s well known, like UK, DVLA, US, EU, VAT and MP. This includes government departments or schemes. Then refer to it by initials, and use acronym Markdown so the full explanation is available as hover text.
Do not use full stops in abbreviations: BBC, not B.B.C.

If you report a specific issue under one category, you should not report it under another category.
For example, if you report a phrase under PoliticisedSentiment for containing negative sentiment towards a political party,
do not also report the exact same phrase under InappropriateLanguage and give the reason as negative sentiment towards a political party.
You can also report the same phrase under InappropriateLanguage if it contains inappropriate in addition to the negative political sentiment, but it must not be reported purely for the negative political sentiment.

Remember: return a valid json object, and nothing else.
"""),
                new Message(
                
                    role: "user",
                    content: content
                )
            },
            temperature: 1,
            max_tokens: 10000,
            stream: false
        );
        //todo: param that forced output in json only

        using var response = await httpClient.PostAsJsonAsync("/v1/chat/completions", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
            //throw new AiClientException(response, await response.Content.ReadAsStringAsync(cancellationToken));
            //throw new InvalidOperationException("Error calling AI endpoint");
        //todo: use this just for now to see response
            throw new PostcodesIoClientException(response, await response.Content.ReadAsStringAsync(cancellationToken));

        // until we switch to the Azure OpenAI Service (which respects json_object),
        // we need to as tolerant as possible to what the model returns when trying to deserialize
        var inclusiveOptions = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        var chatCompletionResponse = await JsonSerializer.DeserializeAsync<ChatCompletionResponse>(
            await response.Content.ReadAsStreamAsync(cancellationToken),
            inclusiveOptions,
            cancellationToken);

        if (chatCompletionResponse is null)
        {
            // the only time it'll be null, is if the API returns "null"
            // (see https://stackoverflow.com/questions/71162382/why-are-the-return-types-of-nets-system-text-json-jsonserializer-deserialize-m)
            // unlikely, but possibly (pass new MemoryStream(Encoding.UTF8.GetBytes("null")) to see it actually return null)
            // note we hard-code passing "null", rather than messing about trying to rewind the stream, as this is such a corner case and we want to let the deserializer take advantage of the async stream (in the happy case)
            //throw new AiClientException(response, "null");
            throw new InvalidOperationException("Error calling AI endpoint");
        }

        string assistantMessage = chatCompletionResponse.choices[0].message.content;

        //todo: just remove everything before the first { and after the last } ?

        // remove the initial " ```json\n" and final "\n```" from responseString
        if (assistantMessage.Contains("```"))
        {
            // sometimes the assistant returns the message in a code block, sometimes not
            // shouldn't be a problem using Azure OpenAI Service and json_object response_format

            assistantMessage = assistantMessage[assistantMessage.IndexOf('\n')..];
            assistantMessage = assistantMessage.TrimEnd(' ', '`', '\n');
        }

        var contentCheckResponse = JsonSerializer.Deserialize<ContentCheckResponse>(assistantMessage, inclusiveOptions);

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