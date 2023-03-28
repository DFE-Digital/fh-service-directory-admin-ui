using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using FamilyHubs.SharedKernel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static FamilyHubs.ServiceDirectoryAdminUi.Ui.Infrastructure.Configuration.PageConfiguration;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;
public class TypeOfServiceModel : PageModel
{
    private readonly IOrganisationAdminClientService _organisationAdminClientService;
    private readonly ITaxonomyService _taxonomyService;
    private readonly ISessionService _session;
    private readonly IRedisCacheService _redis;

    public TypeOfServiceModel(IOrganisationAdminClientService organisationAdminClientService,
                              ITaxonomyService taxonomyService,
                              ISessionService sessionService,
                              IRedisCacheService redisCacheService)
    {
        _organisationAdminClientService = organisationAdminClientService;
        _taxonomyService = taxonomyService;
        _session = sessionService;
        _redis = redisCacheService;
    }

    public string LastPage { get; set; } = default!;
    public string UserFlow { get; set; } = default!;
    public List<TaxonomyDto> TaxonomyRecords { get; private set; } = default!;
    public List<KeyValuePair<TaxonomyDto, List<TaxonomyDto>>> Categories { get; set; } = default!;
    [BindProperty]
    public List<long> CategorySelection { get; set; } = default!;
    [BindProperty]
    public List<long> SubcategorySelection { get; set; } = default!;

    public async Task OnGet()
    {
        LastPage = _redis.RetrieveLastPageName();
        UserFlow = _redis.RetrieveUserFlow();
        await GetCategoriesTreeAsync();
        var organisationViewModel = _redis.RetrieveOrganisationWithService() ?? new OrganisationViewModel();

        if (organisationViewModel != null && organisationViewModel.TaxonomySelection != null && organisationViewModel.TaxonomySelection.Any())
            GetCategoriesFromSelectedTaxonomiesAsync(organisationViewModel.TaxonomySelection);        

    }

    public async Task<IActionResult> OnPost()
    {
        await DeselectOrphanedSubcategories();

        if (CategorySelection.Count() == 0)
            ModelState.AddModelError(nameof(CategorySelection), "Select the support the service offers");
       
        if (CategorySelection.Count() > 0)
            await ValidateSubcategoryIsSelectedForCategory();

        if (!ModelState.IsValid)
        {
            await GetCategoriesTreeAsync();
            return Page();
        }
        
        var sessionVm = _redis.RetrieveOrganisationWithService() ?? new OrganisationViewModel();
        sessionVm.TaxonomySelection = GetSelectedTaxonomiesFromSelectedCategories();
        _redis?.StoreOrganisationWithService(sessionVm);

        if (_redis?.RetrieveLastPageName() == CheckServiceDetailsPageName)
            return RedirectToPage($"/OrganisationAdmin/{CheckServiceDetailsPageName}");
        
        return RedirectToPage("/OrganisationAdmin/ServiceDeliveryType");
    }

    private async Task DeselectOrphanedSubcategories()
    {
        await GetCategoriesTreeAsync();
        bool removeSubcat;
        var NewSubcategorySelection = new List<long>(SubcategorySelection);

        foreach (var subcat in SubcategorySelection)
        {
            removeSubcat = true;
            foreach (var parentCategory in Categories)
            {
                foreach (var subcategory in parentCategory.Value)
                {
                    if (subcategory.Id == subcat)
                    {
                        if (CategorySelection.Contains(parentCategory.Key.Id))
                            removeSubcat = false;
                    }
                }
            }
            if (removeSubcat)
                NewSubcategorySelection.Remove(subcat);
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

    private List<long>? GetSelectedTaxonomiesFromSelectedCategories()
    {
        var selectedTaxonomies = new List<long>();
        foreach (var category in CategorySelection)
        {
            selectedTaxonomies.Add(category);
        }
        foreach (var subcategory in SubcategorySelection)
        {
            selectedTaxonomies.Add(subcategory);
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
        PaginatedList<TaxonomyDto> taxonomies = _organisationAdminClientService.GetTaxonomyList(1, 9999).Result;
        CategorySelection = new List<long>();
        SubcategorySelection = new List<long>();

        if (taxonomies != null && selectedTaxonomies.Any())
        {
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
}