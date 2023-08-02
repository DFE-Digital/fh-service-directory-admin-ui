using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.ServiceWizzard.Pages;

public class ContactDetailsModel : BasePageModel
{
    [BindProperty]
    public List<string> ContactSelection { get; set; } = default!;

    [BindProperty]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    public string? Email { get; set; }

    [BindProperty]
    [Phone(ErrorMessage = "Please enter a valid phone number")]
    public string? Telephone { get; set; }

    [BindProperty]
    public string? Website { get; set; }

    [BindProperty]
    [Phone(ErrorMessage = "Please enter a valid phone number")]
    public string? Textphone { get; set; }

    [BindProperty]
    public bool ValidationValid { get; set; } = true;

    [BindProperty]
    public bool OneOptionSelected { get; set; } = true;

    [BindProperty]
    public bool EmailValid { get; set; } = true;

    [BindProperty]
    public bool PhoneValid { get; set; } = true;

    [BindProperty]
    public bool WebsiteValid { get; set; } = true;

    [BindProperty]
    public bool TextValid { get; set; } = true;
    public ContactDetailsModel(IRequestDistributedCache requestCache, IConfiguration configuration)
        : base(requestCache)
    {
       
    }

    public async Task OnGet()
    {
        OrganisationViewModel? viewModel = await GetOrganisationViewModel();
        if (viewModel == null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(viewModel.Email))
        {
            Email = viewModel.Email;
            ContactSelection.Add("email"); 
        }

        if (!string.IsNullOrWhiteSpace(viewModel.Telephone))
        {
            Telephone = viewModel.Telephone;
            ContactSelection.Add("phone");
        }
        if (!string.IsNullOrWhiteSpace(viewModel.Website))
        {
            Website = viewModel.Website;
            ContactSelection.Add("website");
        }
        if (!string.IsNullOrWhiteSpace(viewModel.TextPhone))
        {
            Textphone = viewModel.TextPhone;
            ContactSelection.Add("textphone");
        }
    }

    public async Task<IActionResult> OnPost()
    {
        if (ContactSelection == null || !ContactSelection.Contains("email"))
        {
            Email = String.Empty;
        }
        if (ContactSelection == null || !ContactSelection.Contains("phone"))
        {
            Telephone = String.Empty;
        }
        if (ContactSelection == null || !ContactSelection.Contains("website"))
        {
            Website = String.Empty;
        }
        if (ContactSelection == null || !ContactSelection.Contains("textphone"))
        {
            Textphone = String.Empty;
        }

        if (ContactSelection != null)
        {
            if (!ContactSelection.Contains("email") && !ContactSelection.Contains("phone") && !ContactSelection.Contains("website") && !ContactSelection.Contains("textphone"))
            {
                OneOptionSelected = false;
                ValidationValid = false;
                ModelState.AddModelError("Select One Option", "Please select one option");
                return Page();
            }

            if (ContactSelection.Contains("email") && (string.IsNullOrWhiteSpace(Email) || !Regex.IsMatch(Email, @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$")))
            {  
                EmailValid = false;
                ValidationValid = false;   
            }

            if (ContactSelection.Contains("phone") && (string.IsNullOrWhiteSpace(Telephone) || !Regex.IsMatch(Telephone, @"^[A-Za-z0-9]*$")))
            {
                PhoneValid = false;
                ValidationValid = false;
            }

            if (ContactSelection.Contains("website") && (string.IsNullOrWhiteSpace(Website) || !Regex.IsMatch(Website, @"[(http(s)?):\/\/(www\.)?a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)")))
            {
                WebsiteValid = false;
                ValidationValid = false;
            }

            if (ContactSelection.Contains("textphone") && (string.IsNullOrWhiteSpace(Textphone) || !Regex.IsMatch(Textphone, @"^[A-Za-z0-9]*$")))
            {
                TextValid = false;
                ValidationValid = false;
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
        viewModel.Email = Email;
        viewModel.Telephone = Telephone;
        viewModel.Website = Website;
        viewModel.TextPhone = Textphone;
        viewModel.ContactSelection = ContactSelection;

        await SetCacheAsync(viewModel);

        if (string.Compare(await GetLastPage(), "/CheckServiceDetails", StringComparison.OrdinalIgnoreCase) == 0)
        {
            return RedirectToPage("CheckServiceDetails", new { area = "ServiceWizzard" });
        }

        return RedirectToPage("ServiceDescription", new { area = "ServiceWizzard" });
    }
}
