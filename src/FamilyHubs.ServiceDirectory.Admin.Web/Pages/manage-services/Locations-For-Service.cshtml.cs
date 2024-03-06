using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public class Locations_For_ServiceModel : ServicePageModel
{
    public List<LocationDto> Locations { get; private set; }

    private readonly IServiceDirectoryClient _serviceDirectoryClient;

    public Locations_For_ServiceModel(
        IRequestDistributedCache connectionRequestCache,
        IServiceDirectoryClient serviceDirectoryClient)
        : base(ServiceJourneyPage.Locations_For_Service, connectionRequestCache)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
    }

    protected override async Task OnGetWithModelAsync(CancellationToken cancellationToken)
    {
        Locations = await GetLocations(_serviceDirectoryClient, cancellationToken);
    }

    protected override IActionResult OnPostWithModel()
    {
        //todo: magic string
        string action = Request.Form["action"].ToString();
        if (action == "AddAnotherLocation")
        {
            ServiceModel!.Locations.Add(new ServiceLocationModel {Id = ServiceModel.CurrentLocation!.Value});
            ServiceModel.CurrentLocation = null;
            //todo: better to redirect or call nextpage with a flag?
            return RedirectToServicePage(ServiceJourneyPage.Select_Location, Flow);
        }
        return NextPage();
    }

    //public async Task<IActionResult> OnAddAnotherLocation()
    //{
    //    Locations
    //    return RedirectToPage("/manage-locations/Add-Location", new { serviceId = ServiceModel!.Id });  
    //}
}