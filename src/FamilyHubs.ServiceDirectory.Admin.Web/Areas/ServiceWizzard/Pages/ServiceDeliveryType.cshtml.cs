using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.ServiceWizzard.Pages;

public class ServiceDeliveryTypeModel : BasePageModel
{
    public Dictionary<int, string> DictServiceDelivery { get; private set; }

    [BindProperty]
    public List<string> ServiceDeliverySelection { get; set; } = default!;

    [BindProperty]
    public bool ValidationValid { get; set; } = true;
    public ServiceDeliveryTypeModel(IRequestDistributedCache requestCache)
        : base(requestCache)
    {
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

        OrganisationViewModel? viewModel = await GetOrganisationViewModel();
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

        OrganisationViewModel? viewModel = await GetOrganisationViewModel();
        if (viewModel == null)
        {
            viewModel = new OrganisationViewModel();
        }
        viewModel.ServiceDeliverySelection = ServiceDeliverySelection;

        await SetCacheAsync(viewModel);

        if (ServiceDeliverySelection.Contains("1"))
            return RedirectToPage("InPersonWhere", new { area = "ServiceWizzard" });

        ClearAddress(viewModel);

        await SetCacheAsync(viewModel);

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