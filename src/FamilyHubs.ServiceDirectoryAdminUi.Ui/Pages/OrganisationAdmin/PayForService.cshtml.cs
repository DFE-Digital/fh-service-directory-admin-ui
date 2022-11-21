using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static FamilyHubs.ServiceDirectoryAdminUi.Ui.Infrastructure.Configuration.PageConfiguration;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

[Authorize(Policy = "ServiceMaintainer")]
public class PayForServiceModel : PageModel
{
    public string LastPage { get; set; } = default!;
    public string UserFlow { get; set; } = default!;

    private readonly ISessionService _session;
    private readonly IRedisCacheService _redis;

    [BindProperty]
    [Required]
    public string IsPayedFor { get; set; } = default!;

    [BindProperty]
    [Required]
    public string PayUnit { get; set; } = default!;

    public List<string> PayUnitValues { get; set; } = new List<string>() { "Hour", "Day", "Week", "Month", "Course", "Session" };

    [BindProperty]
    [RegularExpression(@"^\d+.?\d{0,2}$")]
    public decimal Cost { get; set; } = default!;

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

    public PayForServiceModel(ISessionService sessionService, IRedisCacheService redisCacheService)
    {
        _session = sessionService;
        _redis = redisCacheService;
    }
    public void OnGet(string strOrganisationViewModel)
    {
        LastPage = _redis.RetrieveLastPageName();
        UserFlow = _redis.RetrieveUserFlow();
        
        var organisationViewModel = _redis.RetrieveOrganisationWithService();

        if (organisationViewModel != null)
        {
            if (!string.IsNullOrEmpty(organisationViewModel.IsPayedFor))
                IsPayedFor = organisationViewModel.IsPayedFor;

            if (!string.IsNullOrEmpty(organisationViewModel.PayUnit))
                PayUnit = organisationViewModel.PayUnit;

            if (organisationViewModel.Cost != null)
                Cost = organisationViewModel.Cost.Value;
        }

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
            if ((!Regex.IsMatch(Cost.ToString(), @"^\d+.?\d{0,2}$") || Cost < 0.01m) && string.IsNullOrEmpty(PayUnit))
            {
                ValidationValid = false;
                CostUnitValid = false;
                return Page();
            }

            if (!Regex.IsMatch(Cost.ToString(), @"^\d+.?\d{0,2}$") || Cost < 0.01m)
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
        
        var organisationViewModel = _redis.RetrieveOrganisationWithService();

        if (organisationViewModel != null)
        {
            organisationViewModel.IsPayedFor = IsPayedFor;
            organisationViewModel.PayUnit = PayUnit;
            organisationViewModel.Cost = Cost;
        }
        
        _redis.StoreOrganisationWithService(organisationViewModel);
        
        if (_redis.RetrieveLastPageName() == CheckServiceDetailsPageName)
        {
            return RedirectToPage($"/OrganisationAdmin/{CheckServiceDetailsPageName}");
        }
        return RedirectToPage("/OrganisationAdmin/ContactDetails");

    }
}
