using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Authorization;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

[Authorize(Roles = RoleGroups.AdminRole)]
public class Service_DetailModel : ServicePageModel
{
    public string? ForChildren { get; set; }
    public string? CostDescription { get; set; }
    public IEnumerable<string> When { get; set; } = Enumerable.Empty<string>();
    public IEnumerable<string> TimeDescription { get; set; } = Enumerable.Empty<string>();

    private readonly IServiceDirectoryClient _serviceDirectoryClient;

    public Service_DetailModel(
        IRequestDistributedCache connectionRequestCache,
        IServiceDirectoryClient serviceDirectoryClient)
    : base(ServiceJourneyPage.Service_Detail, connectionRequestCache)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
    }

    protected override void OnGetWithModel()
    {
        ForChildren = GetForChildren();
        CostDescription = GetCostDescription(service);
        When = service.GetWeekdaysAndWeekends();
        TimeDescription = service.GetTimeDescription();
    }

    private string GetCostDescription(ServiceDto service)
    {
        if (service.CostOptions.Count > 0)
        {
            return "Yes, it costs money to use. " + service.CostOptions.First().AmountDescription;
        }
        return "No, it is free to use.";
    }
}