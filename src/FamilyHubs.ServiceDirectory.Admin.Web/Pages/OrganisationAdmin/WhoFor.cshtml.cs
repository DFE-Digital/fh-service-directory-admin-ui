using System.ComponentModel.DataAnnotations;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using static FamilyHubs.ServiceDirectory.Admin.Core.Constants.PageConfiguration;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.OrganisationAdmin;

public class WhoForModel : PageModel
{
    public string LastPage { get; set; } = default!;
    public string UserFlow { get; set; } = default!;

    private readonly ICacheService _cacheService;

    [BindProperty, Required]
    public string Children { get; set; } = default!;

    [BindProperty]
    public string? SelectedMinAge { get; set; }

    [BindProperty]
    public string? SelectedMaxAge { get; set; }

    [BindProperty]
    public bool ValidationValid { get; set; } = true;

    [BindProperty]
    public bool OneOptionSelected { get; set; } = true;

    public bool AgeRangeSelected { get; set; } = true;

    public bool ValidAgeRange { get; set; } = true;

    public List<SelectListItem> AgeRange { get; set; } = default!;

    public WhoForModel(
        ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public void OnGet(string strOrganisationViewModel)
    {
        LastPage = _cacheService.RetrieveLastPageName();
        UserFlow = _cacheService.RetrieveUserFlow();

        var organisationViewModel = _cacheService.RetrieveOrganisationWithService();

        if (organisationViewModel != null)
        {
            if (!string.IsNullOrEmpty(organisationViewModel.Children))
                Children = organisationViewModel.Children;

            if (organisationViewModel.MinAge != null)
            {
                SelectedMinAge = organisationViewModel.MinAge.Value.ToString();
            }
            if (organisationViewModel.MaxAge != null)
            {
                SelectedMaxAge = organisationViewModel.MaxAge.Value.ToString();
            }
        }
        InitialiseAgeRange();

    }

    public IActionResult OnPost()
    {
        if (Children != "Yes" && Children != "No")
        {
            ModelState.AddModelError("Select One Option", "Please select one option");
            ValidationValid = false;
            OneOptionSelected = false;
            return Page();
        }

        if (Children == "Yes" && (string.IsNullOrWhiteSpace(SelectedMinAge) || string.IsNullOrWhiteSpace(SelectedMaxAge)))
        {
            ModelState.AddModelError("Select Age Range", "Please select age range");
            AgeRangeSelected = false;
            ValidationValid = false;
            InitialiseAgeRange();
            return Page();
        }

        if (Children == "Yes" && !string.IsNullOrWhiteSpace(SelectedMinAge) && !string.IsNullOrWhiteSpace(SelectedMaxAge) && int.Parse(SelectedMinAge) >= int.Parse(SelectedMaxAge))
        {
            ModelState.AddModelError("Age Range Invalid", "Please select a different age range");
            ValidAgeRange = false;
            ValidationValid = false;
            InitialiseAgeRange();
            return Page();
        }

        var organisationViewModel = _cacheService.RetrieveOrganisationWithService();

        if (organisationViewModel != null)
        {
            if (int.TryParse(SelectedMinAge, out var minAge))
            {
                organisationViewModel.MinAge = minAge;
            }

            if (int.TryParse(SelectedMaxAge, out var maxAge))
            {
                organisationViewModel.MaxAge = maxAge;
            }
        }

        if (Children == "Yes")
        {
            if (organisationViewModel?.WhoForSelection != null && organisationViewModel.WhoForSelection.Any())
            {
                if (!organisationViewModel.WhoForSelection.Contains("Children"))
                    organisationViewModel.WhoForSelection.Add("Children");
            }
            else
            {
                if (organisationViewModel != null)
                {
                    organisationViewModel.WhoForSelection = new List<string> { "Children" };
                }
            }
        }
        else
        {
            if (organisationViewModel?.WhoForSelection != null && organisationViewModel.WhoForSelection.Any())
            {
                if (organisationViewModel.WhoForSelection.Contains("Children"))
                    organisationViewModel.WhoForSelection.Remove("Children");
            }
        }

        if (organisationViewModel != null)
        {
            organisationViewModel.Children = Children;
            _cacheService.StoreOrganisationWithService(organisationViewModel);
        }

        return RedirectToPage(_cacheService.RetrieveLastPageName() == CheckServiceDetailsPageName
            ? $"/OrganisationAdmin/{CheckServiceDetailsPageName}"
            : "/OrganisationAdmin/WhatLanguage");
    }

    private void InitialiseAgeRange()
    {
        AgeRange = new List<SelectListItem>
        {
            new SelectListItem{ Value="0", Text="0 to 12 months" },
            new SelectListItem{ Value="1", Text="1 year old"},
            new SelectListItem{ Value="2", Text="2 years old"},
            new SelectListItem{ Value="3", Text="3 years old"},
            new SelectListItem{ Value="4", Text="4 years old"},
            new SelectListItem{ Value="5", Text="5 years old"},
            new SelectListItem{ Value="6", Text="6 years old"},
            new SelectListItem{ Value="7", Text="7 years old"},
            new SelectListItem{ Value="8", Text="8 years old"},
            new SelectListItem{ Value="9", Text="9 years old"},
            new SelectListItem{ Value="10", Text="10 years old"},
            new SelectListItem{ Value="11", Text="11 years old"},
            new SelectListItem{ Value="12", Text="12 years old"},
            new SelectListItem{ Value="13", Text="13 years old"},
            new SelectListItem{ Value="14", Text="14 years old"},
            new SelectListItem{ Value="15", Text="15 years old"},
            new SelectListItem{ Value="16", Text="16 years old"},
            new SelectListItem{ Value="17", Text="17 years old"},
            new SelectListItem{ Value="18", Text="18 years old"},
            new SelectListItem{ Value="19", Text="19 years old"},
            new SelectListItem{ Value="20", Text="20 years old"},
            new SelectListItem{ Value="21", Text="21 years old"},
            new SelectListItem{ Value="22", Text="22 years old"},
            new SelectListItem{ Value="23", Text="23 years old"},
            new SelectListItem{ Value="24", Text="24 years old"},
            new SelectListItem{ Value="25", Text="25 years old"}
        };
    }
}