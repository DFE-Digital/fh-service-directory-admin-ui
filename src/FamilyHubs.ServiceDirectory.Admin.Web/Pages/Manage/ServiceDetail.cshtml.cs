using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using Microsoft.AspNetCore.Authorization;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.Manage;


[Authorize]
public class ServiceDetailModel : HeaderPageModel
{
    public string? Title { get; set; }

    private readonly IServiceDirectoryClient _serviceDirectoryClient;

    public ServiceDetailModel(IServiceDirectoryClient serviceDirectoryClient)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
    }

    public void OnGet()
    {

    }
}