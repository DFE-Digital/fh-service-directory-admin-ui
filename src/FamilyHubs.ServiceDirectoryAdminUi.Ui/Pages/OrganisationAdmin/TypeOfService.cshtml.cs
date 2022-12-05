using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralTaxonomys;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using FamilyHubs.SharedKernel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using static FamilyHubs.ServiceDirectoryAdminUi.Ui.Infrastructure.Configuration.PageConfiguration;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

[Authorize(Policy = "ServiceMaintainer")]
public class TypeOfServiceModel : PageModel
{
    private readonly IOpenReferralOrganisationAdminClientService _openReferralOrganisationAdminClientService;
    private readonly ITaxonomyService _taxonomyService;
    private readonly ISessionService _session;
    private readonly IRedisCacheService _redis;

    public TypeOfServiceModel(IOpenReferralOrganisationAdminClientService openReferralOrganisationAdminClientService,
                              ITaxonomyService taxonomyService,
                              ISessionService sessionService,
                              IRedisCacheService redisCacheService)
    {
        _openReferralOrganisationAdminClientService = openReferralOrganisationAdminClientService;
        _taxonomyService = taxonomyService;
        _session = sessionService;
        _redis = redisCacheService;
    }

    public string LastPage { get; set; } = default!;
    public string UserFlow { get; set; } = default!;
    public List<OpenReferralTaxonomyDto> OpenReferralTaxonomyRecords { get; private set; } = default!;
    public List<KeyValuePair<OpenReferralTaxonomyDto, List<OpenReferralTaxonomyDto>>> Categories { get; set; } = default!;
    [BindProperty]
    public List<string> CategorySelection { get; set; } = default!;
    [BindProperty]
    public List<string> SubcategorySelection { get; set; } = default!;

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
        if (CategorySelection.Count() == 0)
            ModelState.AddModelError(nameof(CategorySelection), "Please select one option");
        
        if (SubcategorySelection.Count() == 0)
            ModelState.AddModelError(nameof(SubcategorySelection), "Please select subcategory");

        if (CategorySelection.Count() > 0 && SubcategorySelection.Count() > 0)
            await ValidateCategoryIsSelectedForSubCategory();

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

    private async Task ValidateSubcategoryIsSelectedForCategory()
    {
        await GetCategoriesTreeAsync();
        string parentCat = string.Empty;
        bool error = true;

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
        }

        if (error)
            ModelState.AddModelError(nameof(CategorySelection), "Please select a sub-category for each category");
    }

    private List<string>? GetSelectedTaxonomiesFromSelectedCategories()
    {
        var selectedTaxonomies = new List<string>();
        foreach (string category in CategorySelection)
        {
            selectedTaxonomies.Add(category);
        }
        foreach (string subcategory in SubcategorySelection)
        {
            selectedTaxonomies.Add(subcategory);
        }
        return selectedTaxonomies;
    }

    private async Task GetCategoriesTreeAsync()
    {
        List<KeyValuePair<OpenReferralTaxonomyDto, List<OpenReferralTaxonomyDto>>> categories = await _taxonomyService.GetCategories();

        if (categories != null)
            Categories = new List<KeyValuePair<OpenReferralTaxonomyDto, List<OpenReferralTaxonomyDto>>>(categories);
    }

    private void GetCategoriesFromSelectedTaxonomiesAsync(List<string> selectedTaxonomies)
    {
        PaginatedList<OpenReferralTaxonomyDto> taxonomies = _openReferralOrganisationAdminClientService.GetTaxonomyList(1, 9999).Result;
        CategorySelection = new List<string>();
        SubcategorySelection = new List<string>();

        if (taxonomies != null && selectedTaxonomies.Any())
        {
            foreach (string taxonomyKey in selectedTaxonomies)
            {
                OpenReferralTaxonomyDto? taxonomy = taxonomies.Items.FirstOrDefault(x => x.Id == taxonomyKey);

                if (taxonomy != null && string.IsNullOrEmpty(taxonomy.Parent))
                    CategorySelection.Add(taxonomy.Id);
                else if (taxonomy != null && !string.IsNullOrEmpty(taxonomy.Parent))
                    SubcategorySelection.Add(taxonomy.Id);
            }
        }
    }
}