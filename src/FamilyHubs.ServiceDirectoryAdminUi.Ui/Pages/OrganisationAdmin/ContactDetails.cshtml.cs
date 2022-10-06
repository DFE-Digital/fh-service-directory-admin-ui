using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
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

    //[BindProperty]
    //public string? StrOrganisationViewModel { get; set; }

    [BindProperty]
    public bool ValidationValid { get; set; } = true;

    [BindProperty]
    public bool OneOptionSelected { get; set; } = true;

    public ContactDetailsModel(ISessionService sessionService)
    {
        _session = sessionService;
    }
    public void OnGet()
    {
        LastPage = _session.RetrieveLastPageName(HttpContext);
        UserFlow = _session.RetrieveUserFlow(HttpContext);

        /*** Using Session storage as a service ***/
        var organisationViewModel = _session.RetrieveOrganisationWithService(HttpContext);
        if (organisationViewModel != null)
        {
            if (!string.IsNullOrWhiteSpace(organisationViewModel.Email))
                Email = organisationViewModel.Email;
            if (!string.IsNullOrWhiteSpace(organisationViewModel.Telephone))
                Telephone = organisationViewModel.Telephone;
            if (!string.IsNullOrWhiteSpace(organisationViewModel.Website))
                Website = organisationViewModel.Website;
            if (!string.IsNullOrWhiteSpace(organisationViewModel.Textphone))
                Website = organisationViewModel.Textphone;

            if (organisationViewModel.ContactSelection != null && organisationViewModel.ContactSelection.Any())
            {
                ContactSelection = organisationViewModel.ContactSelection;
            }
        }

        //StrOrganisationViewModel = strOrganisationViewModel;

        //var organisationViewModel = JsonConvert.DeserializeObject<OrganisationViewModel>(StrOrganisationViewModel);
        //if (organisationViewModel != null)
        //{
        //    if (!string.IsNullOrWhiteSpace(organisationViewModel.Email))
        //        Email = organisationViewModel.Email;
        //    if (!string.IsNullOrWhiteSpace(organisationViewModel.Telephone))
        //        Telephone = organisationViewModel.Telephone;
        //    if (!string.IsNullOrWhiteSpace(organisationViewModel.Website))
        //        Website = organisationViewModel.Website;
        //    if (!string.IsNullOrWhiteSpace(organisationViewModel.Textphone))
        //        Website = organisationViewModel.Textphone;

        //    if (organisationViewModel.ContactSelection != null && organisationViewModel.ContactSelection.Any())
        //    {
        //        ContactSelection = organisationViewModel.ContactSelection;
        //    }
        //}
    }

    public IActionResult OnPost()
    {
        //Reset values if checkbox unselected
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

        if (string.IsNullOrWhiteSpace(Email) && string.IsNullOrWhiteSpace(Website) && string.IsNullOrWhiteSpace(Textphone) && string.IsNullOrWhiteSpace(Telephone))
        {
            OneOptionSelected = false;
            ModelState.AddModelError("Select One Option", "Please select one option");
        }

        if (!ModelState.IsValid)
        {
            ValidationValid = false;
            return Page();
        }

        /*** Using Session storage as a service ***/
        
        
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


        //if (!string.IsNullOrEmpty(StrOrganisationViewModel))
        //{
        //    var organisationViewModel = JsonConvert.DeserializeObject<OrganisationViewModel>(StrOrganisationViewModel) ?? new OrganisationViewModel();
        //    organisationViewModel.Email = Email;
        //    organisationViewModel.Telephone = Telephone;
        //    organisationViewModel.Website = Website;
        //    organisationViewModel.Textphone = Textphone;
        //    organisationViewModel.ContactSelection = ContactSelection;

        //    StrOrganisationViewModel = JsonConvert.SerializeObject(organisationViewModel);
        //}

        //return RedirectToPage("/OrganisationAdmin/ServiceDescription", new
        //{
        //    strOrganisationViewModel = StrOrganisationViewModel
        //});
    }
}