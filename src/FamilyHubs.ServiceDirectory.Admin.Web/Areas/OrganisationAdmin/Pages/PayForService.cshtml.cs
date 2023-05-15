using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static FamilyHubs.ServiceDirectory.Admin.Core.Constants.PageConfiguration;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.OrganisationAdmin.Pages;

public class PayForServiceModel : PageModel
{
    public string LastPage { get; set; } = default!;
    public string UserFlow { get; set; } = default!;

    private readonly ICacheService _cacheService;

    [BindProperty]
    [Required]
    public string IsPayedFor { get; set; } = default!;

    [BindProperty]
    [Required]
    public string PayUnit { get; set; } = default!;

    public List<string> PayUnitValues { get; set; } = new List<string> { "Hour", "Day", "Week", "Month", "Course", "Session" };

    [BindProperty]
    [RegularExpression(@"^\d+.?\d{0,2}$")]
    public decimal Cost { get; set; }

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

    public PayForServiceModel(
        ICacheService cacheService)
    {
        _cacheService = cacheService;
    }
    public void OnGet(string strOrganisationViewModel)
    {
        LastPage = _cacheService.RetrieveLastPageName();
        UserFlow = _cacheService.RetrieveUserFlow();
        
        var organisationViewModel = _cacheService.RetrieveOrganisationWithService();

        if (organisationViewModel == null) return;

        if (!string.IsNullOrEmpty(organisationViewModel.IsPayedFor))
            IsPayedFor = organisationViewModel.IsPayedFor;

        if (!string.IsNullOrEmpty(organisationViewModel.PayUnit))
            PayUnit = organisationViewModel.PayUnit;

        if (organisationViewModel.Cost != null)
            Cost = organisationViewModel.Cost.Value;
    }

    public IActionResult OnPost()
    {
        if (string.IsNullOrWhiteSpace(IsPayedFor))
        {
            ValidationValid = false;
            OneOptionSelected = false;
            return Page();
        }

        if (IsPayedFor == "Yes")
        {
            if ((!Regex.IsMatch(Cost.ToString(CultureInfo.InvariantCulture), @"^\d+.?\d{0,2}$") || Cost < 0.01m) && string.IsNullOrEmpty(PayUnit))
            {
                ValidationValid = false;
                CostUnitValid = false;
                return Page();
            }

            if (!Regex.IsMatch(Cost.ToString(CultureInfo.InvariantCulture), @"^\d+.?\d{0,2}$") || Cost < 0.01m)
            {
                ValidationValid = false;
                CostValid = false;
            }

            if (string.IsNullOrEmpty(PayUnit))
            {
                ValidationValid = false;
                UnitSelected = false;
            }
        }

        if (!ValidationValid)
        {
            return Page();
        }
        
        var organisationViewModel = _cacheService.RetrieveOrganisationWithService();

        if (organisationViewModel != null)
        {
            organisationViewModel.IsPayedFor = IsPayedFor;
            organisationViewModel.PayUnit = PayUnit;
            organisationViewModel.Cost = Cost;
        }
        
        _cacheService.StoreOrganisationWithService(organisationViewModel);

        return RedirectToPage(_cacheService.RetrieveLastPageName() == CheckServiceDetailsPageName 
            ? $"/OrganisationAdmin/{CheckServiceDetailsPageName}" 
            : "/OrganisationAdmin/ContactDetails");
    }
}
