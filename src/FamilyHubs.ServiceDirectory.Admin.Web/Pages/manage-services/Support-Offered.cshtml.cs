using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public class SupportOfferedUserInput
{
    public List<long?> SelectedCategories { get; set; } = new();
    public List<long> SelectedSubCategories { get; set; } = new();
    public string ErrorElementId { get; set; } = string.Empty;
    public bool IsCategoryError { get; set; }
    public long SubCategoryErrorGroupId { get; set; }
}

public class Support_OfferedModel : ServicePageModel<SupportOfferedUserInput>
{
    private readonly ITaxonomyService _taxonomyService;

    public List<KeyValuePair<TaxonomyDto, List<TaxonomyDto>>> Taxonomies { get; set; } = new();

    [BindProperty]
    public SupportOfferedUserInput UserInput { get; set; } = new();

    public string? ServiceName { get; set; }

    public Support_OfferedModel(
        IRequestDistributedCache connectionRequestCache,
        ITaxonomyService taxonomyService)
            : base(ServiceJourneyPage.Support_Offered, connectionRequestCache)
    {
        _taxonomyService = taxonomyService;
    }

    protected override async Task OnGetWithErrorAsync(CancellationToken cancellationToken)
    {
        Taxonomies = await _taxonomyService.GetCategories(cancellationToken);

        UserInput = ServiceModel!.UserInput!;
    }

    protected override async Task OnGetWithModelAsync(CancellationToken cancellationToken)
    {
        Taxonomies = await _taxonomyService.GetCategories(cancellationToken);

        ServiceName = ServiceModel!.Name;
        UserInput.SelectedCategories = ServiceModel!.SelectedCategories;
        UserInput.SelectedSubCategories = ServiceModel.SelectedSubCategories;
    }

    protected override async Task<IActionResult> OnPostWithModelAsync(CancellationToken cancellationToken)
    {
        Taxonomies = await _taxonomyService.GetCategories(cancellationToken);

        //no selection 
        if (UserInput.SelectedCategories.Count == 0 && UserInput.SelectedSubCategories.Count == 0)
        {
            var category = Taxonomies[0].Key.Id;
            UserInput.ErrorElementId = $"category-{category}";
            UserInput.IsCategoryError = true;
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
                    UserInput.SubCategoryErrorGroupId = category!.Value;
                    return RedirectToSelf(UserInput, ErrorId.Support_Offered__SelectSubCategory);
                }
            }
        }

        ServiceModel!.SelectedCategories = UserInput.SelectedCategories;
        ServiceModel.SelectedSubCategories = UserInput.SelectedSubCategories;

        return NextPage();
    }
}