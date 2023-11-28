using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services
{
    public class who_forModel : ServiceWithCachePageModel
    {
        private readonly IServiceDirectoryClient _serviceDirectoryClient;

        public who_forModel(
            IRequestDistributedCache connectionRequestCache,
            IServiceDirectoryClient serviceDirectoryClient)
            : base(ServiceJourneyPage.Who_For, connectionRequestCache)
        {
            _serviceDirectoryClient = serviceDirectoryClient;
        }

        protected override async Task OnGetWithModelAsync(CancellationToken cancellationToken)
        {
            switch (Flow)
            {
                case JourneyFlow.Edit:
                    var service = await _serviceDirectoryClient.GetServiceById(ServiceId!.Value, cancellationToken);
                    break;

                default:
                    // pick up values from the cache
                    break;
            }
        }

        protected override Task<IActionResult> OnPostWithModelAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(NextPage());
        }
    }
}
