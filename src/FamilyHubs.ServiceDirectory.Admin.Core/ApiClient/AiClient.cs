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
    ContentCheckResponse ProcessAssistantMessage(string assistantMessage);
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

    // until we switch to the Azure OpenAI Service (which respects json_object),
    // we need to as tolerant as possible to what the model returns when trying to deserialize
    private static JsonSerializerOptions InclusiveOptions = new JsonSerializerOptions
    {
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public AiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    //todo: show security check as passing, if no security issues
    //todo: if the model supplies invalid json, automatically retry (up to a limit), although shouldn't be required if we use openai
   //todo: run it twice for each and combine the issues??
    //todo: try specifying a json schema foe the json object
    //todo: mention suggestions should fit grammatically if replacing the content wrt case, e.g. "Oliver Reed" => "a famous individual" rather than "A f"
    //todo: mention that PII content violations aren't security issues

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
1) Task Overview

Review the user content for suitability to be shown an a service directory hosted on a GOV.UK public site.
As the site is a GOV.UK site, it should follow all the GOV.UK design principles and content design guidelines.
The results of your review will be shown to a set of human reviewers, who might make edits to the content following your review.
The human reviewers will also have the ability to click a button to automatically replace problematic snippets in the content with your suggested replacements.
The human reviewers will have the final decision on whether the content is suitable for publication.

2) Response Expected

Your response should only contain a valid json object and nothing outside of the json object.

Here's an example json object containing flagged issues to demonstrate the response format required:
 {
 "ReadingLevel": 9,
 "InappropriateLanguage": {
   "Flag": true,
   "Instances": [
     { 
       "Reason": "Vulgar language",
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
       "Reason": "Password, Secret or Key data exposure",
       "Content": "The password for the admin account is 'password123'.",
       "SuggestedReplacement": "",
       "Notes": "A password has been included."
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

3) Returned Json Object Expectations

3.1) "ReadingLevel" key and value

The value for the "ReadingLevel" key should be the reading age required to read and comprehend the content. Only use whole number ages, i.e. valid integers. Consider sentence complexity, vocabulary, content depth, paragraph length, and topic relevance.

3.2) Category Keys

The json object should include these keys, each of which represents a different category of potential issues with the user content:
"InappropriateLanguage", "Security", "PoliticisedSentiment", "PII", "GrammarAndSpelling", "StyleViolations".
Each category key should always be returned.

3.2.1) Category Key Value Object

Each category key should have a valid json object as its value, representing whether there are issues related to that particular category.
Each category object should return a boolean "Flag" key, which should have the value true if there are any issues related to that category, and false if there are no issues related to the category.
Each category object should have a key called "Instances" that contains an array of objects representing each instance of issue found.
If there are no relevant issues relating to the category, then the "Instances" key should have an empty array value.
Here's an example category key (the example is for the "" category) when there are no issues to report for the category:
"InappropriateLanguage": {
  "Flag": false,
  "Instances": []
},

Do not shorten the reply, e.g. by replacing the object with false, i.e. "InappropriateLanguage": false
The json in your reply is processed by code that expects it in a certain format. Do not deviate from the instructions.

3.2.1.1) "Instances" array value object ("Instance" object)

If any issues are found for the category, each instance should be returned as an "Instances" array value represented as a json object.
For example, if 2 different instances of inappropriate language are found then the "Instances" array should contain 2 array values.
Each instance object should contain 
For each instance of an issue found for a particular category, return a json object representing the issues.
Each instance array object should contain 3 mandatory keys:
"Reason", "Content" and "SuggestedReplacement"
and an optional field "Notes".
The "Reason" value should be a string describing the reason the issue has been raised for the parent category.
The "Content" value should be a string containing the the subsection of the user content that the issue instance relates to. It should exactly match a subsection of the supplied user content that, so that an automatic replacement of the problematic content can be actioned. E.g. do not add quotes around the content, or report multiple user content subsections in a single "Instance" object.
The "SuggestedReplacement" value should contain a json string with a suggested replacement for the inappropriate content as returned in the "Content" value.
The "Notes" value should be a string where you can add any additional notes that might be helpful for the human editor/approver relevant to the reported issue.
You could suggest alternatives to your suggested replacement as returned in "SuggestedReplacement",
and/or explain why you believe the issue needs to be addressed
and/or add context about the issue.

If there are multiple snippets of content with the same "Reason", then add an array value for each instance, with the same "Reason".
Do not have one array value instance with multiple snippets of content.

Remember that the "Content" value should be directly replaceable with the "SuggestedReplacement" value. Anything that breaks that should not be returned.

Example valid category objects:
"InappropriateLanguage": {
  "Flag": true,
  "Instances": [
    { 
      "Reason": "Vulgar language",
      "Content": "bloody stupid idiots",
      "SuggestedReplacement": "mentally disadvantaged people",
      "Notes": "Alternatively, remove the whole sentence the content appears in, as it adds little value."
    }
]

3.2.2) Category Semantics             

3.2.2.1) "InappropriateLanguage" category

"InappropriateLanguage"'s purpose is to return an object that indicates if the user content contains inappropriate language.
If inappropriate language is found, "Flag" should be set to true, and the "Instances" array should contain an array value for each separate instance of inappropriate language.
Each array value in the array for the the "Instances" key, should be represented by an "Instance" object as outlined in section 3.2.1.1)
An "InappropriateLanguage" key and object should be returned even if there are no instances of inappropriate language.

3.2.2.2) "Security" category

The "Security" key and related object value should follow the same pattern as outlined in section 3.2.1) Category Key Value Object.
The "Security" category should flag whether the content contains security vulnerabilities and the details of each instance of a potential security issue.
Any exposure of personally identifiable information (PII) should *NOT* be reported as a "Security" issue (it should be reported as a "PII" issue only).
Example "Reason"s for a "Security" instance are:
"SQL injection", "Cross-site scripting (XSS)", "Cross-site request forgery (CSRF)", "Password, Secret or Key data exposure"
Do not limit any security issues you find to the list of supplied examples just given.

3.2.2.3) "PoliticisedSentiment" category

The "PoliticisedSentiment" key and related object value should follow the same pattern as outlined in section 3.2.1) Category Key Value Object.
The "PoliticisedSentiment" category should flag whether whether the content contains politicised sentiment or political bias along with the details of each instance of problematic politicised content.

3.2.2.4) "PII" category

The "PII" key and related object value should follow the same pattern as outlined in section 3.2.1) Category Key Value Object.
The "PII" category should flag whether the content potentially contains personally identifiable information (PII)
in accordance with the The Data Protection Act 2018 and the UK’s implementation of the General Data Protection Regulation,
along with the details of each instance of a potential PII violation.

3.2.2.4.1) What is 'Personal Data'?
The UK data protection legislation defines 'personal data' as any information that relates to an identified or identifiable person. Either a named person or a person who can be identified using a combination of all the data available.

Data may also be personal where that individual can be identified indirectly from the information you hold in combination with other information.

Consider the example below.

"A newspaper wrote a story about suspected financial irregularities at a school and mentioned that the source was an anonymous headteacher. The paper specified the town and the two schools the headteacher had previously worked in. With a small amount of research it would be possible to determine the headteacher’s name. 

The key is that the individual could be identified.

Under data protection legislation, personal data is data that relates to a living individual.  As a matter of policy, DfE typically affords the data of deceased individuals the same protections as those available to a living individual.

3.2.2.4.2) What is 'Special Category Data'?

Special category data is a type of personal data that the UK GDPR identifies as requiring higher protection. This category includes:
Race, Ethnicity, Religious or philosophical beliefs, Trade union membership, Genetic data, Biometric and genetic data (e.g. fingerprints), Data concerning health, Sexual orientation

3.2.2.5) "GrammarAndSpelling" category

The "PII" key and related object value should follow the same pattern as outlined in section 3.2.1) Category Key Value Object.
The "PII" category should flag whether the content contains grammar or spelling mistakes, along with the details of each instance.
The string value of the "Reason" key for a spelling mistake should be "Spelling".
Grammatical mistakes should have a value for the "Reason" key be a brief description of the grammatical mistake.
Example grammatical mistakes include:
"Incorrect subject-verb agreement", "Wrong tense or verb form",
"Incorrect singular/plural agreement", "Incorrect word form", "Unclear pronoun reference", "Incorrect use of articles",
"Wrong or missing prepositions", "Omitted commas", "Too many commas", "Possessive apostrophe error", "Incorrect word use"

3.2.2.6) "StyleViolations" category

The "StyleViolations" key and related object value should follow the same pattern as outlined in section 3.2.1) Category Key Value Object.
The "StyleViolations" category should flag whether the content contains GDS style violations.
GDS style rules are outlined on this page: "https://www.gov.uk/guidance/style-guide/a-to-z-of-gov-uk-style".
Each instance should quote the name of the style violation in the "Reason" value string.
Some of the most important, relevant style rules are (represented using this format - "Name": reason):

"Abbreviations and acronyms": The first time you use an abbreviation or acronym explain it in full on each page unless it’s well known, like UK, DVLA, US, EU, VAT and MP. This includes government departments or schemes. Then refer to it by initials, and use acronym Markdown so the full explanation is available as hover text. Do not use full stops in abbreviations: BBC, not B.B.C.
"Active voice": Use the active rather than passive voice, as it contributes to concise, clear content.
"Addresses in the UK": Each part of the address should be on a new line. The content write the town and postcode on separate lines, not use commas at the end of each line, write the country on the line after the postcode not before, only include a country if there is a reasonable chance that the user will be writing to the address from a different country
"Addressing the user": Address the user as ‘you’ where possible and avoid using gendered pronouns like ‘he’ and ‘she’. Content on the site often makes a direct appeal to citizens and businesses to get involved or take action: ‘You can contact HMRC by phone and email’ or ‘Pay your car tax’, for example.
"Ages": Do not use hyphens in ages unless to avoid confusion, although it’s always best to write in a way that avoids ambiguity. For example, ‘a class of 15 16-year-old students took the A level course’ can be written as ‘15 students aged 16 took the A level course’. Use ‘aged 4 to 16 years’, not ‘4-16 years’.Avoid using ‘the over 50s’ or ‘under-18s’. Instead, make it clear who’s included: ‘aged 50 years and over’ and ‘aged 17 and under’.
"Allow list": Use allow list as the noun and allow as the verb. Do not use white list or whitelist.
"American and UK English": Use UK English spelling and grammar. For example, use ‘organise’ not ‘organize’, ‘modelling’ not ‘modeling’, and ‘fill in a form’, not ‘fill out a form’. American proper nouns, like 4th Mechanized Brigade or Pearl Harbor, take American English spelling.
"Ampersand": Use and rather than &, unless it’s a department’s logo image or a company’s name as it appears on the Companies House register.
"Banned words": Plain English is mandatory for all of GOV.UK so avoid using these words: agenda (unless it’s for a meeting), use ‘plan’ instead;advance, use ‘improve’ or something more specific;collaborate, use ‘work with’;combat (unless military), use ‘solve’, ‘fix’ or something more specific;commit/pledge, use ‘plan to x’, or ‘we’re going to x’ where ‘x’ is a specific verb;counter, use ‘prevent’ or try to rephrase a solution to a problem;deliver, use ‘make’, ‘create’, ‘provide’ or a more specific term (pizzas, post and services are delivered - not abstract concepts like improvements);deploy (unless it’s military or software), use ‘use’ or if putting something somewhere use ‘build’, ‘create’ or ‘put into place’;dialogue, use ‘spoke to’ or ‘discussion’;disincentivise, use ‘discourage’ or ‘deter’;empower, use ‘allow’ or ‘give permission’;facilitate, say something specific about how you’re helping - for example, use ‘run’ if talking about a workshop;focus, use ‘work on’ or ‘concentrate on’;foster (unless it’s children), use ‘encourage’ or ‘help’;impact (unless talking about a collision), use ‘have an effect on’ or ‘influence’;incentivise, use ‘encourage’ or ‘motivate’;initiate, use ‘start’ or ‘begin’;key (unless it unlocks something), usually not needed but can use ‘important’ or ‘significant’;land (unless you’re talking about aircraft), depending on context, use ‘get’ or ‘achieve’;leverage (unless in the financial sense), use ‘influence’ or ‘use’;liaise, use ‘work with’ or ‘work alongside’;overarching, usually superfluous but can use ‘encompassing’;progress, use ‘work on’ or ‘develop’ or ‘make progress’;promote (unless talking about an ad campaign or career advancement), use ‘recommend’ or ‘support’;robust (unless talking about a sturdy object), depending on context, use ‘well thought out’ or ‘comprehensive’;slim down (unless talking about one’s waistline), use ‘make smaller’ or ‘reduce the size’;streamline, use ‘simplify’ or ‘remove unnecessary administration’;strengthening (unless it’s strengthening bridges or other structures), depending on context, use ‘increasing funding’ or ‘concentrating on’ or ‘adding more staff’;tackle (unless talking about fishing tackle or a physical tackle, like in rugby), use ‘stop’, ‘solve’ or ‘deal with’;transform, describe what you’re doing to change the thing;utilise, use ‘use’

3.3) "Summary" key and value

The value of the top-level "Summary" key should contain a json string summary of your findings.
It's OK to add any additional information that may be relevant for the human reviewer.

3.4) Don't report the same specific issue as issue instance in multiple categories

If you report a specific issue under one category, you should not report it under another category.
For example, if you report a phrase under "PoliticisedSentiment" for containing negative sentiment towards a political party,
do not also report the exact same phrase under "InappropriateLanguage" and give the reason as negative sentiment towards a political party.
You can report the exact same phrase under "InappropriateLanguage" if it contains inappropriate language in addition to the negative political sentiment, but it must not be reported purely for the negative political sentiment.

Another example, if you report an instance of an individual's name as a "PII" category issue, do not also report it as a "Security" issue (as it's a data leakage).

3.5) Example root json object when no issues are found

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

Notes:
If a category doesn't have any issues, there's no need to supply an Instance with blank string or null values for the "Reason", "Content" or "SuggestedReplacement" keys.

3.6) JSON object response format

It is critical that your response should only contain a json object, and no other text.
A valid json object is one that is conformant to RFC 7159 and is compatible with Microsoft .Net's "System.Text.Json" deserializer.
Don't wrap the json object in markdown formatting.
Do not add any explanation of the contents of the json object, either before or after the json object (there is a top-level key in the root json object called "Summary" which you can use to summaries your findings).
Do not add any additional text for any reason before or after the json object.
Do not include comments in the json object, e.g. '//'.
Do not add any characters before the initial '{' or end '}'.
Ensure all characters that should be escaped in json strings are correctly escaped and don't try to escape characters that shouldn't be escaped, e.g. the single quote (') character does not need to be escaped.
Json strings should use double quotes (") for string delimiters, do not use single quotes (') to wrap string values as it's not valid json.
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

        //// until we switch to the Azure OpenAI Service (which respects json_object),
        //// we need to as tolerant as possible to what the model returns when trying to deserialize
        //var inclusiveOptions = new JsonSerializerOptions
        //{
        //    AllowTrailingCommas = true,
        //    ReadCommentHandling = JsonCommentHandling.Skip,
        //    PropertyNameCaseInsensitive = true,
        //    NumberHandling = JsonNumberHandling.AllowReadingFromString
        //};

        var chatCompletionResponse = await JsonSerializer.DeserializeAsync<ChatCompletionResponse>(
            await response.Content.ReadAsStreamAsync(cancellationToken),
            InclusiveOptions,
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

        return ProcessAssistantMessage(assistantMessage);
    }

    public ContentCheckResponse ProcessAssistantMessage(string assistantMessage)
    {
        //todo: just remove everything before the first { and after the last } ?

        // remove the initial " ```json\n" and final "\n```" from responseString
        if (assistantMessage.Contains("```"))
        {
            // sometimes the assistant returns the message in a code block, sometimes not
            // shouldn't be a problem using Azure OpenAI Service and json_object response_format

            assistantMessage = assistantMessage[assistantMessage.IndexOf('\n')..];
            assistantMessage = assistantMessage.TrimEnd(' ', '`', '\n');
        }

        var contentCheckResponse = JsonSerializer.Deserialize<ContentCheckResponse>(assistantMessage, InclusiveOptions);

        if (contentCheckResponse is null)
        {
            // the only time it'll be null, is if the API returns "null"
            // (see https://stackoverflow.com/questions/71162382/why-are-the-return-types-of-nets-system-text-json-jsonserializer-deserialize-m)
            // unlikely, but possibly (pass new MemoryStream(Encoding.UTF8.GetBytes("null")) to see it actually return null)
            // note we hard-code passing "null", rather than messing about trying to rewind the stream, as this is such a corner case and we want to let the deserializer take advantage of the async stream (in the happy case)
            //throw new AiClientException(response, "null");
            throw new InvalidOperationException("Error processing assistant response");
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