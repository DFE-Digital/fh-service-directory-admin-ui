using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

public class OfferAtFamiliesPlaceModel : PageModel
{
    [BindProperty]
    public string Familychoice { get; set; } = default!;

    [BindProperty]
    public OrganisationViewModel OrganisationViewModel { get; set; } = new OrganisationViewModel();

    [BindProperty]
    public string? StrOrganisationViewModel { get; set; }

    [BindProperty]
    public bool ValidationValid { get; set; } = true;

    public void OnGet(string strOrganisationViewModel)
    {
        StrOrganisationViewModel = strOrganisationViewModel;
        if (!string.IsNullOrEmpty(strOrganisationViewModel))
        {
            OrganisationViewModel = JsonConvert.DeserializeObject<OrganisationViewModel>(StrOrganisationViewModel) ?? new OrganisationViewModel();
            if (!string.IsNullOrEmpty(OrganisationViewModel.Familychoice))
            {
                Familychoice = OrganisationViewModel.Familychoice;
            }
        }
    }

    public IActionResult OnPost()
    {
        if (string.IsNullOrEmpty(StrOrganisationViewModel))
        {
            ValidationValid = false;
            return Page();
        }

        if (!ModelState.IsValid)
        {
            ValidationValid = false;
            return Page();
        }

        if (!string.IsNullOrEmpty(StrOrganisationViewModel))
        {
            OrganisationViewModel = JsonConvert.DeserializeObject<OrganisationViewModel>(StrOrganisationViewModel) ?? new OrganisationViewModel();
        }

        OrganisationViewModel.Familychoice = Familychoice;

        StrOrganisationViewModel = JsonConvert.SerializeObject(OrganisationViewModel);

        return RedirectToPage("/OrganisationAdmin/WhoFor", new
        {
            StrOrganisationViewModel
        });
    }
}