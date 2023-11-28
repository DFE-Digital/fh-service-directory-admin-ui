using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services
{
    [Authorize(Roles = RoleGroups.AdminRole)]
    public class who_forModel : ServiceWithCachePageModel
    {
        private readonly IServiceDirectoryClient _serviceDirectoryClient;

        [BindProperty]
        public string? FromAge { get; set; }

        [BindProperty]
        public string? ToAge { get; set; }

        public IEnumerable<SelectListItem> MinimumAges => MinimumAgeOptions;
        public IEnumerable<SelectListItem> MaximumAges => MinimumAgeOptions.Concat(ExtraMaximumAgeOptions);

        public static SelectListItem[] MinimumAgeOptions { get; set; } =
        {
            new() { Value="-1", Text="Select age", Selected = true},
            new() { Value="0", Text="0 to 12 months"},
            new() { Value="1", Text="1 year old"},
            new() { Value="2", Text="2 years old"},
            new() { Value="3", Text="3 years old"},
            new() { Value="4", Text="4 years old"},
            new() { Value="5", Text="5 years old"},
            new() { Value="6", Text="6 years old"},
            new() { Value="7", Text="7 years old"},
            new() { Value="8", Text="8 years old"},
            new() { Value="9", Text="9 years old"},
            new() { Value="10", Text="10 years old"},
            new() { Value="11", Text="11 years old"},
            new() { Value="12", Text="12 years old"},
            new() { Value="13", Text="13 years old"},
            new() { Value="14", Text="14 years old"},
            new() { Value="15", Text="15 years old"},
            new() { Value="16", Text="16 years old"},
            new() { Value="17", Text="17 years old"},
            new() { Value="18", Text="18 years old"},
            new() { Value="19", Text="19 years old"},
            new() { Value="20", Text="20 years old"},
            new() { Value="21", Text="21 years old"},
            new() { Value="22", Text="22 years old"},
            new() { Value="23", Text="23 years old"},
            new() { Value="24", Text="24 years old"},
            new() { Value="25", Text="25 years old"},
        };

        private static SelectListItem[] ExtraMaximumAgeOptions { get; set; } =
        {
            new() { Value = "127", Text = "25+ years old" }
        };

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
                    var eligibility = service.Eligibilities.FirstOrDefault();
                    if (eligibility != null) // && eligibility.EligibilityType != EligibilityType.NotSet)
                    {
                        FromAge = eligibility.MinimumAge.ToString();
                        ToAge = eligibility.MaximumAge.ToString();
                    }
                    break;

                default:
                    if (ServiceModel!.MinimumAge != null)
                    {
                        FromAge = ServiceModel.MinimumAge.ToString();
                    }

                    if (ServiceModel.MaximumAge != null)
                    {
                        ToAge = ServiceModel.MaximumAge.ToString();
                    }
                    break;
            }
        }

        protected override Task<IActionResult> OnPostWithModelAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(NextPage());
        }
    }
}
