using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models.ServiceJourney;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.SharedKernel.Razor.FullPages.SingleAutocomplete;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public class vcs_organisationModel : ServicePageModel
{
    public const int NoSelectionId = -1;

    [BindProperty]
    public string? SelectedValue { get; set; }
    public string Label => "Search and select the VCS organisation that runs this service";
    public string? DisabledOptionValue => NoSelectionId.ToString();
    public IEnumerable<ISingleAutocompleteOption> Options { get; private set; } = Enumerable.Empty<ISingleAutocompleteOption>();

    public vcs_organisationModel(IRequestDistributedCache connectionRequestCache)
        : base(ServiceJourneyPage.Vcs_Organisation, connectionRequestCache)
    {
    }

    protected override void OnGetWithModel()
    {
    }

    protected override IActionResult OnPostWithModel()
    {
        return NextPage();
    }
}