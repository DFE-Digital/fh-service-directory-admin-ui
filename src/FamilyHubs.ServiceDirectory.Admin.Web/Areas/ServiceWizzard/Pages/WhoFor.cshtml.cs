using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.ServiceWizzard.Pages;

public class WhoForModel : BasePageModel
{
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

    public WhoForModel(IRequestDistributedCache requestCache)
        : base(requestCache)
    {
    }
    public async Task OnGet()
    {
        OrganisationViewModel? viewModel = await GetOrganisationViewModel();
        if (viewModel != null)
        {
            if (!string.IsNullOrEmpty(viewModel.Children))
                Children = viewModel.Children;

            if (viewModel.MinAge != null)
            {
                SelectedMinAge = viewModel.MinAge.Value.ToString();
            }
            if (viewModel.MaxAge != null)
            {
                SelectedMaxAge = viewModel.MaxAge.Value.ToString();
            }
        }
        InitializeAgeRange();
    }

    public async Task<IActionResult> OnPost()
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
            InitializeAgeRange();
            return Page();
        }

        if (Children == "Yes" && !string.IsNullOrWhiteSpace(SelectedMinAge) && !string.IsNullOrWhiteSpace(SelectedMaxAge) && Int32.Parse(SelectedMinAge) >= Int32.Parse(SelectedMaxAge))
        {
            ModelState.AddModelError("Age Range Invalid", "Please select a different age range");
            ValidAgeRange = false;
            ValidationValid = false;
            InitializeAgeRange();
            return Page();
        }

        OrganisationViewModel? viewModel = await GetOrganisationViewModel();
        if (viewModel == null) 
        {
            viewModel = new OrganisationViewModel();
        }

        if (int.TryParse(SelectedMinAge, out var minAge))
        {
            viewModel.MinAge = minAge;
        }

        if (int.TryParse(SelectedMaxAge, out var maxAge))
        {
            viewModel.MaxAge = maxAge;
        }

        if (Children == "Yes")
        {
            if (viewModel?.WhoForSelection != null && viewModel.WhoForSelection.Any())
            {
                if (!viewModel.WhoForSelection.Contains("Children"))
                    viewModel.WhoForSelection.Add("Children");
            }
            else
            {
#pragma warning disable CS8602 
                viewModel.WhoForSelection = new List<string>()
                {
                    "Children"
                };
#pragma warning restore CS8602 
            }
        }
        else
        {
            if (viewModel?.WhoForSelection != null && viewModel.WhoForSelection.Any() && viewModel.WhoForSelection.Contains("Children"))
            {
                viewModel.WhoForSelection.Remove("Children");
            }
        }
#pragma warning disable CS8602 
        viewModel.Children = Children;
#pragma warning restore CS8602 
        await SetCacheAsync(viewModel);

        if (string.Compare(await GetLastPage(), "/CheckServiceDetails", StringComparison.OrdinalIgnoreCase) == 0)
        {
            return RedirectToPage("CheckServiceDetails", new { area = "ServiceWizzard" });
        }

        return RedirectToPage("WhatLanguage", new { area = "ServiceWizzard" });

    }

    private void InitializeAgeRange()
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
            new SelectListItem{ Value="25", Text="25 years old"},
        };
    }
}
