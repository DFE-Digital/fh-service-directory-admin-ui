using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services
{
    public class who_forModel : ServiceWithCachePageModel
    {
        public who_forModel(IRequestDistributedCache connectionRequestCache)
            : base(ServiceJourneyPage.Who_For, connectionRequestCache)
        {
        }

        protected override Task<IActionResult> OnPostWithModelAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(NextPage());
        }
    }
}
