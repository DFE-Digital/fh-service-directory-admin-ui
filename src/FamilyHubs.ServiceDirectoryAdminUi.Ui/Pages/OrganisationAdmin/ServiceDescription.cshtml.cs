using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

public class ServiceDescriptionModel : PageModel
{
    public string LastPage { get; set; } = default!;
    public string UserFlow { get; set; } = default!;

    private readonly ISessionService _session;

    [BindProperty]
    [MaxLength(500, ErrorMessage = "You can only add upto 500 characters")]
    public string? Description { get; set; } = default!;

    //[BindProperty]
    //public string? StrOrganisationViewModel { get; set; }

    public ServiceDescriptionModel(ISessionService sessionService)
    {
        _session = sessionService;
    }
    public void OnGet(string strOrganisationViewModel)
    {
        LastPage = _session.RetrieveLastPageName(HttpContext);
        UserFlow = _session.RetrieveUserFlow(HttpContext);

        /*** Using Session storage as a service ***/
        var organisationViewModel = _session.RetrieveOrganisationWithService(HttpContext) ?? new OrganisationViewModel();
        if (organisationViewModel != null && !string.IsNullOrEmpty(organisationViewModel.ServiceDescription))
        {
            Description = organisationViewModel.ServiceDescription;
        }



        //StrOrganisationViewModel = strOrganisationViewModel;

        //var organisationViewModel = JsonConvert.DeserializeObject<OrganisationViewModel>(StrOrganisationViewModel) ?? new OrganisationViewModel();
        //if (organisationViewModel != null && !string.IsNullOrEmpty(organisationViewModel.ServiceDescription))
        //{
        //    Description = organisationViewModel.ServiceDescription;
        //}
    }

    public IActionResult OnPost()
    {
        /*** Using Session storage as a service ***/
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var organisationViewModel = _session.RetrieveOrganisationWithService(HttpContext) ?? new OrganisationViewModel();
            organisationViewModel.ServiceDescription = Description;

        _session.StoreOrganisationWithService(HttpContext, organisationViewModel);

        return RedirectToPage("/OrganisationAdmin/CheckServiceDetails");


        //if (!ModelState.IsValid)
        //{
        //    return Page();
        //}

        //if (!string.IsNullOrEmpty(StrOrganisationViewModel))
        //{
        //    var organisationViewModel = JsonConvert.DeserializeObject<OrganisationViewModel>(StrOrganisationViewModel) ?? new OrganisationViewModel();
        //    organisationViewModel.ServiceDescription = Description;

        //    StrOrganisationViewModel = JsonConvert.SerializeObject(organisationViewModel);
        //}

        //return RedirectToPage("/OrganisationAdmin/CheckServiceDetails", new
        //{
        //    strOrganisationViewModel = StrOrganisationViewModel
        //});
    }
}
