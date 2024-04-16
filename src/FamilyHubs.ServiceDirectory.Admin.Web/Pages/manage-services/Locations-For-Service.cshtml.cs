using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models.ServiceJourney;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public class Locations_For_ServiceModel : ServicePageModel
{
    public const string SubmitAction = "action";
    public const string SubmitAction_Continue = "continue";
    public const string SubmitAction_AddAnotherLocation = "add";

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
        string action = Request.Form[SubmitAction].ToString();
        if (action == SubmitAction_AddAnotherLocation)
        {
            if (ServiceModel!.CurrentLocation != null)
            {
                ServiceModel.Locations.Add(ServiceModel.CurrentLocation);
            }
            ServiceModel.CurrentLocation = null;

            return RedirectToServicePage(ServiceJourneyPage.Select_Location, Flow);
        }
        return NextPage();
    }
}