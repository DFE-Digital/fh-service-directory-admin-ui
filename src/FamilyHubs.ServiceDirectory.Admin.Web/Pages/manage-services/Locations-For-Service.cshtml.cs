using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public class Locations_For_ServiceModel : ServicePageModel
{
    public Locations_For_ServiceModel(IRequestDistributedCache connectionRequestCache)
        : base(ServiceJourneyPage.Locations_For_Service, connectionRequestCache)
    {
    }

    protected override void OnGetWithModel()
    {
    }

    protected override IActionResult OnPostWithModel()
    {
        return NextPage();
    }
}