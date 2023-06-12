using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static FamilyHubs.ServiceDirectory.Admin.Core.Constants.PageConfiguration;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.OrganisationAdmin.Pages;
public class TypeOfServiceModel : PageModel
{
    private readonly IServiceDirectoryClient _serviceDirectoryClient;
    private readonly ITaxonomyService _taxonomyService;
    private readonly ICacheService _cacheService;

    public TypeOfServiceModel(
        IServiceDirectoryClient serviceDirectoryClient,
        ITaxonomyService taxonomyService,
        ICacheService cacheService)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
        _taxonomyService = taxonomyService;
        _cacheService = cacheService;
    }

    public string LastPage { get; set; } = default!;
    public string UserFlow { get; set; } = default!;
    public List<KeyValuePair<TaxonomyDto, List<TaxonomyDto>>> Categories { get; set; } = default!;
    [BindProperty]
    public List<long> CategorySelection { get; set; } = default!;
    [BindProperty]
    public List<long> SubcategorySelection { get; set; } = default!;

    public async Task OnGet()
    {
        LastPage = await _cacheService.RetrieveLastPageName();
        UserFlow = await _cacheService.RetrieveUserFlow();
        await GetCategoriesTreeAsync();
        var organisationViewModel = await _cacheService.RetrieveOrganisationWithService() ?? new OrganisationViewModel();

        if (organisationViewModel.TaxonomySelection != null && organisationViewModel.TaxonomySelection.Any())
            GetCategoriesFromSelectedTaxonomiesAsync(organisationViewModel.TaxonomySelection);        

    }

    public async Task<IActionResult> OnPost()
    {
        await DeselectOrphanedSubcategories();

        if (CategorySelection.Count == 0)
            ModelState.AddModelError(nameof(CategorySelection), "Select the support the service offers");
       
        if (CategorySelection.Count > 0)
            await ValidateSubcategoryIsSelectedForCategory();

        if (!ModelState.IsValid)
        {
            await GetCategoriesTreeAsync();
            return Page();
        }
        
        var sessionVm = await _cacheService.RetrieveOrganisationWithService() ?? new OrganisationViewModel();
        sessionVm.TaxonomySelection = GetSelectedTaxonomiesFromSelectedCategories();
        await _cacheService.StoreOrganisationWithService(sessionVm);

        return RedirectToPage(await _cacheService.RetrieveLastPageName() == CheckServiceDetailsPageName ? 
            $"/OrganisationAdmin/{CheckServiceDetailsPageName}" 
            : "/OrganisationAdmin/ServiceDeliveryType");
    }

    private async Task DeselectOrphanedSubcategories()
    {
        await GetCategoriesTreeAsync();
        var newSubcategorySelection = new List<long>(SubcategorySelection);

        foreach (var subCategory in SubcategorySelection)
        {
            var removeSubCategory = true;
            foreach (var parentCategory in Categories)
            {
                foreach (var subcategory in parentCategory.Value)
                {
                    if (subcategory.Id == subCategory)
                    {
                        if (CategorySelection.Contains(parentCategory.Key.Id))
                            removeSubCategory = false;
                    }
                }
            }
            if (removeSubCategory)
                newSubcategorySelection.Remove(subCategory);
        }

        SubcategorySelection = newSubcategorySelection;
    }

    private async Task ValidateSubcategoryIsSelectedForCategory()
    {
        await GetCategoriesTreeAsync();

        foreach (var cat in CategorySelection)
        {
            var error = true;
            foreach (var parentCategory in Categories)
            {
                if (parentCategory.Key.Id == cat)
                {
                    foreach (var subcategory in parentCategory.Value)
                    {
                        if (SubcategorySelection.Contains(subcategory.Id))
                            error = false;
                    }
                }
            }
            
            if (error)
                ModelState.AddModelError(nameof(CategorySelection), "Select name of sub-category support");
        }
    }

    private List<long> GetSelectedTaxonomiesFromSelectedCategories()
    {
        var selectedTaxonomies = CategorySelection.ToList();
        selectedTaxonomies.AddRange(SubcategorySelection);
        return selectedTaxonomies;
    }

    private async Task GetCategoriesTreeAsync()
    {
        var categories = await _taxonomyService.GetCategories();

        Categories = new List<KeyValuePair<TaxonomyDto, List<TaxonomyDto>>>(categories);
    }

    private void GetCategoriesFromSelectedTaxonomiesAsync(List<long> selectedTaxonomies)
    {
        var taxonomies = _serviceDirectoryClient.GetTaxonomyList(1, 9999).Result;
        CategorySelection = new List<long>();
        SubcategorySelection = new List<long>();

        foreach (var taxonomyKey in selectedTaxonomies)
        {
            var taxonomy = taxonomies.Items.FirstOrDefault(x => x.Id == taxonomyKey);

            if (taxonomy != null && !taxonomy.ParentId.HasValue)
                CategorySelection.Add(taxonomy.Id);
            else if (taxonomy != null && taxonomy.ParentId.HasValue)
                SubcategorySelection.Add(taxonomy.Id);
        }
    }
}