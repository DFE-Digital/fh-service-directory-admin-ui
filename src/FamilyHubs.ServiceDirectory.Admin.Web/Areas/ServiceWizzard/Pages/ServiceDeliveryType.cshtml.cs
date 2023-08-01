using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.ServiceWizzard.Pages;

public class ServiceDeliveryTypeModel : PageModel
{
    private readonly IRequestDistributedCache _requestCache;

    public Dictionary<int, string> DictServiceDelivery { get; private set; }

    [BindProperty]
    public List<string> ServiceDeliverySelection { get; set; } = default!;

    [BindProperty]
    public bool ValidationValid { get; set; } = true;
    public ServiceDeliveryTypeModel(IRequestDistributedCache requestCache)
    {
        _requestCache = requestCache;
        DictServiceDelivery = new Dictionary<int, string>();
    }
    public async Task OnGet()
    {
        var myEnumDescriptions = from ServiceDeliveryType n in Enum.GetValues(typeof(ServiceDeliveryType))
                                 select new { Id = (int)n, Name = Utility.GetEnumDescription(n) };

        foreach (var myEnumDescription in myEnumDescriptions)
        {
            if (myEnumDescription.Id == 0)
                continue;
            DictServiceDelivery[myEnumDescription.Id] = myEnumDescription.Name;
        }

        var user = HttpContext.GetFamilyHubsUser();
        OrganisationViewModel? viewModel = await _requestCache.GetAsync(user.Email);
        if (viewModel != null && viewModel.ServiceDeliverySelection != null)
        {
            ServiceDeliverySelection = viewModel.ServiceDeliverySelection;
        }
    }

    public async Task<IActionResult> OnPost()
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

        var user = HttpContext.GetFamilyHubsUser();
        OrganisationViewModel? viewModel = await _requestCache.GetAsync(user.Email);
        if (viewModel == null)
        {
            viewModel = new OrganisationViewModel();
        }
        viewModel.ServiceDeliverySelection = ServiceDeliverySelection;

        await _requestCache.SetAsync(user.Email, viewModel);

        if (ServiceDeliverySelection.Contains("1"))
            return RedirectToPage("InPersonWhere", new { area = "ServiceWizzard" });

        ClearAddress(viewModel);

        await _requestCache.SetAsync(user.Email, viewModel);

        return RedirectToPage("WhoFor", new { area = "ServiceWizzard" });
    }

    private void ClearAddress(OrganisationViewModel organisationViewModel)
    {
        organisationViewModel.Address1 = String.Empty;
        organisationViewModel.City = String.Empty;
        organisationViewModel.PostalCode = String.Empty;
        organisationViewModel.StateProvince = String.Empty;
        organisationViewModel.InPersonSelection?.Clear();
    }
}
