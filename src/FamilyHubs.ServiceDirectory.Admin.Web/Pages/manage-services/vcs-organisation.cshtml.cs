using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Models.ServiceJourney;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.SharedKernel.Razor.FullPages.SingleAutocomplete;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public class vcs_organisationModel : ServicePageModel, ISingleAutocompletePageModel
{
    private readonly IServiceDirectoryClient _serviceDirectoryClient;
    public const int NoSelectionId = -1;

    [BindProperty]
    public string? SelectedValue { get; set; }
    public string Label => "Search and select the VCS organisation that runs this service";
    public string? DisabledOptionValue => NoSelectionId.ToString();
    public IEnumerable<ISingleAutocompleteOption> Options { get; private set; } = Enumerable.Empty<ISingleAutocompleteOption>();

    public vcs_organisationModel(
        IServiceDirectoryClient serviceDirectoryClient,
        IRequestDistributedCache connectionRequestCache)
        : base(ServiceJourneyPage.Vcs_Organisation, connectionRequestCache)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
    }

    protected override async Task OnGetWithModelAsync(CancellationToken cancellationToken)
    {
        await PopulateOptionsWithOrganisations(cancellationToken);

        SelectedValue = ServiceModel!.OrganisationId.ToString();
    }

    protected override async Task OnGetWithErrorAsync(CancellationToken cancellationToken)
    {
        await PopulateOptionsWithOrganisations(cancellationToken);
    }

    private async Task PopulateOptionsWithOrganisations(CancellationToken cancellationToken)
    {
        //todo: order autocomplete according so that returns matches at start first, rather than alphabetically
        var organisations = await _serviceDirectoryClient.GetOrganisations(
            cancellationToken,
            OrganisationType.VCFS,
            ServiceModel!.LaOrganisationId);

        Options = organisations
            .OrderBy(o => o.Name)
            .Select(x => new SingleAutocompleteOption(x.Id.ToString(), x.Name));
    }

    protected override IActionResult OnPostWithModel()
    {
        if (!long.TryParse(SelectedValue, out var vcsOrganisationId) || vcsOrganisationId == NoSelectionId)
        {
            return RedirectToSelf(ErrorId.Vcs_Organisation__NoVcsSelected);
        }

        ServiceModel!.Updated = ServiceModel.Updated || HasBeenUpdated(vcsOrganisationId);

        ServiceModel!.OrganisationId = vcsOrganisationId;

        return NextPage();
    }

    private bool HasBeenUpdated(long organisationId)
    {
        return ServiceModel!.OrganisationId != organisationId;
    }
}