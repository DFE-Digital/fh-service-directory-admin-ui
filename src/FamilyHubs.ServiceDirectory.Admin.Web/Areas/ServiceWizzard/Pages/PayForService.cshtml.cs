using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.ServiceWizzard.Pages;

public class PayForServiceModel : BasePageModel
{
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

    [BindProperty]
    public string? CostDetails { get; set; }

    public PayForServiceModel(IRequestDistributedCache requestCache)
        : base(requestCache)
    {
        
    }

    public async Task OnGet()
    {
        OrganisationViewModel? viewModel = await GetOrganisationViewModel();
        if (viewModel == null)
            return;

        if (!string.IsNullOrEmpty(viewModel.IsPayedFor))
            IsPayedFor = viewModel.IsPayedFor;

        if (!string.IsNullOrEmpty(viewModel.PayUnit))
            PayUnit = viewModel.PayUnit;

        if (viewModel.Cost != null)
            Cost = viewModel.Cost.Value;

        if(!string.IsNullOrEmpty(viewModel.CostDetails))
        {
            CostDetails = viewModel.CostDetails;
        }

    }

    public async Task<IActionResult> OnPost()
    {
        if (string.IsNullOrWhiteSpace(IsPayedFor))
        {
            ValidationValid = false;
            OneOptionSelected = false;
            return Page();
        }

        if (IsPayedFor == "Yes-Circumstances" && string.IsNullOrEmpty(CostDetails)) 
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

        OrganisationViewModel? viewModel = await GetOrganisationViewModel();
        if (viewModel == null) 
        {
            viewModel = new OrganisationViewModel();
        }

        viewModel.IsPayedFor = IsPayedFor;
        viewModel.PayUnit = PayUnit;
        viewModel.Cost = Cost;
        viewModel.CostDetails = CostDetails;

        await SetCacheAsync(viewModel);

        if (string.Compare(await GetLastPage(), "/CheckServiceDetails", StringComparison.OrdinalIgnoreCase) == 0)
        {
            return RedirectToPage("CheckServiceDetails", new { area = "ServiceWizzard" });
        }

        return RedirectToPage("ServiceTimes", new { area = "ServiceWizzard" });
    }
}
