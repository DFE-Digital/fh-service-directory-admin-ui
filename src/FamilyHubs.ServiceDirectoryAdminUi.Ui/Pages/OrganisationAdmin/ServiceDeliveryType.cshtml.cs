using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using static FamilyHubs.ServiceDirectoryAdminUi.Ui.Infrastructure.Configuration.PageConfiguration;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

public class ServiceDeliveryTypeModel : PageModel
{
    public Dictionary<int, string> DictServiceDelivery = new();
    private readonly ISessionService _session;

    [BindProperty]
    public List<string> ServiceDeliverySelection { get; set; } = default!;

    //[BindProperty]
    //public string? StrOrganisationViewModel { get; set; }

    [BindProperty]
    public bool ValidationValid { get; set; } = true;

    public ServiceDeliveryTypeModel(ISessionService sessionService)
    {
        _session = sessionService;
    }
    public void OnGet(string strOrganisationViewModel)
    {
        /*** Using Session storage as a service ***/
        var myEnumDescriptions = from ServiceDelivery n in Enum.GetValues(typeof(ServiceDelivery))
                                 select new { Id = (int)n, Name = Utility.GetEnumDescription(n) };

        foreach (var myEnumDescription in myEnumDescriptions)
        {
            if (myEnumDescription.Id == 0)
                continue;
            DictServiceDelivery[myEnumDescription.Id] = myEnumDescription.Name;
        }

        var organisationViewModel = _session.RetrieveService(HttpContext) ?? new OrganisationViewModel();
        if (organisationViewModel != null && organisationViewModel.ServiceDeliverySelection != null)
        {
            ServiceDeliverySelection = organisationViewModel.ServiceDeliverySelection;
        }



        //StrOrganisationViewModel = strOrganisationViewModel;

        //var myEnumDescriptions = from ServiceDelivery n in Enum.GetValues(typeof(ServiceDelivery))
        //                         select new { Id = (int)n, Name = Utility.GetEnumDescription(n) };

        //foreach (var myEnumDescription in myEnumDescriptions)
        //{
        //    if (myEnumDescription.Id == 0)
        //        continue;
        //    DictServiceDelivery[myEnumDescription.Id] = myEnumDescription.Name;
        //}

        //var organisationViewModel = JsonConvert.DeserializeObject<OrganisationViewModel>(StrOrganisationViewModel) ?? new OrganisationViewModel();
        //if (organisationViewModel != null && organisationViewModel.ServiceDeliverySelection != null)
        //{
        //    ServiceDeliverySelection = organisationViewModel.ServiceDeliverySelection;
        //}
    }

    public IActionResult OnPost()
    {
        var myEnumDescriptions = from ServiceDelivery n in Enum.GetValues(typeof(ServiceDelivery))
                                 select new { Id = (int)n, Name = Utility.GetEnumDescription(n) };

        if (!ModelState.IsValid || ServiceDeliverySelection.Count == 0)
        {
            foreach (var myEnumDescription in myEnumDescriptions)
            {
                if (myEnumDescription.Id == 0)
                    continue;
                DictServiceDelivery[myEnumDescription.Id] = myEnumDescription.Name;
            }
            ValidationValid = false;
            return Page();

        }

        /*** Using Session storage as a service ***/
            var organisationViewModel = _session.RetrieveService(HttpContext) ?? new OrganisationViewModel();
            organisationViewModel.ServiceDeliverySelection = new List<string>(ServiceDeliverySelection);
            _session.StoreService(HttpContext, organisationViewModel);
    

        if (ServiceDeliverySelection.Contains("1"))
        {
            return RedirectToPage("/OrganisationAdmin/InPersonWhere");
        }

        if (_session.RetrieveLastPageName(HttpContext) == CheckServiceDetailsPageName)
        {
            return RedirectToPage($"/OrganisationAdmin/{CheckServiceDetailsPageName}");
        }
        return RedirectToPage("/OrganisationAdmin/WhoFor");

        //if (StrOrganisationViewModel != null)
        //{
        //    var organisationViewModel = JsonConvert.DeserializeObject<OrganisationViewModel>(StrOrganisationViewModel) ?? new OrganisationViewModel();
        //    organisationViewModel.ServiceDeliverySelection = new List<string>(ServiceDeliverySelection);
        //    StrOrganisationViewModel = JsonConvert.SerializeObject(organisationViewModel);
        //}

        //if (ServiceDeliverySelection.Contains("1"))
        //{
        //    return RedirectToPage("/OrganisationAdmin/InPersonWhere", new
        //    {
        //        strOrganisationViewModel = StrOrganisationViewModel
        //    });
        //}

        //return RedirectToPage("/OrganisationAdmin/WhoFor", new
        //{
        //    strOrganisationViewModel = StrOrganisationViewModel
        //});

    }

}
