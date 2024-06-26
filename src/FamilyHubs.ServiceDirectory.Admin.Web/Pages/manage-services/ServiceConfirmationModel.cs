﻿using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models.ServiceJourney;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Authorization;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

[Authorize(Roles = RoleGroups.AdminRole)]
public class ServiceConfirmationModel : HeaderPageModel
{
    private readonly IRequestDistributedCache _cache;

    public string? ServiceType { get; set; }

    public ServiceConfirmationModel(IRequestDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task OnGetAsync(string? serviceType)
    {
        ServiceType = serviceType;

        var familyHubsUser = HttpContext.GetFamilyHubsUser();

        await _cache.RemoveAsync<ServiceModel<object>>(familyHubsUser.Email);
    }
}