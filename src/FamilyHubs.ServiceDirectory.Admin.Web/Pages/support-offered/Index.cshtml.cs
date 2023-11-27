using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Authorization;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.support_offered;

[Authorize(Roles = RoleGroups.AdminRole)]
public class IndexModel : ServiceWithCachePageModel
{
    protected IndexModel(IRequestDistributedCache connectionRequestCache)
        : base(ServiceJourneyPage.Support_Offered, connectionRequestCache)
    {
    }
}