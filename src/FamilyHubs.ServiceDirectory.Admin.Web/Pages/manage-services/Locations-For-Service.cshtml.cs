using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models.ServiceJourney;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public class Locations_For_ServiceModel : ServicePageModel
{
    public List<ServiceLocationModel> Locations { get; set; } = new();

    public Locations_For_ServiceModel(
        IRequestDistributedCache connectionRequestCache)
        : base(ServiceJourneyPage.Locations_For_Service, connectionRequestCache)
    {
    }

    protected override void OnGetWithModel()
    {
        Locations = ServiceModel!.Locations;
        if (ServiceModel.CurrentLocation != null)
        {
            Locations.Add(ServiceModel.CurrentLocation);
        }
    }

    protected override IActionResult OnPostWithModel()
    {
        //todo: magic string
        string action = Request.Form["action"].ToString();
        if (action == "AddAnotherLocation")
        {
            ServiceModel!.Locations.Add(ServiceModel.CurrentLocation!);
            ServiceModel.CurrentLocation = null;
            //todo: better to redirect or call nextpage with a flag?
            return RedirectToServicePage(ServiceJourneyPage.Select_Location, Flow);
        }
        return NextPage();
    }
}