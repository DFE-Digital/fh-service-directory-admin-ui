using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Authorization;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

[Authorize(Roles = RoleGroups.AdminRole)]
public class Support_OfferedModel : ServiceWithCachePageModel
{ 
    public Support_OfferedModel(IRequestDistributedCache connectionRequestCache)
        : base(ServiceJourneyPage.Support_Offered, connectionRequestCache)
    {
    }
}