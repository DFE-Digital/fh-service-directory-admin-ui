using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

public class ServiceDescriptionModel : PageModel
{
    [BindProperty]
    [MaxLength(500, ErrorMessage = "You can only add upto 500 characters")]
    public string? Description { get; set; } = default!;

    [BindProperty]
    public string? StrOrganisationViewModel { get; set; }

    public void OnGet(string strOrganisationViewModel)
    {
        StrOrganisationViewModel = strOrganisationViewModel;

        var organisationViewModel = JsonConvert.DeserializeObject<OrganisationViewModel>(StrOrganisationViewModel) ?? new OrganisationViewModel();
        if (organisationViewModel != null && !string.IsNullOrEmpty(organisationViewModel.ServiceDescription))
        {
            Description = organisationViewModel.ServiceDescription;
        }
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (!string.IsNullOrEmpty(StrOrganisationViewModel))
        {
            var organisationViewModel = JsonConvert.DeserializeObject<OrganisationViewModel>(StrOrganisationViewModel) ?? new OrganisationViewModel();
            organisationViewModel.ServiceDescription = Description;

            StrOrganisationViewModel = JsonConvert.SerializeObject(organisationViewModel);
        }

        return RedirectToPage("/OrganisationAdmin/CheckServiceDetails", new
        {
            strOrganisationViewModel = StrOrganisationViewModel
        });
    }
}
