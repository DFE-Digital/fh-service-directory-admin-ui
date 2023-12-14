using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public class costModel : ServicePageModel
{
    private readonly IServiceDirectoryClient _serviceDirectoryClient;

    public costModel(
        IRequestDistributedCache connectionRequestCache,
        IServiceDirectoryClient serviceDirectoryClient)
        : base(ServiceJourneyPage.Cost, connectionRequestCache)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
    }
}