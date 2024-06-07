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

    private static Dictionary<long, string> _overrides = new()
    {
        //todo: why are the service id's going up in 10,000's!
        {
            260877,
            " ```json\n{\n  \"ReadingLevel\": 5,\r\n  \"InappropriateLanguage\": {\r\n    \"Flag\": true,\r\n    \"Instances\": [\r\n      {\"Reason\": \"Use of derogatory term 'smelly kids'\", \"Content\": \"'This is a service for smelly kids.'\", \"SuggestedReplacement\": \"We provide services to help children with hygiene.\"},\r\n      {\"Reason\": \"Offensive phrase 'shit outta luck'\", \"Content\": \"'was shit outta luck as we got him good.'\", \"SuggestedReplacement\": \"fortunately, the process was successful.\"},\r\n      {\"Reason\": \"Inappropriate reference to a political figure in a demeaning context\", \"Content\": \"'Nigel Farage loves seeing the kids get dipped.'\", \"SuggestedReplacement\": \"\"}\n    ]\n  },\n  \"Security\": {\n    \"Flag\": false,\n    \"Instances\": []\n  },\n  \"PoliticisedSentiment\": {\n    \"Flag\": true,\n    \"Instances\": [\n      {\"Reason\": \"Inappropriate reference to a political figure\", \"Content\": \"'Nigel Farage loves seeing the kids get dipped.'\", \"SuggestedReplacement\": \"\"}\n    ]\n  },\n  \"PII\": {\n    \"Flag\": false,\n    \"Instances\": []\n  },\n  \"GrammarAndSpelling\": {\n    \"Flag\": true,\n    \"Instances\": [\n      {\"Reason\": \"Typo in 'sheep dip'\", \"Content\": \"'We dip them in sheep dip to get rid of all ticks and flees.'\", \"SuggestedReplacement\": \"We use a specialized shampoo to get rid of ticks and fleas.\"},\n      {\"Reason\": \"Incorrect verb agreement 'is' vs. 'are'\", \"Content\": \"'The smelliest kid we've sheep dipped is Stinky Bobby Hill.'\", \"SuggestedReplacement\": \"One of the smelliest kids we've sheep dipped was Stinky Bobby Hill.\"}\n    ]\n  },\n  \"StyleViolations\": {\n    \"Flag\": true,\n    \"Instances\": [\n      {\"Reason\": \"Informal tone not suitable for a professional service\", \"Content\": \"Party on dude!\", \"SuggestedReplacement\": \"We appreciate your participation!\"}\n    ]\n  },\n  \"Summary\": \"The text contains several instances of inappropriate language, political bias, and informal style. Grammar and spelling errors have also been identified.\"\n}\n```"
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
            " ```json\r\n{\r\n  \"ReadingLevel\": 8.7,\r\n  \"InappropriateLanguage\": {\r\n    \"Flag\": false,\r\n    \"Instances\": []\r\n  },\r\n  \"Security\": {\r\n    \"Flag\": true,\r\n    \"Instances\": [\r\n      {\r\n        \"Content\": \"The password for the admin account is 'password123'.\",\r\n        \"Reason\": \"Displaying default passwords can lead to security vulnerabilities.\",\r\n        \"SuggestedReplacement\": \"Remove this sentence from public view.\"\r\n      }\r\n    ]\r\n  },\r\n  \"PoliticisedSentiment\": {\r\n    \"Flag\": false,\r\n    \"Instances\": []\r\n  },\r\n  \"PII\": {\r\n    \"Flag\": true,\r\n    \"Instances\": [\r\n      {\r\n        \"Content\": \"You'll have to create an account to log-in.\", // This could be a hint towards data collection, though not direct PII.\r\n        \"Reason\": \"Mentioning mandatory account creation might indicate unnecessary personal data collection.\",\r\n        \"SuggestedReplacement\": \"Logging in is optional and can be done anonymously.\"\r\n      }\r\n    ]\r\n  },\r\n  \"GrammarAndSpelling\": {\r\n    \"Flag\": false,\r\n    \"Instances\": []\r\n  },\r\n  \"StyleViolations\": {\r\n    \"Flag\": true,\r\n    \"Instances\": [\r\n      {\r\n        \"Content\": \"You can chat to a human in office hours, or to a chat bot outside of office hours, or if you'd prefer the anonymity.\", // This sentence could be broken down for clarity.\r\n        \"Reason\": \"The sentence is complex and may not be easily understood by all readers.\",\r\n        \"SuggestedReplacement\": \"You can chat with a human during office hours, or use our anonymous chatbot outside of those times.\"\r\n      }\r\n    ]\r\n  },\r\n  \"Summary\": \"Security issues found due to displaying the admin password and potential PII concerns. Style violation noted for sentence clarity. No grammar/spelling errors detected.\"\r\n}\n```"
        },
        {
            270877,
            " ```json\n{\n \"ReadingLevel\": 8,\n \"InappropriateLanguage\": {\n   \"Flag\": false,\n   \"Instances\": []\n },\n \"Security\": {\n   \"Flag\": true,\n   \"Instances\": [{\"Content\": \"' OR '1'='1'\", \"Reason\":\"Potential SQL Injection vulnerability\"}],\n  \"Summary\": \"Detected potential SQL injection attack in the input content.\"\n }\n}\n```"
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