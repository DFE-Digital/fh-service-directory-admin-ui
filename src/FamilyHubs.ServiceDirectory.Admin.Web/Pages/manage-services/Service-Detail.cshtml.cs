using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

[Authorize(Roles = RoleGroups.AdminRole)]
public class Service_DetailModel : PageModel
{
    public long ServiceId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string ForChildren { get; set; }

    private readonly IServiceDirectoryClient _serviceDirectoryClient;

    public Service_DetailModel(IServiceDirectoryClient serviceDirectoryClient)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
    }

    public async Task OnGetAsync(long serviceId)
    {
        ServiceId = serviceId;
        var service = await _serviceDirectoryClient.GetServiceById(serviceId);
        Name = service.Name;
        Description = service.Description;
        ForChildren = GetForChildren(service);
    }

    private static string GetForChildren(ServiceDto service)
    {
        var eligibility = service.Eligibilities.FirstOrDefault();
        if (eligibility == null)
        {
            return "No";
        }

        // could be 0 years old (like Find & Connect) or 0 to 12 months, but 0 to 12 months to 1 year, for example looks odd!
        return $"Yes - {AgeToString(eligibility.MinimumAge)} years old to {AgeToString(eligibility.MaximumAge)} years old";
    }

    private static string AgeToString(int age)
    {
        return age == 127 ? "25+" : age.ToString();
    }
}