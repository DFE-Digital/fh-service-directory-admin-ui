using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using static FamilyHubs.ServiceDirectoryAdminUi.Ui.Infrastructure.Configuration.PageConfiguration;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

public class InPersonWhereModel : PageModel
{
    [BindProperty]
    public string Address_1 { get; set; } = default!;
    [BindProperty]
    public string Address_2 { get; set; } = default!;
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

    //[BindProperty]
    //public string? StrOrganisationViewModel { get; set; }

    [BindProperty]
    public bool ValidationValid { get; set; } = true;

    [BindProperty]
    public bool Address1Valid { get; set; } = true;

    [BindProperty]
    public bool TownCityValid { get; set; } = true;

    [BindProperty]
    public bool PostcodeValid { get; set; } = true;

    [BindProperty]
    public bool PostcodeAPIValid { get; set; } = true;

    private readonly IPostcodeLocationClientService _postcodeLocationClientService;
    private readonly ISessionService _session;

    public InPersonWhereModel(IPostcodeLocationClientService postcodeLocationClientService, ISessionService sessionService)
    {
        _postcodeLocationClientService = postcodeLocationClientService;
        _session = sessionService;
    }

    public void OnGet(string strOrganisationViewModel)
    {
        /*** Using Session storage as a service ***/
        OrganisationViewModel = _session.RetrieveService(HttpContext) ?? new OrganisationViewModel();

        OrganisationViewModel.Country = "England";
        if (OrganisationViewModel != null)
        {
            if (!string.IsNullOrEmpty(OrganisationViewModel.Address_1))
                if (OrganisationViewModel.Address_1.Split("|").Length > 1)
                {
                    Address_1 = OrganisationViewModel.Address_1.Split("|")[0];
                    Address_2 = OrganisationViewModel.Address_1.Split("|")[1];
                }
                else
                {
                    Address_1 = OrganisationViewModel.Address_1;
                }
            if (!string.IsNullOrEmpty(OrganisationViewModel.City))
                City = OrganisationViewModel.City;
            if (!string.IsNullOrEmpty(OrganisationViewModel.Postal_code))
                Postal_code = OrganisationViewModel.Postal_code;
            if (!string.IsNullOrEmpty(OrganisationViewModel.State_province))
                State_province = OrganisationViewModel.State_province;
            if (OrganisationViewModel.InPersonSelection != null && OrganisationViewModel.InPersonSelection.Any())
                InPersonSelection = OrganisationViewModel.InPersonSelection;
        }



        //StrOrganisationViewModel = strOrganisationViewModel;
        //if (!string.IsNullOrEmpty(strOrganisationViewModel))
        //    OrganisationViewModel = JsonConvert.DeserializeObject<OrganisationViewModel>(StrOrganisationViewModel) ?? new OrganisationViewModel();


        //OrganisationViewModel.Country = "England";
        //if (OrganisationViewModel != null)
        //{
        //    if (!string.IsNullOrEmpty(OrganisationViewModel.Address_1))
        //        if (OrganisationViewModel.Address_1.Split("|").Length > 1)
        //        {
        //            Address_1 = OrganisationViewModel.Address_1.Split("|")[0];
        //            Address_2 = OrganisationViewModel.Address_1.Split("|")[1];
        //        }
        //        else
        //        {
        //            Address_1 = OrganisationViewModel.Address_1;
        //        }
        //    if (!string.IsNullOrEmpty(OrganisationViewModel.City))
        //        City = OrganisationViewModel.City;
        //    if (!string.IsNullOrEmpty(OrganisationViewModel.Postal_code))
        //        Postal_code = OrganisationViewModel.Postal_code;
        //    if (!string.IsNullOrEmpty(OrganisationViewModel.State_province))
        //        State_province = OrganisationViewModel.State_province;
        //    if (OrganisationViewModel.InPersonSelection != null && OrganisationViewModel.InPersonSelection.Any())
        //        InPersonSelection = OrganisationViewModel.InPersonSelection;
        //}
    }

    public async Task<IActionResult> OnPost()
    {
        /*** Using Session storage as a service ***/
        ModelState.Remove("Country");
        ModelState.Remove("Address_2");
        ModelState.Remove("State_province");
        ModelState.Remove("PostcodeAPIValid");
        Country = "England";

        try
        {
            PostcodeApiModel postcodeApiModel = await _postcodeLocationClientService.LookupPostcode(Postal_code);
        }
        catch
        {
            PostcodeAPIValid = false;
        }

        if (!ModelState.IsValid ||
            string.IsNullOrEmpty(Address_1) ||
            string.IsNullOrEmpty(City) ||
            string.IsNullOrEmpty(Postal_code) ||
            PostcodeAPIValid == false)
        {
            ValidationValid = false;

            if (string.IsNullOrEmpty(Address_1))
                Address1Valid = false;

            if (string.IsNullOrEmpty(City))
                TownCityValid = false;

            if (string.IsNullOrEmpty(Postal_code) || PostcodeAPIValid == false ||
                Postal_code.Replace(" ", String.Empty).Length < 6 || Postal_code.Replace(" ", String.Empty).Length > 7)
                PostcodeValid = false;

            return Page();
        }


        OrganisationViewModel = _session.RetrieveService(HttpContext) ?? new OrganisationViewModel();
            OrganisationViewModel.InPersonSelection = new List<string>(InPersonSelection);
            OrganisationViewModel.Address_1 = Address_1 + "|" + Address_2;
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

        _session.StoreService(HttpContext, OrganisationViewModel);

        if (_session.RetrieveLastPageName(HttpContext) == CheckServiceDetailsPageName)
        {
            return RedirectToPage($"/OrganisationAdmin/{CheckServiceDetailsPageName}");
        }
        return RedirectToPage("/OrganisationAdmin/OfferAtFamiliesPlace");




        //ModelState.Remove("Country");
        //ModelState.Remove("Address_2");
        //ModelState.Remove("State_province");
        //ModelState.Remove("PostcodeAPIValid");
        //Country = "England";

        //try
        //{
        //    PostcodeApiModel postcodeApiModel = await _postcodeLocationClientService.LookupPostcode(Postal_code);
        //}
        //catch
        //{
        //    PostcodeAPIValid = false;
        //}

        //if (!ModelState.IsValid ||
        //    string.IsNullOrEmpty(Address_1) ||
        //    string.IsNullOrEmpty(City) ||
        //    string.IsNullOrEmpty(Postal_code) ||
        //    PostcodeAPIValid == false)
        //{
        //    ValidationValid = false;

        //    if (string.IsNullOrEmpty(Address_1))
        //        Address1Valid = false;

        //    if (string.IsNullOrEmpty(City))
        //        TownCityValid = false;

        //    if (string.IsNullOrEmpty(Postal_code) || PostcodeAPIValid == false ||
        //        Postal_code.Replace(" ", String.Empty).Length < 6 || Postal_code.Replace(" ", String.Empty).Length > 7)
        //        PostcodeValid = false;

        //    return Page();
        //}


        //if (!string.IsNullOrEmpty(StrOrganisationViewModel))
        //{
        //    OrganisationViewModel = JsonConvert.DeserializeObject<OrganisationViewModel>(StrOrganisationViewModel) ?? new OrganisationViewModel();
        //    OrganisationViewModel.InPersonSelection = new List<string>(InPersonSelection);
        //    OrganisationViewModel.Address_1 = Address_1 + "|" + Address_2;
        //    OrganisationViewModel.City = City;
        //    OrganisationViewModel.State_province = State_province;
        //    OrganisationViewModel.Country = "England";
        //    OrganisationViewModel.Postal_code = Postal_code;

        //    if (!string.IsNullOrEmpty(Postal_code))
        //    {   
        //        PostcodeApiModel postcodeApiModel = await _postcodeLocationClientService.LookupPostcode(Postal_code);
        //        if (postcodeApiModel != null)
        //        {
        //            OrganisationViewModel.Latitude = postcodeApiModel.result.latitude;
        //            OrganisationViewModel.Longtitude = postcodeApiModel.result.longitude;
        //        }
        //    }

        //    StrOrganisationViewModel = JsonConvert.SerializeObject(OrganisationViewModel);
        //}

        //return RedirectToPage("/OrganisationAdmin/OfferAtFamiliesPlace", new
        //{
        //    strOrganisationViewModel = StrOrganisationViewModel
        //});


    }
}
