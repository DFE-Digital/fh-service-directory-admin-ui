using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.ServiceValidators;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.Azure;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.staged;

//todo: show suggested replacement with content with the replacement in and highlighted (leave existing line alone?) - fixes issue of accept suggestion link next to content where substitution wouldn't be successful due to inexact match
//todo: add reanalyze button
//todo: update service when accept suggestion clicked, or show updated with all suggestions and save button
//todo: get a suggestion to replace the whole content fixing all issues

public record CategoryDisplay(string Name, Category? Category);
public record CategoryInstanceDisplay(string CategoryName, Instance Instance, int Ordinal, string PropertyName, HtmlString HighlightedProperty);

//todo: validate taxonomies - if it doesn't match one of ours, then reject it
//todo: work on multiple fields
//todo: in prod version, checks will be done as a batch process (online as well when interacting) and the details saved to a service meta table

[Authorize(Roles = RoleGroups.AdminRole)]
public class Service_DetailsModel : HeaderPageModel
{
    private readonly IServiceDirectoryClient _serviceDirectoryClient;
    private readonly IAiClient _aiClient;
    private readonly IServiceRenderChecker _serviceRenderChecker;

    public ServiceDto? Service { get; set; }
//    public HtmlString HighlightedDescription { get; set; }
    public List<RenderCheckResult> RenderCheckResults;
    public ContentCheckResponse ContentCheckResponse { get; set; }
    public List<CategoryInstanceDisplay>? CategoryInstances { get; set; }

    public Service_DetailsModel(
        IServiceDirectoryClient serviceDirectoryClient,
        IAiClient aiClient,
        IServiceRenderChecker serviceRenderChecker)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
        _aiClient = aiClient;
        _serviceRenderChecker = serviceRenderChecker;
    }

    public async Task OnGetAsync(
        CancellationToken cancellationToken,
        long serviceId,
        bool? reanalyse)
    {
        // in prod version, do render checks and ai checks in parallel (although it will be a batch process, so not too important)

        Service = await _serviceDirectoryClient.GetServiceById(serviceId, cancellationToken);

        RenderCheckResults = new List<RenderCheckResult>();
        if (Service.ServiceType == ServiceType.FamilyExperience)
        {
            RenderCheckResults.Add(await _serviceRenderChecker.CheckRendering(RenderCheck.FindSearch,serviceId, cancellationToken));
        }
        else
        {
            RenderCheckResults.Add(new (RenderCheck.ConnectSearch, "Connect search", true, ""));
            RenderCheckResults.Add(
                await _serviceRenderChecker.CheckRendering(RenderCheck.ConnectDetails, serviceId, cancellationToken));
        }

        if (!string.IsNullOrEmpty(Service.Description))
        {
            if (reanalyse == true)
            {
                ContentCheckResponse = await _aiClient.Call(Service.Description, cancellationToken);
            }
            else
            {
                ContentCheckResponse = GetOverride(Service.Id)
                                       ?? await _aiClient.Call(Service.Description, cancellationToken);
            }

            CategoryInstances = new List<CategoryInstanceDisplay>();

            AddCategoryInstanceDisplays("Security", ContentCheckResponse.Security?.Instances);
            AddCategoryInstanceDisplays("Inappropriate language", ContentCheckResponse.InappropriateLanguage?.Instances);
            AddCategoryInstanceDisplays("Political bias", ContentCheckResponse.PoliticisedSentiment?.Instances);
            AddCategoryInstanceDisplays("Personally Identifiable Information", ContentCheckResponse.PII?.Instances);
            AddCategoryInstanceDisplays("GDS style violations", ContentCheckResponse.StyleViolations?.Instances);
            AddCategoryInstanceDisplays("Grammar and Spelling", ContentCheckResponse.GrammarAndSpelling?.Instances);

            //todo: doesn't highlight all categories
            //todo: handle repeat/overlapping text by only having one span per region??
            //HighlightedDescription = HighlightDescription(Service.Description,
            //    ContentCheckResponse.GrammarAndSpelling,
            //    ContentCheckResponse.InappropriateLanguage,
            //    ContentCheckResponse.Security,
            //    ContentCheckResponse.PoliticisedSentiment,
            //    ContentCheckResponse.PII,
            //    ContentCheckResponse.GrammarAndSpelling,
            //    ContentCheckResponse.StyleViolations);
        }
    }

    private void AddCategoryInstanceDisplays(string categoryName, List<Instance>? instances)
    {
        foreach (var (index, instance) in instances?.Select((instance, index) => (index + 1, instance))
                                          ?? Enumerable.Empty<(int, Instance)>())
        {
            CategoryInstances!.Add(new CategoryInstanceDisplay(categoryName, instance, index, "Description", HighlightIssue(Service!.Description!, instance)));
        }
    }

    private static HtmlString HighlightIssue(string property, Instance instance)
    {
        if (string.IsNullOrEmpty(instance.Content))
        {
            return new HtmlString(property);
        }

        string highlightedProperty = property.Replace(
            instance.Content,
            $"<span class=\"highlight\">{instance.Content}</span>");

        if (instance.Content.StartsWith("'") && instance.Content.EndsWith("'") && highlightedProperty == property)
        {
            string content = instance.Content[1..^1];
            highlightedProperty = property.Replace(
                content,
                $"<span class=\"highlight\">{content}</span>");
        }

        return new HtmlString(highlightedProperty);
    }

    private static HtmlString HighlightDescription(string description, params Category?[] categories)
    {
        string highlightedDescription = description;

        foreach (var instance in categories.SelectMany(c => c?.Instances ?? Enumerable.Empty<Instance>())
                     .Where(i => !string.IsNullOrEmpty(i.Content)))
        {
            string content = instance.Content;

            highlightedDescription = highlightedDescription.Replace(
                content,
                $"<span class=\"highlight\">{content}</span>");

            if (content.StartsWith("'") && content.EndsWith("'") && highlightedDescription == description)
            {
                content = content[1..^1];
                highlightedDescription = highlightedDescription.Replace(
                    content,
                    $"<span class=\"highlight\">{content}</span>");
            }
        }

        return new HtmlString(highlightedDescription);
    }

    /* to get a response from gpt, use this prompt, with the user content at the bottom...
     
1) Task Overview

Review the user content at the end of this prompt for suitability to be shown an a service directory hosted on a GOV.UK public site.
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
"SQL injection", "Cross-site scripting (XSS)", "Cross-site request forgery (CSRF)", "Sensitive information leak".
Do not limit any security issues you find to the list of supplied examples just given.
"Sensitive information leak" should be used if the content inadvertently contains e.g. a password, secret or API key.
If the content inadvertently contains a password, report that as a "Sensitive information leak", rather than suggesting a more secure password!

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

User Content:     
     
     */

    //todo: gpt-4 pii has an issue
    private static Dictionary<long, string> _overrides = new()
    {
        //todo: why are the service id's going up in 10,000's!
        {
            // Various
            260877,
            // ms phi-3 medium
            //" ```json\n{\n  \"ReadingLevel\": 5,\r\n  \"InappropriateLanguage\": {\r\n    \"Flag\": true,\r\n    \"Instances\": [\r\n      {\"Reason\": \"Use of derogatory term 'smelly kids'\", \"Content\": \"'This is a service for smelly kids.'\", \"SuggestedReplacement\": \"We provide services to help children with hygiene.\"},\r\n      {\"Reason\": \"Offensive phrase 'shit outta luck'\", \"Content\": \"'was shit outta luck as we got him good.'\", \"SuggestedReplacement\": \"fortunately, the process was successful.\"},\r\n      {\"Reason\": \"Inappropriate reference to a political figure in a demeaning context\", \"Content\": \"'Nigel Farage loves seeing the kids get dipped.'\", \"SuggestedReplacement\": \"\"}\n    ]\n  },\n  \"Security\": {\n    \"Flag\": false,\n    \"Instances\": []\n  },\n  \"PoliticisedSentiment\": {\n    \"Flag\": true,\n    \"Instances\": [\n      {\"Reason\": \"Inappropriate reference to a political figure\", \"Content\": \"'Nigel Farage loves seeing the kids get dipped.'\", \"SuggestedReplacement\": \"\"}\n    ]\n  },\n  \"PII\": {\n    \"Flag\": false,\n    \"Instances\": []\n  },\n  \"GrammarAndSpelling\": {\n    \"Flag\": true,\n    \"Instances\": [\n      {\"Reason\": \"Typo in 'sheep dip'\", \"Content\": \"'We dip them in sheep dip to get rid of all ticks and flees.'\", \"SuggestedReplacement\": \"We use a specialized shampoo to get rid of ticks and fleas.\"},\n      {\"Reason\": \"Incorrect verb agreement 'is' vs. 'are'\", \"Content\": \"'The smelliest kid we've sheep dipped is Stinky Bobby Hill.'\", \"SuggestedReplacement\": \"One of the smelliest kids we've sheep dipped was Stinky Bobby Hill.\"}\n    ]\n  },\n  \"StyleViolations\": {\n    \"Flag\": true,\n    \"Instances\": [\n      {\"Reason\": \"Informal tone not suitable for a professional service\", \"Content\": \"Party on dude!\", \"SuggestedReplacement\": \"We appreciate your participation!\"}\n    ]\n  },\n  \"Summary\": \"The text contains several instances of inappropriate language, political bias, and informal style. Grammar and spelling errors have also been identified.\"\n}\n```"
            // gpt 4
            "{\n\"ReadingLevel\": 10,\n\"InappropriateLanguage\": {\n\"Flag\": true,\n\"Instances\": [\n{\n\"Reason\": \"Vulgar language\",\n\"Content\": \"shit outta luck\",\n\"SuggestedReplacement\": \"very unlucky\",\n\"Notes\": \"The use of vulgar language is inappropriate for a GOV.UK public site.\"\n},\n{\n\"Reason\": \"Inappropriate slang\",\n\"Content\": \"Party on dude!\",\n\"SuggestedReplacement\": \"Thank you for your interest.\",\n\"Notes\": \"The use of slang is not suitable for formal content.\"\n},\n{\n\"Reason\": \"Inappropriate slang\",\n\"Content\": \"eejits\",\n\"SuggestedReplacement\": \"foolish\",\n\"Notes\": \"The term 'eejits' is inappropriate for formal content.\"\n}\n]\n},\n\"Security\": {\n\"Flag\": false,\n\"Instances\": []\n},\n\"PoliticisedSentiment\": {\n\"Flag\": true,\n\"Instances\": [\n{\n\"Reason\": \"Political endorsement\",\n\"Content\": \"The service is sponsored by Reform UK - Nigel Farage loves seeing the kids get dipped.\",\n\"SuggestedReplacement\": \"The service is provided with community support.\",\n\"Notes\": \"Political endorsements are inappropriate for a neutral public service directory.\"\n}\n]\n},\n\"PII\": {\n\"Flag\": true,\n\"Instances\": [\n{\n\"Reason\": \"Contains name of individual\",\n\"Content\": \"The smelliest kid we've sheep dipped is 'Stinky' Bobby Hill, who tried to escape the dip, but was shit outta luck as we got him good.\",\n\"SuggestedReplacement\": \"One of the smelliest kids we've helped tried to escape the dip but was very unlucky as we got him good.\",\n\"Notes\": \"Mentioning an individual's name in this context is not appropriate and violates privacy guidelines.\"\n}\n]\n},\n\"GrammarAndSpelling\": {\n\"Flag\": true,\n\"Instances\": [\n{\n\"Reason\": \"Spelling\",\n\"Content\": \"flees\",\n\"SuggestedReplacement\": \"fleas\"\n}\n]\n},\n\"StyleViolations\": {\n\"Flag\": true,\n\"Instances\": [\n{\n\"Reason\": \"Abbreviations and acronyms\",\n\"Content\": \"B.B.C.\",\n\"SuggestedReplacement\": \"BBC\",\n\"Notes\": \"GDS style guidelines recommend not using full stops in abbreviations.\"\n},\n{\n\"Reason\": \"Active voice\",\n\"Content\": \"We've received criticism from the B.B.C. program 'Watchdog' but ignore them, as their eejits!\",\n\"SuggestedReplacement\": \"We received criticism from the BBC program 'Watchdog' but chose to disregard it.\",\n\"Notes\": \"Using the active voice contributes to concise, clear content.\"\n}\n]\n},\n\"Summary\": \"The content contains issues related to inappropriate language, politicised sentiment, personal identifiable information, grammar and spelling, and style violations. Several changes are necessary to make the content suitable for a GOV.UK public site.\"\n}"
        },
        {
            250877,
            " ```json\r\n{\r\n  \"ReadingLevel\": 8,\r\n  \"InappropriateLanguage\": {\r\n    \"Flag\": false,\r\n    \"Instances\": []\r\n  },\r\n  \"Security\": {\r\n    \"Flag\": false,\r\n    \"Instances\": []\r\n  },\r\n  \"PoliticisedSentiment\": {\r\n    \"Flag\": false,\r\n    \"Instances\": []\r\n  },\r\n  \"PII\": {\r\n    \"Flag\": true, \r\n    \"Instances\": [{\r\n      \"Reason\": \"Disclosure of a personal story about an individual without their explicit consent.\", \r\n      \"Content\": \"This service works with substance dependant individuals to overcome their addiction.\r\nWe once famously helped Oliver Reed kick his alcohol addiction!\", \r\n      \"SuggestedReplacement\": \"Our service supports people struggling with substance dependency, guiding them on the path of recovery. One such story is how we assisted in overcoming alcohol addiction - a case that gained public recognition.\"\r\n    }]\r\n  },\r\n  \"GrammarAndSpelling\": {\r\n    \"Flag\": false,\r\n    \"Instances\": []\r\n  },\r\n  \"StyleViolations\": {\r\n    \"Flag\": true, \r\n    \"Instances\": [{\r\n      \"Reason\": \"The sentence is not structured in an active voice which improves readability and engagement.\", \r\n      \"Content\": \"We once famously helped Oliver Reed kick his alcohol addiction\",\r\n      \"SuggestedReplacement\": \"Oliver Reed overcame his alcohol addiction with our help, a famous instance.\"\r\n    }]\r\n  },\r\n  \"Summary\": \"The text discloses personal information without consent and could be structured in a more active voice for better readability.\"\n}\n``` "
        },
        {
            240877,
            " ```json\n{\n  \"ReadingLevel\": 10,\n  \"InappropriateLanguage\": {\n    \"Flag\": true,\n    \"Instances\": [\n      {\n        \"Content\": \"counterproductive\",\n        \"SuggestedReplacement\": \"ineffective\"\n      },\n      {\n        \"Content\": \"Tory's disastrous\",\n        \"Reason\": \"Politicized sentiment, using derogatory language to describe a political party.\",\n        \"SuggestedReplacement\": \"\"\n      }\n    ]\n  },\n  \"Security\": {\n    \"Flag\": false,\n    \"Instances\": []\n  },\n  \"PoliticisedSentiment\": {\n    \"Flag\": true,\n    \"Instances\": [\n      {\n        \"Content\": \"Tory's disastrous and counterproductive policy of austerity\",\n        \"Reason\": \"Negative sentiment towards a political party.\",\n        \"SuggestedReplacement\": \"\"\n      }\n    ]\n  },\n  \"PII\": {\n    \"Flag\": false,\n    \"Instances\": []\n  },\n  \"GrammarAndSpelling\": {\n    \"Flag\": false,\n    \"Instances\": []\n  },\n  \"StyleViolations\": {\n    \"Flag\": false,\n    \"Instances\": [\n      {\n        \"Content\": \"who are in desperate need of help\",\n        \"SuggestedReplacement\": \"in urgent need of assistance\"\n      }\n    ]\n  },\n  \"Summary\": \"The text contains politicized sentiment and a style violation. Suggested replacements for specific phrases have been provided.\"\n}\n```"
        },
        {
            270878,
            // ms phi-3 medium
            //" ```json\r\n{\r\n  \"ReadingLevel\": 8.7,\r\n  \"InappropriateLanguage\": {\r\n    \"Flag\": false,\r\n    \"Instances\": []\r\n  },\r\n  \"Security\": {\r\n    \"Flag\": true,\r\n    \"Instances\": [\r\n      {\r\n        \"Content\": \"The password for the admin account is 'password123'.\",\r\n        \"Reason\": \"Displaying default passwords can lead to security vulnerabilities.\",\r\n        \"SuggestedReplacement\": \"Remove this sentence from public view.\"\r\n      }\r\n    ]\r\n  },\r\n  \"PoliticisedSentiment\": {\r\n    \"Flag\": false,\r\n    \"Instances\": []\r\n  },\r\n  \"PII\": {\r\n    \"Flag\": true,\r\n    \"Instances\": [\r\n      {\r\n        \"Content\": \"You'll have to create an account to log-in.\", // This could be a hint towards data collection, though not direct PII.\r\n        \"Reason\": \"Mentioning mandatory account creation might indicate unnecessary personal data collection.\",\r\n        \"SuggestedReplacement\": \"Logging in is optional and can be done anonymously.\"\r\n      }\r\n    ]\r\n  },\r\n  \"GrammarAndSpelling\": {\r\n    \"Flag\": false,\r\n    \"Instances\": []\r\n  },\r\n  \"StyleViolations\": {\r\n    \"Flag\": true,\r\n    \"Instances\": [\r\n      {\r\n        \"Content\": \"You can chat to a human in office hours, or to a chat bot outside of office hours, or if you'd prefer the anonymity.\", // This sentence could be broken down for clarity.\r\n        \"Reason\": \"The sentence is complex and may not be easily understood by all readers.\",\r\n        \"SuggestedReplacement\": \"You can chat with a human during office hours, or use our anonymous chatbot outside of those times.\"\r\n      }\r\n    ]\r\n  },\r\n  \"Summary\": \"Security issues found due to displaying the admin password and potential PII concerns. Style violation noted for sentence clarity. No grammar/spelling errors detected.\"\r\n}\n```"
            // gpt 4
            "{\n\"ReadingLevel\": 8,\n\"InappropriateLanguage\": {\n\"Flag\": false,\n\"Instances\": []\n},\n\"Security\": {\n\"Flag\": true,\n\"Instances\": [\n{\n\"Reason\": \"Sensitive information leak\",\n\"Content\": \"The password for the admin account is 'password123'.\",\n\"SuggestedReplacement\": \"\",\n\"Notes\": \"A password has been included. It should be removed to prevent security risks.\"\n}\n]\n},\n\"PoliticisedSentiment\": {\n\"Flag\": false,\n\"Instances\": []\n},\n\"PII\": {\n\"Flag\": false,\n\"Instances\": []\n},\n\"GrammarAndSpelling\": {\n\"Flag\": false,\n\"Instances\": []\n},\n\"StyleViolations\": {\n\"Flag\": true,\n\"Instances\": [\n{\n\"Reason\": \"Abbreviations and acronyms\",\n\"Content\": \"log-in\",\n\"SuggestedReplacement\": \"log in\",\n\"Notes\": \"According to GDS style guidelines, 'log in' should be used as a verb phrase.\"\n}\n]\n},\n\"Summary\": \"The content contains a sensitive information leak due to the inclusion of an admin password. Additionally, there is a minor style violation regarding the use of 'log-in'.\"\n}"
        },
        {
            270877,
            " ```json\n{\n \"ReadingLevel\": 8,\n \"InappropriateLanguage\": {\n   \"Flag\": false,\n   \"Instances\": []\n },\n \"Security\": {\n   \"Flag\": true,\n   \"Instances\": [{\"Content\": \"' OR '1'='1'\", \"Reason\":\"Potential SQL Injection vulnerability\"}],\n  \"Summary\": \"Detected potential SQL injection attack in the input content.\"\n }\n}\n```"
        },
        {
            270879,
            // gpt 4
            "{\n\"ReadingLevel\": 11,\n\"InappropriateLanguage\": {\n\"Flag\": false,\n\"Instances\": []\n},\n\"Security\": {\n\"Flag\": false,\n\"Instances\": []\n},\n\"PoliticisedSentiment\": {\n\"Flag\": false,\n\"Instances\": []\n},\n\"PII\": {\n\"Flag\": false,\n\"Instances\": []\n},\n\"GrammarAndSpelling\": {\n\"Flag\": true,\n\"Instances\": [\n{\n\"Reason\": \"Spelling\",\n\"Content\": \"utilising\",\n\"SuggestedReplacement\": \"using\",\n\"Notes\": \"According to GDS guidelines, 'utilising' should be replaced with 'using'.\"\n}\n]\n},\n\"StyleViolations\": {\n\"Flag\": true,\n\"Instances\": [\n{\n\"Reason\": \"Banned words\",\n\"Content\": \"agenda\",\n\"SuggestedReplacement\": \"plan\",\n\"Notes\": \"According to GDS guidelines, 'agenda' should be replaced with 'plan'.\"\n},\n{\n\"Reason\": \"Banned words\",\n\"Content\": \"liaise\",\n\"SuggestedReplacement\": \"work with\",\n\"Notes\": \"According to GDS guidelines, 'liaise' should be replaced with 'work with'.\"\n},\n{\n\"Reason\": \"Banned words\",\n\"Content\": \"deploying\",\n\"SuggestedReplacement\": \"using\",\n\"Notes\": \"According to GDS guidelines, 'deploying' should be replaced with 'using'.\"\n},\n{\n\"Reason\": \"Banned words\",\n\"Content\": \"fostering\",\n\"SuggestedReplacement\": \"encouraging\",\n\"Notes\": \"According to GDS guidelines, 'fostering' should be replaced with 'encouraging'.\"\n},\n{\n\"Reason\": \"Banned words\",\n\"Content\": \"combat\",\n\"SuggestedReplacement\": \"solve\",\n\"Notes\": \"According to GDS guidelines, 'combat' should be replaced with 'solve'.\"\n},\n{\n\"Reason\": \"Abbreviations and acronyms\",\n\"Content\": \"FSS\",\n\"SuggestedReplacement\": \"Family Support Service (FSS)\",\n\"Notes\": \"According to GDS guidelines, abbreviations should be expanded on first use.\"\n},\n{\n\"Reason\": \"Abbreviations and acronyms\",\n\"Content\": \"ESI\",\n\"SuggestedReplacement\": \"Economic Stability Initiatives (ESI)\",\n\"Notes\": \"According to GDS guidelines, abbreviations should be expanded on first use.\"\n},\n{\n\"Reason\": \"Abbreviations and acronyms\",\n\"Content\": \"PGP\",\n\"SuggestedReplacement\": \"Parental Guidance Programs (PGP)\",\n\"Notes\": \"According to GDS guidelines, abbreviations should be expanded on first use.\"\n},\n{\n\"Reason\": \"Abbreviations and acronyms\",\n\"Content\": \"CDS\",\n\"SuggestedReplacement\": \"Child Development Support (CDS)\",\n\"Notes\": \"According to GDS guidelines, abbreviations should be expanded on first use.\"\n}\n]\n},\n\"Summary\": \"The content contains several style violations according to GDS guidelines, including banned words and improper use of abbreviations. Additionally, there is a minor spelling issue. Addressing these issues will improve the content's suitability for publication on a GOV.UK site.\"\n}"
        }
    };

    private ContentCheckResponse? GetOverride(long serviceId)
    {
        if (_overrides.TryGetValue(serviceId, out var overrideJson))
        {
            return _aiClient.ProcessAssistantMessage(overrideJson);
        }

        return null;
    }
}