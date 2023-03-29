using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static FamilyHubs.ServiceDirectory.Admin.Core.Constants.PageConfiguration;


namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.OrganisationAdmin;

public class ServiceDeliveryTypeModel : PageModel
{
    public string LastPage { get; set; } = default!;
    public string UserFlow { get; set; } = default!;

    public Dictionary<int, string> DictServiceDelivery = new Dictionary<int, string>();

    private readonly IRedisCacheService _redis;

    [BindProperty]
    public List<string> ServiceDeliverySelection { get; set; } = default!;

    [BindProperty]
    public bool ValidationValid { get; set; } = true;

    public ServiceDeliveryTypeModel(
        IRedisCacheService redisCacheService)
    {
        _redis = redisCacheService;
    }
    public void OnGet(string strOrganisationViewModel)
    {
        LastPage = _redis.RetrieveLastPageName();
        UserFlow = _redis.RetrieveUserFlow();

        var myEnumDescriptions = from ServiceDeliveryType n in Enum.GetValues(typeof(ServiceDeliveryType))
                                 select new { Id = (int)n, Name = Utility.GetEnumDescription(n) };

        foreach (var myEnumDescription in myEnumDescriptions)
        {
            if (myEnumDescription.Id == 0)
                continue;
            DictServiceDelivery[myEnumDescription.Id] = myEnumDescription.Name;
        }
        
        var organisationViewModel = _redis.RetrieveOrganisationWithService() ?? new OrganisationViewModel();

        if (organisationViewModel.ServiceDeliverySelection != null)
        {
            ServiceDeliverySelection = organisationViewModel.ServiceDeliverySelection;
        }
    }

    public IActionResult OnPost()
    {
        var myEnumDescriptions = from ServiceDeliveryType n in Enum.GetValues(typeof(ServiceDeliveryType))
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

        
        var organisationViewModel = _redis.RetrieveOrganisationWithService() ?? new OrganisationViewModel();
        organisationViewModel.ServiceDeliverySelection = ServiceDeliverySelection;
        _redis.StoreOrganisationWithService(organisationViewModel);

        if (ServiceDeliverySelection.Contains("1"))
            return RedirectToPage("/OrganisationAdmin/InPersonWhere");

        ClearAddress(organisationViewModel);
        
        _redis.StoreOrganisationWithService(organisationViewModel);


        return RedirectToPage(_redis.RetrieveLastPageName() == CheckServiceDetailsPageName 
            ? $"/OrganisationAdmin/{CheckServiceDetailsPageName}" 
            : "/OrganisationAdmin/WhoFor");
    }

    private void ClearAddress(OrganisationViewModel organisationViewModel)
    {
        organisationViewModel.Address1 = string.Empty;
        organisationViewModel.City = string.Empty;
        organisationViewModel.PostalCode = string.Empty;
        organisationViewModel.StateProvince = string.Empty;
        organisationViewModel.InPersonSelection?.Clear();
    }
}
