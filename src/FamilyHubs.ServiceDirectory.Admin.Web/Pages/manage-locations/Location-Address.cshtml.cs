using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_locations;

public class Location_AddressModel : LocationPageModel
{
    public Location_AddressModel(IRequestDistributedCache connectionRequestCache)
        : base(LocationJourneyPage.Location_Address, connectionRequestCache)
    {
    }

    protected override Task<IActionResult> OnPostWithModelAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(NextPage());
    }
}