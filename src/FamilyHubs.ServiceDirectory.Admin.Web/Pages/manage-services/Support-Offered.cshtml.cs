using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public class SupportOfferedUserInput
{
    public List<long?> SelectedCategories { get; set; } = new List<long?>();
    public List<long> SelectedSubCategories { get; set; } = new List<long>();
    public string ErrorElementId { get; set; } = string.Empty;
}

public class Support_OfferedModel : ServicePageModel<SupportOfferedUserInput>
{
    private readonly ITaxonomyService _taxonomyService;
    private readonly IServiceDirectoryClient _serviceDirectoryClient;

    public List<KeyValuePair<TaxonomyDto, List<TaxonomyDto>>> Taxonomies { get; set; } = new();

    [BindProperty]
    public SupportOfferedUserInput UserInput { get; set; } = new();

    public string? ServiceName { get; set; }

    public Support_OfferedModel(IRequestDistributedCache connectionRequestCache, ITaxonomyService taxonomyService, IServiceDirectoryClient serviceDirectoryClient)
            : base(ServiceJourneyPage.Support_Offered, connectionRequestCache)
    {
        _taxonomyService = taxonomyService;
        _serviceDirectoryClient = serviceDirectoryClient;
    }

    protected override async Task OnGetWithModelAsync(CancellationToken cancellationToken)
    {
        Taxonomies = await _taxonomyService.GetCategories();

        if (Errors.HasErrors)
        {
            UserInput = ServiceModel!.UserInput!;
            return;
        }

        switch (Flow)
        {
            case JourneyFlow.Edit:
                if (ServiceId != null)
                {
                    var service = await _serviceDirectoryClient.GetServiceById(ServiceId!.Value, cancellationToken);
                    ServiceName = service.Name;
                    UserInput.SelectedCategories = service.Taxonomies.Select(x => x.ParentId).Distinct().ToList();
                    UserInput.SelectedSubCategories = service.Taxonomies.Select(x => x.Id).ToList();
                }

                break;
            default:
                ServiceName = ServiceModel!.Name;
                UserInput.SelectedCategories = ServiceModel!.SelectedCategories;
                UserInput.SelectedSubCategories = ServiceModel.SelectedSubCategories;
                break;
        }
    }
    protected override async Task<IActionResult> OnPostWithModelAsync(CancellationToken cancellationToken)
    {
        Taxonomies = await _taxonomyService.GetCategories();

        //no selection 
        if (UserInput.SelectedCategories.Count == 0 && UserInput.SelectedSubCategories.Count == 0)
        {
            var category = Taxonomies[0].Key.Id;
            UserInput.ErrorElementId = $"category-{category}";
            return RedirectToSelf(UserInput, ErrorId.Support_Offered__SelectCategory);
        }

        //no sub category selection 
        if (UserInput.SelectedCategories.Count > 0)
        {
            foreach (var category in UserInput.SelectedCategories)
            {
                var possibleSubCategories = Taxonomies.First(x => x.Key.Id == category).Value;
                var possibleSubCategoryIds = possibleSubCategories.Select(x => x.Id).ToList();
                var hasSelection = possibleSubCategoryIds.Intersect(UserInput.SelectedSubCategories).Any();
                if (!hasSelection)
                {
                    long subCategoryId = possibleSubCategoryIds[0];
                    UserInput.ErrorElementId = $"category-{subCategoryId}";
                    return RedirectToSelf(UserInput, ErrorId.Support_Offered__SelectSubCategory);
                }
            }
        }

        switch (Flow)
        {
            case JourneyFlow.Edit:
                await UpdateTaxonomies(UserInput.SelectedSubCategories, cancellationToken);
                break;

            default:
                ServiceModel!.SelectedCategories = UserInput.SelectedCategories;
                ServiceModel.SelectedSubCategories = UserInput.SelectedSubCategories;
                break;
        }


        return NextPage();
    }

    private async Task UpdateTaxonomies(List<long> selectedTaxonomyIds, CancellationToken cancellationToken)
    {
        var service = await _serviceDirectoryClient.GetServiceById(ServiceId!.Value, cancellationToken);
        var taxonomies = await _serviceDirectoryClient.GetTaxonomyList(1, 999999);

        var selectedTaxonomies = taxonomies.Items.Where(x => selectedTaxonomyIds.Contains(x.Id)).ToList();

        service.Taxonomies = selectedTaxonomies;

        await _serviceDirectoryClient.UpdateService(service, cancellationToken);
    }
}