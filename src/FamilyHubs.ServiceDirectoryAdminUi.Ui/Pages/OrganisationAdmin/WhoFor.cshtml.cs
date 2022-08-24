using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

public class WhoForModel : PageModel
{
    [BindProperty]
    public List<string> WhoForSelection { get; set; } = default!;

    [BindProperty]
    public string SelectedMinAge { get; set; } = default!;

    [BindProperty]
    public string SelectedMaxAge { get; set; } = default!;

    [BindProperty]
    public string? StrOrganisationViewModel { get; set; }

    public void OnGet(string strOrganisationViewModel)
    {
        StrOrganisationViewModel = strOrganisationViewModel;

        var organisationViewModel = JsonConvert.DeserializeObject<OrganisationViewModel>(StrOrganisationViewModel);
        if (organisationViewModel != null)
        {
            if (organisationViewModel.WhoForSelection != null && organisationViewModel.WhoForSelection.Any())
                WhoForSelection = organisationViewModel.WhoForSelection;
            if (organisationViewModel.MinAge != null)
            {
                SelectedMinAge = organisationViewModel.MinAge.Value.ToString();
            }
            if (organisationViewModel.MaxAge != null)
            {
                SelectedMaxAge = organisationViewModel.MaxAge.Value.ToString();
            }
        }
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid || string.IsNullOrEmpty(StrOrganisationViewModel))
        {
            return Page();
        }

        var organisationViewModel = JsonConvert.DeserializeObject<OrganisationViewModel>(StrOrganisationViewModel ?? "");
        if (organisationViewModel == null)
        {
            return Page();
        }

        if (int.TryParse(SelectedMinAge, out int minAge))
        {
            organisationViewModel.MinAge = minAge;
        }

        if (int.TryParse(SelectedMaxAge, out int maxAge))
        {
            organisationViewModel.MaxAge = maxAge;
        }

        organisationViewModel.WhoForSelection = new List<string>(WhoForSelection);

        StrOrganisationViewModel = JsonConvert.SerializeObject(organisationViewModel);

        return RedirectToPage("/OrganisationAdmin/WhatLanguage", new
        {
            strOrganisationViewModel = StrOrganisationViewModel
        });
    }
}
