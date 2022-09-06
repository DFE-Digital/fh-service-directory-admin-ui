using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

public class ServiceDeliveryTypeModel : PageModel
{
    public Dictionary<int, string> DictServiceDelivery = new();

    [BindProperty]
    public List<string> ServiceDeliverySelection { get; set; } = default!;

    [BindProperty]
    public string? StrOrganisationViewModel { get; set; }

    [BindProperty]
    public bool validationValid { get; set; } = true;

    public void OnGet(string strOrganisationViewModel)
    {
        StrOrganisationViewModel = strOrganisationViewModel;

        var myEnumDescriptions = from ServiceDelivery n in Enum.GetValues(typeof(ServiceDelivery))
                                 select new { Id = (int)n, Name = Utility.GetEnumDescription(n) };

        foreach (var myEnumDescription in myEnumDescriptions)
        {
            if (myEnumDescription.Id == 0)
                continue;
            DictServiceDelivery[myEnumDescription.Id] = myEnumDescription.Name;
        }

        var organisationViewModel = JsonConvert.DeserializeObject<OrganisationViewModel>(StrOrganisationViewModel) ?? new OrganisationViewModel();
        if (organisationViewModel != null && organisationViewModel.ServiceDeliverySelection != null)
        {
            ServiceDeliverySelection = organisationViewModel.ServiceDeliverySelection;
        }
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (StrOrganisationViewModel != null)
        {
            var organisationViewModel = JsonConvert.DeserializeObject<OrganisationViewModel>(StrOrganisationViewModel) ?? new OrganisationViewModel();
            organisationViewModel.ServiceDeliverySelection = new List<string>(ServiceDeliverySelection);
            StrOrganisationViewModel = JsonConvert.SerializeObject(organisationViewModel);
        }

        if (ServiceDeliverySelection.Contains("1"))
        {
            return RedirectToPage("/OrganisationAdmin/InPersonWhere", new
            {
                strOrganisationViewModel = StrOrganisationViewModel
            });
        }

        return RedirectToPage("/OrganisationAdmin/WhoFor", new
        {
            strOrganisationViewModel = StrOrganisationViewModel
        });

    }

}
