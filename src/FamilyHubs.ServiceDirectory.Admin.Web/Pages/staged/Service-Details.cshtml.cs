using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Html;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.staged;

public record CategoryDisplay(string Name, Category? Category);

//todo: componentise service search result and render on details page
// alternatively render from real site in an iframe?

[Authorize(Roles = RoleGroups.AdminRole)]
public class Service_DetailsModel : HeaderPageModel
{
    private readonly IServiceDirectoryClient _serviceDirectoryClient;
    private readonly IAiClient _aiClient;

    public ServiceDto? Service { get; set; }
    public HtmlString HighlightedDescription { get; set; }
    public ContentCheckResponse ContentCheckResponse { get; set; }

    public Service_DetailsModel(
        IServiceDirectoryClient serviceDirectoryClient,
        IAiClient aiClient)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
        _aiClient = aiClient;
    }

    public async Task OnGetAsync(
        CancellationToken cancellationToken,
        long serviceId)
    {
        Service = await _serviceDirectoryClient.GetServiceById(serviceId, cancellationToken);

        if (!string.IsNullOrEmpty(Service.Description))
        {
            ContentCheckResponse = await _aiClient.Call(Service.Description, cancellationToken);

            //todo: doesn't highlight all categories
            //todo: handle repeat/overlapping text by only having one span per region??
            HighlightedDescription = HighlightDescription(Service.Description,
                ContentCheckResponse.GrammarAndSpelling,
                ContentCheckResponse.InappropriateLanguage,
                ContentCheckResponse.Security,
                ContentCheckResponse.PoliticisedSentiment,
                ContentCheckResponse.PII,
                ContentCheckResponse.GrammarAndSpelling,
                ContentCheckResponse.StyleViolations);
        }
    }

    private static HtmlString HighlightDescription(string description, params Category?[] categories)
    {
        string highlightedDescription = description;

        foreach (var instance in categories.SelectMany(c => c?.Instances ?? Enumerable.Empty<Instance>()))
        {
            highlightedDescription = highlightedDescription.Replace(
                instance.Content,
                $"<span class=\"highlight\">{instance.Content}</span>");
        }

        return new HtmlString(highlightedDescription);
    }
}