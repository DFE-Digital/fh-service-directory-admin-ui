using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Html;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.staged;

public record CategoryDisplay(string Name, Category? Category);

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