using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Models.ServiceJourney;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.SharedKernel.Razor.FullPages.SingleAutocomplete;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public class local_authorityModel : ServicePageModel, ISingleAutocompletePageModel
{
    public const int NoSelectionId = -1;

    [BindProperty]
    public string? SelectedValue { get; set; }
    public string Label => "Search and select the local authority area this service is in";
    public string? DisabledOptionValue => NoSelectionId.ToString();
    public IEnumerable<ISingleAutocompleteOption> Options { get; private set; } = Enumerable.Empty<ISingleAutocompleteOption>();

    private readonly IServiceDirectoryClient _serviceDirectoryClient;

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

        SelectedValue = ServiceModel!.ServiceType == ServiceTypeArg.La
            ? ServiceModel.OrganisationId.ToString()
            : ServiceModel.LaOrganisationId.ToString();
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
            OrganisationType.LA);

        Options = organisations
            .OrderBy(o => o.Name)
            .Select(x => new SingleAutocompleteOption(x.Id.ToString(), x.Name));
    }

    protected override IActionResult OnPostWithModel()
    {
        if (!long.TryParse(SelectedValue, out var laOrganisationId) || laOrganisationId == NoSelectionId)
        {
            return RedirectToSelf(ErrorId.Local_Authority__NoLaSelected);
        }

        ServiceModel!.Updated = ServiceModel.Updated || HasBeenUpdated(laOrganisationId);

        if (ServiceModel!.ServiceType == ServiceTypeArg.La)
        {
            ServiceModel.OrganisationId = laOrganisationId;
        }
        ServiceModel.LaOrganisationId = laOrganisationId;

        return NextPage();
    }

    private bool HasBeenUpdated(long organisationId)
    {
        //todo: check: no need to check la org id when vcs service, as either the user
        // will change the vcs service (as they'll have to select from a different set) and continue
        // which will set the updated flag
        // or they'll back out, and the original data (& updated flag) will be restored
        return ServiceModel!.OrganisationId != organisationId;
    }
}