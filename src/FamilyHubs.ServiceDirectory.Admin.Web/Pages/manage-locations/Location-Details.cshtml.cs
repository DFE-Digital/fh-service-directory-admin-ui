using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Authorization;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_locations;

[Authorize(Roles = RoleGroups.AdminRole)]
public class Location_DetailsModel : LocationPageModel
{
    public Location_DetailsModel(IRequestDistributedCache connectionRequestCache)
        : base(LocationJourneyPage.Location_Details, connectionRequestCache)
    {
    }

    protected override async Task OnGetWithModelAsync(CancellationToken cancellationToken)
    {
    }

    //protected override async Task<IActionResult> OnPostWithModelAsync(CancellationToken cancellationToken)
    //{
    //}
}