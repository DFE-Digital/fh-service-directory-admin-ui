using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public class Service_DetailModel : PageModel
{
    public long ServiceId { get; set; }
    public string? Title { get; set; }

    private readonly IServiceDirectoryClient _serviceDirectoryClient;

    public Service_DetailModel(IServiceDirectoryClient serviceDirectoryClient)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
    }

    public async Task OnGetAsync(long serviceId)
    {
        ServiceId = serviceId;
        var service = await _serviceDirectoryClient.GetServiceById(serviceId);
        Title = service?.Name;
    }
}