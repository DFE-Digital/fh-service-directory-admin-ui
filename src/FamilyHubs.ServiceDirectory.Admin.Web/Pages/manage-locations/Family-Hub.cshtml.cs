using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_locations;

public class Family_HubModel : LocationPageModel
{
    public Family_HubModel(IRequestDistributedCache connectionRequestCache)
        : base(LocationJourneyPage.Family_Hub, connectionRequestCache)
    {
    }

    protected override Task<IActionResult> OnPostWithModelAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(NextPage());
    }
}