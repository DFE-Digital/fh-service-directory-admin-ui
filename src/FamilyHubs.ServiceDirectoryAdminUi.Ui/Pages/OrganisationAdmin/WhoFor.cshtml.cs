using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Options;
using Newtonsoft.Json;
using NuGet.Packaging;
using System.ComponentModel.DataAnnotations;
using static FamilyHubs.ServiceDirectoryAdminUi.Ui.Infrastructure.Configuration.PageConfiguration;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

public class WhoForModel : PageModel
{
    public string LastPage { get; set; } = default!;
    public string UserFlow { get; set; } = default!;

    private readonly ISessionService _session;

    [BindProperty, Required]
    public string Children { get; set; } = default!;

    [BindProperty]
    public string? SelectedMinAge { get; set; } = default!;

    [BindProperty]
    public string? SelectedMaxAge { get; set; } = default!;

    [BindProperty]
    public bool ValidationValid { get; set; } = true;

    [BindProperty]
    public bool OneOptionSelected { get; set; } = true;

    public bool AgeRangeSelected { get; set; } = true;

    public bool ValidAgeRange { get; set; } = true;

    public List<SelectListItem> AgeRange { get; set; }

    public WhoForModel(ISessionService sessionService)
    {
        _session = sessionService;
    }

    public void OnGet(string strOrganisationViewModel)
    {
        LastPage = _session.RetrieveLastPageName(HttpContext);
        UserFlow = _session.RetrieveUserFlow(HttpContext);

        /*** Using Session storage as a service ***/
        var organisationViewModel = _session.RetrieveOrganisationWithService(HttpContext);
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
        InitializeAgeRange();
    }

    public IActionResult OnPost()
    {
        /*** Using Session storage as a service ***/
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

        var organisationViewModel = _session.RetrieveOrganisationWithService(HttpContext);

        if (Children == "Yes")
        {
            if (int.TryParse(SelectedMinAge, out int minAge))
            {
                organisationViewModel.MinAge = minAge;
            }

            if (int.TryParse(SelectedMaxAge, out int maxAge))
            {
                organisationViewModel.MaxAge = maxAge;
            }

            if (organisationViewModel.WhoForSelection != null && organisationViewModel.WhoForSelection.Any())
            {
                organisationViewModel.WhoForSelection.Add("Children");
            }
            else
            {
                organisationViewModel.WhoForSelection = new List<string>();
                organisationViewModel.WhoForSelection.Add("Children");
            }
        }

        organisationViewModel.Children = Children;

        _session.StoreOrganisationWithService(HttpContext, organisationViewModel);

        if (_session.RetrieveLastPageName(HttpContext) == CheckServiceDetailsPageName)
        {
            return RedirectToPage($"/OrganisationAdmin/{CheckServiceDetailsPageName}");
        }
        return RedirectToPage("/OrganisationAdmin/WhatLanguage");

    }

    private void InitializeAgeRange()
    {
        AgeRange = new List<SelectListItem>() {
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