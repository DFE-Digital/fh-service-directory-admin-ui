using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using static FamilyHubs.ServiceDirectoryAdminUi.Ui.Infrastructure.Configuration.PageConfiguration;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

public class ContactDetailsModel : PageModel
{
    private readonly ISessionService _session;

    public string LastPage { get; set; } = default!;
    public string UserFlow { get; set; } = default!;

    [BindProperty]
    public List<string> ContactSelection { get; set; } = default!;
    
    [BindProperty]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    public string? Email { get; set; } = default!;
    
    [BindProperty]
    [Phone(ErrorMessage = "Please enter a valid phone number")]
    public string? Telephone { get; set; } = default!;
    
    [BindProperty]
    public string? Website { get; set; } = default!;
    
    [BindProperty]
    [Phone(ErrorMessage = "Please enter a valid phone number")]
    public string? Textphone { get; set; } = default!;

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

    public ContactDetailsModel(ISessionService sessionService)
    {
        _session = sessionService;
    }
    public void OnGet()
    {
        LastPage = _session.RetrieveLastPageName(HttpContext);
        UserFlow = _session.RetrieveUserFlow(HttpContext);
        ContactSelection = new List<string>();

        var organisationViewModel = _session.RetrieveOrganisationWithService(HttpContext);
        if (organisationViewModel != null)
        {
            if (!string.IsNullOrWhiteSpace(organisationViewModel.Email))
            { 
                Email = organisationViewModel.Email;
                ContactSelection.Add("email"); //TODO - COntactSelection should be populated in the helper methiod (which converts from api model to veiw model)
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
            if (!string.IsNullOrWhiteSpace(organisationViewModel.Textphone))
            {
                Textphone = organisationViewModel.Textphone;
                ContactSelection.Add("textphone");
            }
        }

    }

    public IActionResult OnPost()
    {
        if (ContactSelection == null || !ContactSelection.Contains("email"))
        {
            this.Email = String.Empty;
        }
        if (ContactSelection == null || !ContactSelection.Contains("phone"))
        {
            this.Telephone = String.Empty;
        }
        if (ContactSelection == null || !ContactSelection.Contains("website"))
        {
            this.Website = String.Empty;
        }
        if (ContactSelection == null || !ContactSelection.Contains("textphone"))
        {
            this.Textphone = String.Empty;
        }

        if (!(ContactSelection == null))
        {
            if (!ContactSelection.Contains("email") && !ContactSelection.Contains("phone") && !ContactSelection.Contains("website") && !ContactSelection.Contains("textphone"))
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
                else if (!Regex.IsMatch(Email.ToString(), @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$"))
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
                else if (!Regex.IsMatch(Telephone.ToString(), @"^[A-Za-z0-9]*$"))
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
                else if (!Regex.IsMatch(Website.ToString(), @"[(http(s)?):\/\/(www\.)?a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)"))
                {
                    WebsiteValid = false;
                    ValidationValid = false;
                }

            }

            if (ContactSelection.Contains("textphone"))
            {
                if (string.IsNullOrWhiteSpace(Textphone))
                {
                    TextValid = false;
                    ValidationValid = false;
                }
                else if (!Regex.IsMatch(Textphone.ToString(), @"^[A-Za-z0-9]*$"))
                {
                    TextValid = false;
                    ValidationValid = false;
                }

            }
        }

        if (!ValidationValid)
        {
            return Page();
        }

            var organisationViewModel = _session.RetrieveOrganisationWithService(HttpContext) ?? new OrganisationViewModel();
            organisationViewModel.Email = Email;
            organisationViewModel.Telephone = Telephone;
            organisationViewModel.Website = Website;
            organisationViewModel.Textphone = Textphone;
            organisationViewModel.ContactSelection = ContactSelection;

            _session.StoreOrganisationWithService(HttpContext, organisationViewModel);

        if (_session.RetrieveLastPageName(HttpContext) == CheckServiceDetailsPageName)
        {
            return RedirectToPage($"/OrganisationAdmin/{CheckServiceDetailsPageName}");
        }

        return RedirectToPage("/OrganisationAdmin/ServiceDescription");

    }
}