using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Models.ServiceJourney;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.SharedKernel.Razor.FullPages.SingleAutocomplete;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

//todo: component to replace this, vcs org, which local authority, select location:
// single autocomplete page

public class local_authorityModel : ServicePageModel, ISingleAutocompletePageModel
{
    public const int NoSelectionId = -1;

    [BindProperty]
    public string? SelectedValue { get; set; }
    public string Label => "Search and select the local authority area this service is in";
    public string? DisabledOptionValue => NoSelectionId.ToString();
    public IEnumerable<ISingleAutocompleteOption> Options { get; private set; } = Enumerable.Empty<ISingleAutocompleteOption>();

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
        await PopulateOptionsWithOrganisations(cancellationToken);
    }

    protected override async Task OnGetWithErrorAsync(CancellationToken cancellationToken)
    {
        await PopulateOptionsWithOrganisations(cancellationToken);
    }

    private async Task PopulateOptionsWithOrganisations(CancellationToken cancellationToken)
    {
        //todo: order autocomplete according so that returns matches at start first, rather than alphabetically
        var organisations = await _serviceDirectoryClient.GetOrganisations(cancellationToken);
        Options = organisations
            .Where(o => o.OrganisationType == OrganisationType.LA)
            .OrderBy(o => o.Name)
            .Select(x => new SingleAutocompleteOption(x.Id.ToString(), x.Name));
    }

    protected override IActionResult OnPostWithModel()
    {
        if (!long.TryParse(SelectedValue, out var laOrganisationId) || laOrganisationId == NoSelectionId)
        {
            return RedirectToSelf(ErrorId.Local_Authority__NoLaSelected);
        }

        ServiceModel!.OrganisationId = laOrganisationId;

        return NextPage();
    }
}