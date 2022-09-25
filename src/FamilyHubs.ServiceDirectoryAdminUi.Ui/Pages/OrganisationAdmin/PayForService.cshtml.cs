using System.Text.RegularExpressions;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

public class PayForServiceModel : PageModel
{
    private readonly ISessionService _session;

    [BindProperty]
    public string IsPayedFor { get; set; } = default!;

    [BindProperty]
    public string PayUnit { get; set; } = default!;

    [BindProperty]
    public decimal Cost { get; set; } = default!;

    //[BindProperty]
    //public string? StrOrganisationViewModel { get; set; }

    [BindProperty]
    public bool ValidationValid { get; set; } = true;

    [BindProperty]
    public bool OneOptionSelected { get; set; } = true;

    [BindProperty]
    public bool UnitSelected { get; set; } = true;

    [BindProperty]
    public bool CostValid { get; set; } = true;

    [BindProperty]
    public bool CostUnitValid { get; set; } = true;

    public PayForServiceModel(ISessionService sessionService)
    {
        _session = sessionService;
    }
    public void OnGet(string strOrganisationViewModel)
    {
        /*** Using Session storage as a service ***/
        var organisationViewModel = _session.RetrieveService(HttpContext);
        if (organisationViewModel != null)
        {
            if (!string.IsNullOrEmpty(organisationViewModel.IsPayedFor))
                IsPayedFor = organisationViewModel.IsPayedFor;

            if (!string.IsNullOrEmpty(organisationViewModel.PayUnit))
                PayUnit = organisationViewModel.PayUnit;

            if (organisationViewModel.Cost != null)
                Cost = organisationViewModel.Cost.Value;
        }

        //StrOrganisationViewModel = strOrganisationViewModel;

        //var organisationViewModel = JsonConvert.DeserializeObject<OrganisationViewModel>(StrOrganisationViewModel);
        //if (organisationViewModel != null)
        //{
        //    if (!string.IsNullOrEmpty(organisationViewModel.IsPayedFor))
        //        IsPayedFor = organisationViewModel.IsPayedFor;

        //    if (!string.IsNullOrEmpty(organisationViewModel.PayUnit))
        //        PayUnit = organisationViewModel.PayUnit;

        //    if (organisationViewModel.Cost != null)
        //        Cost = organisationViewModel.Cost.Value;
        //}
    }

    public IActionResult OnPost()
    {
        /*** Using Session storage as a service ***/
        if (IsPayedFor != "Yes")
        {
            ValidationValid = false;
            OneOptionSelected = false;
            return Page();
        }

        if (IsPayedFor == "Yes" && !Regex.IsMatch(Cost.ToString(), @"^\d+(,\d{3})*(\.\d{2,2})?$") && string.IsNullOrEmpty(PayUnit))
        {
            ValidationValid = false;
            CostUnitValid = false;
            return Page();
        }

        if (IsPayedFor == "Yes" && !Regex.IsMatch(Cost.ToString(), @"^\d+(,\d{3})*(\.\d{2,2})?$"))
        {
            ValidationValid = false;
            CostValid = false;
            return Page();
        }

        if (IsPayedFor == "Yes" && string.IsNullOrEmpty(PayUnit))
        {
            ValidationValid = false;
            UnitSelected = false;
            return Page();
        }

        var organisationViewModel = _session.RetrieveService(HttpContext) ?? new OrganisationViewModel();
        organisationViewModel.IsPayedFor = IsPayedFor;
        organisationViewModel.PayUnit = PayUnit;
        organisationViewModel.Cost = Cost;

        _session.StoreService(HttpContext, organisationViewModel);

        return RedirectToPage("/OrganisationAdmin/ContactDetails");



        //if (IsPayedFor != "Yes" && string.IsNullOrEmpty(StrOrganisationViewModel))
        //{
        //    ValidationValid = false;
        //    OneOptionSelected = false;
        //    return Page();
        //}

        //if (IsPayedFor == "Yes" && !Regex.IsMatch(Cost.ToString(), @"^\d+(,\d{3})*(\.\d{2,2})?$") && string.IsNullOrEmpty(PayUnit))
        //{
        //    ValidationValid = false;
        //    CostUnitValid = false;
        //    return Page();
        //}

        //if (IsPayedFor == "Yes" && !Regex.IsMatch(Cost.ToString(), @"^\d+(,\d{3})*(\.\d{2,2})?$"))
        //{
        //    ValidationValid = false;
        //    CostValid = false;
        //    return Page();
        //}

        //if (IsPayedFor == "Yes" && string.IsNullOrEmpty(PayUnit))
        //{
        //    ValidationValid = false;
        //    UnitSelected = false;
        //    return Page();
        //}

        //    if (!string.IsNullOrEmpty(StrOrganisationViewModel))
        //{
        //    var organisationViewModel = JsonConvert.DeserializeObject<OrganisationViewModel>(StrOrganisationViewModel) ?? new OrganisationViewModel();
        //    organisationViewModel.IsPayedFor = IsPayedFor;
        //    organisationViewModel.PayUnit = PayUnit;    
        //    organisationViewModel.Cost = Cost;

        //    StrOrganisationViewModel = JsonConvert.SerializeObject(organisationViewModel);
        //}

        //return RedirectToPage("/OrganisationAdmin/ContactDetails", new
        //{
        //    strOrganisationViewModel = StrOrganisationViewModel
        //});
    }
}
