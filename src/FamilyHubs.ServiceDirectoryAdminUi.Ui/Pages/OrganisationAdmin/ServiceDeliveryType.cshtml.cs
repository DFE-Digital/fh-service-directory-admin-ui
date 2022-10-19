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
    public string LastPage { get; set; } = default!;
    public string UserFlow { get; set; } = default!;

    public Dictionary<int, string> DictServiceDelivery = new();
    
    private readonly ISessionService _session;

    [BindProperty]
    public List<string> ServiceDeliverySelection { get; set; } = default!;

    [BindProperty]
    public bool ValidationValid { get; set; } = true;

    public ServiceDeliveryTypeModel(ISessionService sessionService)
    {
        _session = sessionService;
    }
    public void OnGet(string strOrganisationViewModel)
    {
        LastPage = _session.RetrieveLastPageName(HttpContext);
        UserFlow = _session.RetrieveUserFlow(HttpContext);

        var myEnumDescriptions = from ServiceDelivery n in Enum.GetValues(typeof(ServiceDelivery))
                                 select new { Id = (int)n, Name = Utility.GetEnumDescription(n) };

        foreach (var myEnumDescription in myEnumDescriptions)
        {
            if (myEnumDescription.Id == 0)
                continue;
            DictServiceDelivery[myEnumDescription.Id] = myEnumDescription.Name;
        }

        var organisationViewModel = _session.RetrieveOrganisationWithService(HttpContext) ?? new OrganisationViewModel();
        if (organisationViewModel != null && organisationViewModel.ServiceDeliverySelection != null)
        {
            ServiceDeliverySelection = organisationViewModel.ServiceDeliverySelection;
        }

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

        var organisationViewModel = _session.RetrieveOrganisationWithService(HttpContext) ?? new OrganisationViewModel();
        organisationViewModel.ServiceDeliverySelection = ServiceDeliverySelection;
        _session.StoreOrganisationWithService(HttpContext, organisationViewModel);

        if (ServiceDeliverySelection.Contains("1"))
            return RedirectToPage("/OrganisationAdmin/InPersonWhere");

        ClearAddress(organisationViewModel);
        _session.StoreOrganisationWithService(HttpContext, organisationViewModel);

        if (_session.RetrieveLastPageName(HttpContext) == CheckServiceDetailsPageName)
        {
            return RedirectToPage($"/OrganisationAdmin/{CheckServiceDetailsPageName}");
        }
        return RedirectToPage("/OrganisationAdmin/WhoFor");

    }

    private void ClearAddress(OrganisationViewModel organisationViewModel)
    {
        organisationViewModel.Address_1 = String.Empty;
        organisationViewModel.City = String.Empty;
        organisationViewModel.Postal_code = String.Empty;
        organisationViewModel.State_province = String.Empty;
        organisationViewModel.InPersonSelection?.Clear();
    }
}
