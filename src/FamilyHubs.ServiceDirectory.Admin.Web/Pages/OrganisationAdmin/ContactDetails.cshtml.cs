using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static FamilyHubs.ServiceDirectory.Admin.Core.Constants.PageConfiguration;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.OrganisationAdmin;

public class ContactDetailsModel : PageModel
{
    private readonly IRedisCacheService _redis;

    public string LastPage { get; set; } = default!;
    public string UserFlow { get; set; } = default!;

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
    public string? TextPhone { get; set; }

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

    public ContactDetailsModel(
        IRedisCacheService redisCacheService)
    {
        _redis = redisCacheService;
    }
    public void OnGet()
    {
        LastPage = _redis.RetrieveLastPageName();
        UserFlow = _redis.RetrieveUserFlow();

        ContactSelection = new List<string>();

        var organisationViewModel = _redis.RetrieveOrganisationWithService();

        if (organisationViewModel == null) return;

        if (!string.IsNullOrWhiteSpace(organisationViewModel.Email))
        {
            Email = organisationViewModel.Email;
            ContactSelection.Add("email"); //TODO - ContactSelection should be populated in the helper method (which converts from api model to view model)
        }

        if (!string.IsNullOrWhiteSpace(organisationViewModel.Telephone))
        {
            Telephone = organisationViewModel.Telephone;
            ContactSelection.Add("phone");
        }
        if (!string.IsNullOrWhiteSpace(organisationViewModel.Website))
        {
            Website = organisationViewModel.Website;
            ContactSelection.Add("website");
        }
        if (!string.IsNullOrWhiteSpace(organisationViewModel.TextPhone))
        {
            TextPhone = organisationViewModel.TextPhone;
            ContactSelection.Add("textPhone");
        }
    }

    public IActionResult OnPost()
    {
        if (!ContactSelection.Contains("email"))
        {
            Email = string.Empty;
        }
        if (!ContactSelection.Contains("phone"))
        {
            Telephone = string.Empty;
        }
        if (!ContactSelection.Contains("website"))
        {
            Website = string.Empty;
        }
        if (!ContactSelection.Contains("textPhone"))
        {
            TextPhone = string.Empty;
        }

        if (!ContactSelection.Contains("email") && !ContactSelection.Contains("phone") &&
            !ContactSelection.Contains("website") && !ContactSelection.Contains("textPhone"))
        {
            OneOptionSelected = false;
            ValidationValid = false;
            ModelState.AddModelError("Select One Option", "Please select one option");
            return Page();
        }

        if (ContactSelection.Contains("email"))
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                EmailValid = false;
                ValidationValid = false;
            }
            else if (!Regex.IsMatch(Email, @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$"))
            {
                EmailValid = false;
                ValidationValid = false;
            }
        }

        if (ContactSelection.Contains("phone"))
        {
            if (string.IsNullOrWhiteSpace(Telephone))
            {
                PhoneValid = false;
                ValidationValid = false;
            }
            else if (!Regex.IsMatch(Telephone, @"^[A-Za-z0-9]*$"))
            {
                PhoneValid = false;
                ValidationValid = false;
            }
        }

        if (ContactSelection.Contains("website"))
        {
            if (string.IsNullOrWhiteSpace(Website))
            {
                WebsiteValid = false;
                ValidationValid = false;
            }
            else if (!Regex.IsMatch(Website,
                         @"[(http(s)?):\/\/(www\.)?a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)"))
            {
                WebsiteValid = false;
                ValidationValid = false;
            }
        }

        if (ContactSelection.Contains("textPhone"))
        {
            if (string.IsNullOrWhiteSpace(TextPhone))
            {
                TextValid = false;
                ValidationValid = false;
            }
            else if (!Regex.IsMatch(TextPhone, @"^[A-Za-z0-9]*$"))
            {
                TextValid = false;
                ValidationValid = false;
            }
        }

        if (!ValidationValid)
        {
            return Page();
        }

        var organisationViewModel = _redis.RetrieveOrganisationWithService() ?? new OrganisationViewModel();
        organisationViewModel.Email = Email;
        organisationViewModel.Telephone = Telephone;
        organisationViewModel.Website = Website;
        organisationViewModel.TextPhone = TextPhone;
        organisationViewModel.ContactSelection = ContactSelection;

        _redis.StoreOrganisationWithService(organisationViewModel);

        return RedirectToPage(_redis.RetrieveLastPageName() == CheckServiceDetailsPageName
            ? $"/OrganisationAdmin/{CheckServiceDetailsPageName}"
            : "/OrganisationAdmin/ServiceDescription");
    }
}