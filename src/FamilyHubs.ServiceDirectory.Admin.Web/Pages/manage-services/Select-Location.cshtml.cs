using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public class Select_LocationModel : ServicePageModel
{
    public Select_LocationModel(IRequestDistributedCache connectionRequestCache)
        : base(ServiceJourneyPage.Select_Location, connectionRequestCache)
    {
    }

    protected override IActionResult OnPostWithModel()
    {
        return NextPage();
    }
}