using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models.ServiceJourney;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public class local_authorityModel : ServicePageModel
{
    private readonly IServiceDirectoryClient _serviceDirectoryClient;

    public IEnumerable<OrganisationDto> Organisations { get; private set; }

    public local_authorityModel(
        IServiceDirectoryClient serviceDirectoryClient,
        IRequestDistributedCache connectionRequestCache)
        : base(ServiceJourneyPage.Local_Authority, connectionRequestCache)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
    }

    protected override async Task OnGetWithModelAsync(CancellationToken cancellationToken)
    {
        Organisations = await _serviceDirectoryClient.GetOrganisations(cancellationToken);
        Organisations = Organisations
            .Where(o => o.OrganisationType == OrganisationType.LA)
            .OrderBy(o => o.Name);
    }

    protected override IActionResult OnPostWithModel()
    {
        return NextPage();
    }
}