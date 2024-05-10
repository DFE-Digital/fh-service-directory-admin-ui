using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Models.ServiceJourney;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

//todo: component to replace this, vcs org, which local authority, select location:
// single autocomplete page

public class local_authorityModel : ServicePageModel
{
    public const int NoSelectionId = -1;

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
        await PopulateOrganisations(cancellationToken);
    }

    protected override async Task OnGetWithErrorAsync(CancellationToken cancellationToken)
    {
        await PopulateOrganisations(cancellationToken);
    }

    private async Task PopulateOrganisations(CancellationToken cancellationToken)
    {
        //todo: order autocomplete according so that returns matches at start first, rather than alphabetically
        Organisations = await _serviceDirectoryClient.GetOrganisations(cancellationToken);
        Organisations = Organisations
            .Where(o => o.OrganisationType == OrganisationType.LA)
            .OrderBy(o => o.Name);
    }

    protected override IActionResult OnPostWithModel()
    {
        string laOrganisationIdString = Request.Form["la"]!;

        if (!long.TryParse(laOrganisationIdString, out var laOrganisationId) || laOrganisationId == NoSelectionId)
        {
            return RedirectToSelf(ErrorId.Local_Authority__NoLaSelected);
        }

        ServiceModel!.OrganisationId = long.Parse(laOrganisationIdString);

        return NextPage();
    }
}