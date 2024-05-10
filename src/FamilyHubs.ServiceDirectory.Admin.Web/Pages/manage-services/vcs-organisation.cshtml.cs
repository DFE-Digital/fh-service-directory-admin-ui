using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models.ServiceJourney;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public class vcs_organisationModel : ServicePageModel
{
    public vcs_organisationModel(IRequestDistributedCache connectionRequestCache)
        : base(ServiceJourneyPage.Vcs_Organisation, connectionRequestCache)
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