using System.Net.Http.Json;
using FamilyHubs.SharedKernel.HealthCheck;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using FamilyHubs.Notification.Api.Client.Exceptions;
using FamilyHubs.SharedKernel.Services.PostcodesIo;
using System.Text.Json.Serialization;

namespace FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;

//todo: ask for a priority: minor issues (could be published as is), possible blocking issues (issues could be sever enough to stop publication), block (serious issues, shouldn't be published)

//todo: to reduce the chance of the model messing up the json, don't get it to return the flag, just the instances, then can check for .Any() - removes a possible change of a mismatch
//todo: return the categories as an array, rather than individual objects

//todo: POC to create the staged services (but that will require access to a model with a browse tool)

//todo: use spellcheck component to check spelling and grammer, rather than AI

//todo: use browser tool to check contact details/opening times or anything out of date

//todo: to productionise, use Azure OpenAI Service and this client: https://learn.microsoft.com/en-us/dotnet/api/overview/azure/ai.openai.assistants-readme?view=azure-dotnet-preview

//todo: handle overlapping issue highlights on content

//todo: if render (either render fails or security blocked it), then don't allow approval

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

public record Instance(string Reason, string Content, string? SuggestedReplacement, string? Notes);

public record Category(bool Flag, List<Instance> Instances);

//todo: add to prompt checking e.g. service. pass data as a json object representing a service
//todo: add importance enum to some/all. informational (shouldn't block publication, but possible improvement), marginal (may or may not need to block publication), important (should block publication)
// use to colour block according to highest importance of instance in category

// PoliticisedSentiment or PoliticalBias?

// Categories are supposed to be mandatory, but we must be as tolerant as possible to what the model returns
//todo: change to Category array
public record ContentCheckResponse(
    decimal ReadingLevel,
    Category? InappropriateLanguage,
    Category? Security,
    Category? PoliticisedSentiment,
    Category? PII,
    //todo: don't use category. add a field so that the suggested replacement can be shown
    //todo: have button to make suggested replacement
    Category? GrammarAndSpelling,
    Category? StyleViolations,
    string? Summary
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

    //todo: try specifying a json schema foe the json object
    //todo: mention suggestions should fit grammatically if replacing the content wrt case, e.g. "Oliver Reed" => "a famous individual" rather than "A f"
    //todo: mention that PII content violations aren't security issues
    //todo: handle instances with blank content and blank suggestions, or flag=true and no instances
    //todo: always display security?

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
                //todo: describe each component separately first

                new Message(
                    role: "system",
                    content: """
Review the user content for suitability to be shown an a GOV.UK public site, which is a service directory.
As the site is a GOV.UK site, it should follow all the GOV.UK design principles and content design guidelines.
The results of your review will be shown to a set of human reviewers, who might make edits to the content following your review.
The human reviewers will also have the ability to click a button to automatically replace problematic snippets in the content with your suggested replacements.
The human reviewers will have the final decision on whether the content is suitable for publication.

It is critical that your response should only contain a json object, and no other text.
A valid json object is one that is conformant to RFC 7159 and is compatible with Microsoft .Net's "System.Text.Json" deserializer.
Don't wrap the json object in markdown formatting.
Do not add any explanation of the contents of the json object, either before or after the json object (there is a top-level key in the root json object called "Summary" which you can use to summaries your findings).
Do not add any additional text for any reason before or after the json object.
Do not include comments in the json object, e.g. '//'.
Do not add any characters before the initial '{' or end '}'.
Ensure all characters that should be escaped in json strings are correctly escaped and don't try to escape characters that shouldn't be escaped, e.g. the single quote (') character does not need to be escaped.

Here's an example json object containing flagged issues to demonstrate the response format required:
 {
 "ReadingLevel": 9,
 "InappropriateLanguage": {
   "Flag": true,
   "Instances": [
     { 
       "Reason": "Swear words",
       "Content": "bloody stupid idiots",
       "SuggestedReplacement": "mentally disadvantaged people",
       "Notes": "Alternatively, remove the whole sentence the content appears in, as it adds little value."
     },
     { 
       "Reason": "Inappropriate slang",
       "Content": "OMFG you're going to have the time of your life",
       "SuggestedReplacement": "You're going to have the time of your life",
       "Notes": ""
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
       "Content": "The password for the admin account is 'password123'.",
       "SuggestedReplacement": "",
       "Notes": "Sensitive data should never be exposed in this way."
     }
 ]
 },
 "PoliticisedSentiment": {
   "Flag": true,
   "Instances": [
     { 
       "Reason": "Negative sentiment towards Conservative party",
       "Content": "usually due to the Tory's disastrous and counterproductive policy of austerity",
       "SuggestedReplacement": "usually due to challenging policies regarding public spending",
       "Notes": "The negative sentiment towards a political party could alienate readers and compromise neutrality."
     }
 ]
 },
 "PII": {
   "Flag": true,
   "Instances": [
     { 
       "Reason": "Contains name of supported individual",
       "Content": "we've helped famous alcoholics like Oliver Reed",
       "SuggestedReplacement": "we've helped many alcoholics overcome their addiction",
       "Notes": "The Data Protection Act 2018 does not explicitly disallow mentioning names in a public service directory, but it emphasizes responsible handling and protection of personal data. An individual’s name is considered identifiable."
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
     },
    { 
      "Reason": "Its vs. It’s",
      "Content": "The cat licked it's paw.",
      "SuggestedReplacement": "The cat licked its paw."
    }
 ]
 },
 "StyleViolations": {
   "Flag": true,
   "Instances": [
     { 
       "Reason": "Abbreviations and acronyms",
       "Content": "as featured on the B.B.C.",
       "SuggestedReplacement": "as featured on the BBC",
       "Notes": "GDS style guidelines recommend not using full stops in abbreviations."
     }
 ]
 },
 "Summary": "The content contains many issues, including security issues. It would be prudent to check who inserted the possible security exploits, as it appears someone is trying to hack the service directory. There are so many content issues, it might be best to just rewrite the content from scratch."
}
The value for the "ReadingLevel" key should be the reading age required to read and comprehend the content. Only use whole number ages, i.e. valid integers. Consider sentence complexity, vocabulary, content depth, paragraph length, and topic relevance.
             
"InappropriateLanguage"'s purpose is to return an object that indicates if the user content contains inappropriate language and if it does, then it returns details about each instance of inappropriate language.
An "InappropriateLanguage" key and object should be returned even if there are no instances of inappropriate language.
If the user content contains inappropriate language, the top-level key "InappropriateLanguage" should have an object value,
where the object has a key called "Flag" with the boolean value true,
and a key called "Instances" with an array value, where each array value is an object with 3 mandatory keys: "Reason", "Content" and "SuggestedReplacement" and an optional field "Notes"..
The "Reason" value should be a string describing why the content is inappropriate.
The "Content" value should be a string containing the text that is inappropriate - it should exactly match a subsection of the supplied content, so that a automatic replacement of the problematic content can be actioned. E.g. do not add quotes around the content.
The "SuggestedReplacement" value should contain a string with a suggested replacement for the inappropriate content as returned in the "Content" value.
The "Notes" value should be a string where you can add any additional notes that might be helpful for the human editor/approver relevant to the reported issue. You could suggest alternatives to your suggested replacement, explain why you believe the issue needs to be addressed and/or add context about the issue.

If there are multiple snippets of content with the same "Reason", then add an array value for each instance, with the same "Reason".
Do not have one array value instance with multiple snippets of content.

Remember that the "Content" value should be directly replaceable with the "SuggestedReplacement" value. Anything that breaks that should not be returned.

If there is no inappropriate language, then return the top-level key "InappropriateLanguage" like this:
"InappropriateLanguage": {
    "Flag": false,
    "Instances": []
}
Do not shorten the reply, e.g. by replacing the object with false, i.e. "Inappropriate": false
The json you reply with is processed by code that expects it in a certain format. do not deviate from the instructions.

The "Security" key and related object value should follow the same rules as "InappropriateLanguage", but should flag whether the content contains security vulnerabilities and the details of each instance of a potential security issue.

The "PoliticisedSentiment" key and related object value should follow the same rules as "InappropriateLanguage", but should flag whether the content contains politicised sentiment or political bias along with the details of each instance of problematic politicised content.

The "PII" key and related object value should follow the same rules as "InappropriateLanguage",
but should flag whether the content potentially contains personally identifiable information (PII)
in accordance with the The Data Protection Act 2018 (the UK’s implementation of the General Data Protection Regulation)
along with the details of each instance of a potential PII violation.

The "GrammarAndSpelling" key and related object value should follow the same rules as "InappropriateLanguage",
but should flag whether the content contains grammar or spelling mistakes, along with the details of each instance.
The string value of the "Reason" key for a spelling mistake should be "Spelling".
Grammatical mistakes should have a value for the "Reason" key be a brief description of the grammatical mistake.
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

The "StyleViolations" key and related object value should follow the same rules as "InappropriateLanguage",
but should flag whether the content contains GDS style violations.
Each instance should quote the name of the style violation in the "Reason" value string.
The style rules are:

Name: "Abbreviations and acronyms"
Rule: The first time you use an abbreviation or acronym explain it in full on each page unless it’s well known, like UK, DVLA, US, EU, VAT and MP. This includes government departments or schemes. Then refer to it by initials, and use acronym Markdown so the full explanation is available as hover text. Do not use full stops in abbreviations: BBC, not B.B.C.

The value of the top-level "Summary" key should contain a summary of your findings. It's OK to add any additional information that may be relevant for the human reviewer.

If you report a specific issue under one category, you should not report it under another category.
For example, if you report a phrase under "PoliticisedSentiment" for containing negative sentiment towards a political party,
do not also report the exact same phrase under "InappropriateLanguage" and give the reason as negative sentiment towards a political party.
You can report the exact same phrase under "InappropriateLanguage" if it contains inappropriate language in addition to the negative political sentiment, but it must not be reported purely for the negative political sentiment.

Here's an example root json object where no issues are found:
 {
 "ReadingLevel": 8,
 "InappropriateLanguage": {
   "Flag": false,
   "Instances": []
 },
 "Security": {
   "Flag": false,
   "Instances": []
 },
 "PoliticisedSentiment": {
  "Flag": false,
  "Instances": []
},
 "PII": {
  "Flag": false,
  "Instances": []
},
 "GrammarAndSpelling": {
  "Flag": false,
  "Instances": []
},
 "StyleViolations": {
  "Flag": false,
  "Instances": []
},
 "Summary": "No issues found."
}

If a category doesn't have any issues, there's no need to supply an Instance with blank string or null values for the "Reason", "Content" or "SuggestedReplacement" keys.

Remember: only return a valid json object and nothing else!
"""),

                /*
                 * supplying the examples makes it worse!
                 *E.g. return this...
                   "InappropriateLanguage": {
                   "Flag": true,
                   "Instances": [
                     { "Reason": "Derogatory term and vulgarity", "Content": "smelly kids", "SuggestedReplacement": "Children with odour concerns" },
                     { "Reason": "Derogatory term and vulgarity", "Content": "shit outta luck", "SuggestedReplacement": "Unfortunately, our luck isn’t fortuitous." }
                     ]
                   },
                   
                   rather than this...
                   "InappropriateLanguage": {
                     "Flag": true,
                     "Instances": [
                       { "Reason": "Derogatory term and vulgarity", "Content": "'smelly kids', 'shit outta luck'", "SuggestedReplacement": "'Children with odour concerns', 'Unfortunately, our luck isn’t fortuitous.'" }
                     ]
                   },
                   
                 */

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