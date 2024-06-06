using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.ServiceValidators;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Html;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.staged;

public record CategoryDisplay(string Name, Category? Category);
public record CategoryInstanceDisplay(string CategoryName, Instance Instance, int Ordinal);

//todo: work on multiple fields
//todo: in prod version, checks will be done as a batch process (online as well when interacting) and the details saved to a service meta table

[Authorize(Roles = RoleGroups.AdminRole)]
public class Service_DetailsModel : HeaderPageModel
{
    private readonly IServiceDirectoryClient _serviceDirectoryClient;
    private readonly IAiClient _aiClient;
    private readonly IServiceRenderChecker _serviceRenderChecker;

    public ServiceDto? Service { get; set; }
    public HtmlString HighlightedDescription { get; set; }
    public List<RenderCheckResult> RenderCheckResults;
    public ContentCheckResponse ContentCheckResponse { get; set; }

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
        long serviceId)
    {
        // in prod version, do render checks and ai checks in parallel (although will be a batch process, so not too important)

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

        //todo: need to support overlapping instances
        foreach (var instance in categories.SelectMany(c => c?.Instances ?? Enumerable.Empty<Instance>())
                     .Where(i => !string.IsNullOrEmpty(i.Content)))
        {
            highlightedDescription = highlightedDescription.Replace(
                instance.Content,
                $"<span class=\"highlight\">{instance.Content}</span>");
        }

        return new HtmlString(highlightedDescription);
    }
}