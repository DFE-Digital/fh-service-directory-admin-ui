using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Models;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.ServiceWizzard.Pages;

public class TypeOfSupportModel : BasePageModel
{
    private readonly IServiceDirectoryClient _serviceDirectoryClient;
    private readonly ITaxonomyService _taxonomyService;

    public List<KeyValuePair<TaxonomyDto, List<TaxonomyDto>>> Categories { get; set; } = default!;
    [BindProperty]
    public List<string> CategorySelection { get; set; } = default!;
    [BindProperty]
    public List<string> SubcategorySelection { get; set; } = default!;

    public TypeOfSupportModel(IRequestDistributedCache requestCache, IServiceDirectoryClient serviceDirectoryClient, ITaxonomyService taxonomyService)
        : base(requestCache)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
        _taxonomyService = taxonomyService;
    }
    public async Task OnGet()
    {
        await GetCategoriesTreeAsync();

        OrganisationViewModel? viewModel = await GetOrganisationViewModel();
        if (viewModel != null && viewModel.TaxonomySelection != null)
        {
            GetCategoriesFromSelectedTaxonomiesAsync(viewModel.TaxonomySelection);
        } 
    }

    public async Task<IActionResult> OnPost()
    {
        await DeselectOrphanedSubcategories();

        if (!CategorySelection.Any())
            ModelState.AddModelError(nameof(CategorySelection), "Select the support the service offers");
        else
            await ValidateSubcategoryIsSelectedForCategory();

        if (!ModelState.IsValid)
        {
            await GetCategoriesTreeAsync();
            return Page();
        }

        OrganisationViewModel? viewModel = await GetOrganisationViewModel();
        if (viewModel == null)
        {
            viewModel = new OrganisationViewModel();
        }
        viewModel.TaxonomySelection = GetSelectedTaxonomiesFromSelectedCategories();

        await SetCacheAsync(viewModel);

        if (string.Compare(await GetLastPage(), "/CheckServiceDetails", StringComparison.OrdinalIgnoreCase) == 0)
        {
            return RedirectToPage("CheckServiceDetails", new { area = "ServiceWizzard" });
        }

        return RedirectToPage("WhoFor", new { area = "ServiceWizzard" });
    }

    private List<long>? GetSelectedTaxonomiesFromSelectedCategories()
    {
        var selectedTaxonomies = new List<long>();
        foreach (var category in CategorySelection)
        {
            if (long.TryParse(category, out long categoryId))
            {
                selectedTaxonomies.Add(categoryId);
            } 
        }
        foreach (var subcategory in SubcategorySelection)
        {
            if (long.TryParse(subcategory, out long subcategoryId))
            {
                selectedTaxonomies.Add(subcategoryId);
            }
        }
        return selectedTaxonomies;
    }

    private async Task GetCategoriesTreeAsync()
    {
        List<KeyValuePair<TaxonomyDto, List<TaxonomyDto>>> categories = await _taxonomyService.GetCategories();

        if (categories != null)
            Categories = new List<KeyValuePair<TaxonomyDto, List<TaxonomyDto>>>(categories);
    }

    private void GetCategoriesFromSelectedTaxonomiesAsync(List<long> selectedTaxonomies)
    {
        PaginatedList<TaxonomyDto> taxonomies = _serviceDirectoryClient.GetTaxonomyList(1, 9999).Result;
        CategorySelection = new List<string>();
        SubcategorySelection = new List<string>();

        if (taxonomies != null && selectedTaxonomies.Any())
        {
            foreach (var taxonomyKey in selectedTaxonomies)
            {
                var taxonomy = taxonomies.Items.Find(x => x.Id == taxonomyKey);

                if (taxonomy != null && taxonomy.ParentId == null)
                    CategorySelection.Add(taxonomy.Id.ToString());
                else if (taxonomy != null && taxonomy.ParentId != null)
                    SubcategorySelection.Add(taxonomy.Id.ToString());
            }
        }
    }

    private async Task DeselectOrphanedSubcategories()
    {
        await GetCategoriesTreeAsync();
        bool removeSubcat;
        List<string> NewSubcategorySelection = new List<string>(SubcategorySelection);

        foreach (var subcat in SubcategorySelection)
        {
            if (long.TryParse(subcat, out long taxonomyId))
            {
                removeSubcat = true;
                foreach (var parentCategory in Categories)
                {
                    foreach (var subcategory in parentCategory.Value)
                    {
                        if (subcategory.Id == taxonomyId && CategorySelection.Contains(parentCategory.Key.Id.ToString()))
                        {
                            removeSubcat = false;
                        }
                    }
                }
                if (removeSubcat)
                    NewSubcategorySelection.Remove(subcat);
            }
                
        }

        SubcategorySelection = NewSubcategorySelection;
    }

    private async Task ValidateSubcategoryIsSelectedForCategory()
    {
        await GetCategoriesTreeAsync();
        bool error;

        foreach (var cat in CategorySelection)
        {
            error = true;
            foreach (var parentCategory in Categories)
            {
                if (long.TryParse(cat, out long taxonomyId) && parentCategory.Key.Id == taxonomyId)
                {
#pragma warning disable S3267
                    foreach (var subcategory in parentCategory.Value)
                    {
                        if (SubcategorySelection.Contains(subcategory.Id.ToString()))
                            error = false;
                    }
#pragma warning restore S3267
                }
            }

            if (error)
                ModelState.AddModelError(nameof(CategorySelection), "Select name of sub-category support");
        }
    }
}
