using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

public class OfferAtFamiliesPlaceModel : PageModel
{
    private readonly ISessionService _session;

    [BindProperty]
    public string Familychoice { get; set; } = default!;

    [BindProperty]
    public OrganisationViewModel OrganisationViewModel { get; set; } = new OrganisationViewModel();

    //[BindProperty]
    //public string? StrOrganisationViewModel { get; set; }

    [BindProperty]
    public bool ValidationValid { get; set; } = true;

    public OfferAtFamiliesPlaceModel(ISessionService sessionService)
    {
        _session = sessionService;
    }
    public void OnGet(string strOrganisationViewModel)
    {
        /*** Using Session storage as a service ***/
        OrganisationViewModel = _session.RetrieveService(HttpContext) ?? new OrganisationViewModel();
            if (!string.IsNullOrEmpty(OrganisationViewModel.Familychoice))
            {
                Familychoice = OrganisationViewModel.Familychoice;
            }
        


        //StrOrganisationViewModel = strOrganisationViewModel;
        //if (!string.IsNullOrEmpty(strOrganisationViewModel))
        //{
        //    OrganisationViewModel = JsonConvert.DeserializeObject<OrganisationViewModel>(StrOrganisationViewModel) ?? new OrganisationViewModel();
        //    if (!string.IsNullOrEmpty(OrganisationViewModel.Familychoice))
        //    {
        //        Familychoice = OrganisationViewModel.Familychoice;
        //    }
        //}
    }

    public IActionResult OnPost()
    {
        /*** Using Session storage as a service ***/
        if (!ModelState.IsValid)
        {
            ValidationValid = false;
            return Page();
        }

        OrganisationViewModel = _session.RetrieveService(HttpContext) ?? new OrganisationViewModel();
        

        OrganisationViewModel.Familychoice = Familychoice;

        _session.StoreService(HttpContext, OrganisationViewModel);

        return RedirectToPage("/OrganisationAdmin/WhoFor");

        //if (string.IsNullOrEmpty(StrOrganisationViewModel))
        //{
        //    ValidationValid = false;
        //    return Page();
        //}

        //if (!ModelState.IsValid)
        //{
        //    ValidationValid = false;
        //    return Page();
        //}

        //if (!string.IsNullOrEmpty(StrOrganisationViewModel))
        //{
        //    OrganisationViewModel = JsonConvert.DeserializeObject<OrganisationViewModel>(StrOrganisationViewModel) ?? new OrganisationViewModel();
        //}

        //OrganisationViewModel.Familychoice = Familychoice;

        //StrOrganisationViewModel = JsonConvert.SerializeObject(OrganisationViewModel);

        //return RedirectToPage("/OrganisationAdmin/WhoFor", new
        //{
        //    StrOrganisationViewModel
        //});
    }
}