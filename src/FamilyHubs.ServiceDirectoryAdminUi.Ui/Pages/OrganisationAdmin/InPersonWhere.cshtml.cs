using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

public class InPersonWhereModel : PageModel
{
    [BindProperty]
    public string Address_1 { get; set; } = default!;
    [BindProperty]
    public string Address_2 { get; set; } = "temporary place holder until model extended";
    [BindProperty]
    public string City { get; set; } = default!;
    [BindProperty]
    public string Postal_code { get; set; } = default!;
    [BindProperty]
    public string Country { get; set; } = default!;
    [BindProperty]
    public string State_province { get; set; } = default!;

    [BindProperty]
    public List<string> InPersonSelection { get; set; } = default!;

    [BindProperty]
    public OrganisationViewModel OrganisationViewModel { get; set; } = new OrganisationViewModel();

    [BindProperty]
    public string? StrOrganisationViewModel { get; set; }

    [BindProperty]
    public bool ValidationValid { get; set; } = true;

    [BindProperty]
    public bool Address1Valid { get; set; } = true;

    [BindProperty]
    public bool TownCityValid { get; set; } = true;

    [BindProperty]
    public bool PostcodeValid { get; set; } = true;

    private readonly IPostcodeLocationClientService _postcodeLocationClientService;

    public InPersonWhereModel(IPostcodeLocationClientService postcodeLocationClientService)
    {
        _postcodeLocationClientService = postcodeLocationClientService;
    }

    public void OnGet(string strOrganisationViewModel)
    {
        StrOrganisationViewModel = strOrganisationViewModel;
        if (!string.IsNullOrEmpty(strOrganisationViewModel))
            OrganisationViewModel = JsonConvert.DeserializeObject<OrganisationViewModel>(StrOrganisationViewModel) ?? new OrganisationViewModel();

        
        OrganisationViewModel.Country = "England";
        if (OrganisationViewModel != null)
        {
            if (!string.IsNullOrEmpty(OrganisationViewModel.Address_1))
                Address_1 = OrganisationViewModel.Address_1;
            if (!string.IsNullOrEmpty(OrganisationViewModel.City))
                City = OrganisationViewModel.City;
            if (!string.IsNullOrEmpty(OrganisationViewModel.Postal_code))
                Postal_code = OrganisationViewModel.Postal_code;
            if (!string.IsNullOrEmpty(OrganisationViewModel.State_province))
                State_province = OrganisationViewModel.State_province;
            if (OrganisationViewModel.InPersonSelection != null && OrganisationViewModel.InPersonSelection.Any())
                InPersonSelection = OrganisationViewModel.InPersonSelection;
        }
    }

    public async Task<IActionResult> OnPost()
    {
        if (InPersonSelection.Contains("Our own location"))
        {
            ModelState.Remove("Country");
            Country = "England";
            if (!ModelState.IsValid)
            {
                return Page();
            }
        }

        if (!InPersonSelection.Any())
        {
            return Page();
        }
        

        if (!string.IsNullOrEmpty(StrOrganisationViewModel))
        {
            OrganisationViewModel = JsonConvert.DeserializeObject<OrganisationViewModel>(StrOrganisationViewModel) ?? new OrganisationViewModel();
            OrganisationViewModel.InPersonSelection = new List<string>(InPersonSelection);
            OrganisationViewModel.Address_1 = Address_1;
            OrganisationViewModel.City = City;
            OrganisationViewModel.State_province = State_province;
            OrganisationViewModel.Country = "England";
            OrganisationViewModel.Postal_code = Postal_code;

            if (!string.IsNullOrEmpty(Postal_code))
            {
                PostcodeApiModel postcodeApiModel = await _postcodeLocationClientService.LookupPostcode(Postal_code);
                if (postcodeApiModel != null)
                {
                    OrganisationViewModel.Latitude = postcodeApiModel.result.latitude;
                    OrganisationViewModel.Longtitude = postcodeApiModel.result.longitude;
                }
            }

            StrOrganisationViewModel = JsonConvert.SerializeObject(OrganisationViewModel);
        }

        return RedirectToPage("/OrganisationAdmin/WhoFor", new
        {
            strOrganisationViewModel = StrOrganisationViewModel
        });


    }
}
