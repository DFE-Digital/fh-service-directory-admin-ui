using Application.Common.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using LAHub.Domain.OpenReferralEnities;
using LAHub.Domain.RecordEntities;
using WebUI.Models;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;

public interface IViewModelToApiModelHelper
{
    Task<OpenReferralOrganisationWithServicesRecord> GetOrganisation(OrganisationViewModel viewModel);
}

public class ViewModelToApiModelHelper : IViewModelToApiModelHelper
{
    private readonly IOpenReferralOrganisationAdminClientService _openReferralOrganisationAdminClientService;
    public ViewModelToApiModelHelper(IOpenReferralOrganisationAdminClientService openReferralOrganisationAdminClientService)
    {
        _openReferralOrganisationAdminClientService = openReferralOrganisationAdminClientService;
    }

    public async Task<OpenReferralOrganisationWithServicesRecord> GetOrganisation(OrganisationViewModel viewModel)
    {
        var contactId = Guid.NewGuid().ToString();

        var organisation = new OpenReferralOrganisationWithServicesRecord(
            viewModel.Id.ToString(),
            viewModel.Name,
            viewModel.Description,
            viewModel.Logo,
            new Uri(viewModel.Url ?? string.Empty).ToString(),
            viewModel.Url,
            new List<OpenReferralServiceRecord>()
        {
            new OpenReferralServiceRecord(
                viewModel.ServiceId ?? Guid.NewGuid().ToString(),
                viewModel.ServiceName ?? string.Empty,
                viewModel.ServiceDescription,
                null,
                null,
                null,
                null,
                string.Join(",", viewModel.InPersonSelection != null ? viewModel.InPersonSelection.ToArray() : Array.Empty<string>()),
                "pending",
                viewModel.Website,
                viewModel.Email,
                null,
                GetDeliveryTypes(viewModel.ServiceDeliverySelection),
                GetEligibilities("Children", viewModel.MinAge ?? 0, viewModel.MaxAge ?? 0),
                new List<OpenReferralContactRecord>()
                {
                    new OpenReferralContactRecord(
                        contactId,
                        "Service",
                        string.Empty,
                        new List<OpenReferralPhoneRecord>()
                        {
                            new OpenReferralPhoneRecord(contactId, viewModel.Telephone ?? string.Empty)
                        }
                        )
                },
                GetCost(viewModel.IsPayedFor == "Yes", viewModel.PayUnit ?? string.Empty, viewModel.Cost),
                GetLanguages(viewModel.Languages)
                , new List<OpenReferralService_AreaRecord>()
                {
                    new OpenReferralService_AreaRecord(Guid.NewGuid().ToString(), "Local", null, "http://statistics.data.gov.uk/id/statistical-geography/K02000001")

                }
                , new List<OpenReferralServiceAtLocationRecord>()
                {
                    new OpenReferralServiceAtLocationRecord(
                        Guid.NewGuid().ToString(),
                        new OpenReferralLocationRecord(
                            Guid.NewGuid().ToString(),
                            "Our Location",
                            "",
                            viewModel?.Latitude ?? 0.0D,
                            viewModel?.Longtitude ?? 0.0D,
                            new List<OpenReferralPhysical_AddressRecord>()
                            {
                                new OpenReferralPhysical_AddressRecord(
                                    Guid.NewGuid().ToString(),
                                    viewModel?.Address_1 ?? string.Empty,
                                    viewModel?.City ?? string.Empty,
                                    viewModel?.Postal_code ?? string.Empty,
                                    "England",
                                    viewModel?.State_province ?? string.Empty
                                    )
                            }
                        ))
                }
                , await GetOpenReferralTaxonomies(viewModel?.TaxonomySelection)
                )
            });

        return organisation;
    }

    private static List<OpenReferralCost_OptionRecord> GetCost(bool isPayedFor, string payUnit, decimal? cost)
    {
        List<OpenReferralCost_OptionRecord> list = new();

        if (isPayedFor && cost != null)
        {
            list.Add(new OpenReferralCost_OptionRecord(Guid.NewGuid().ToString(), payUnit ?? string.Empty, cost.Value, null, null, null, null));
        }

        return list;
    }

    private static List<OpenReferralServiceDeliveryRecord> GetDeliveryTypes(List<string>? serviceDeliverySelection)
    {
        List<OpenReferralServiceDeliveryRecord> list = new();
        if (serviceDeliverySelection == null)
            return list;

        foreach (var serviceDelivery in serviceDeliverySelection)
        {
            switch (serviceDelivery)
            {

                case "In Person":
                    list.Add(new OpenReferralServiceDeliveryRecord(Guid.NewGuid().ToString(), ServiceDelivery.InPerson));
                    break;
                case "Online":
                    list.Add(new OpenReferralServiceDeliveryRecord(Guid.NewGuid().ToString(), ServiceDelivery.Online));
                    break;
                case "Telephone":
                    list.Add(new OpenReferralServiceDeliveryRecord(Guid.NewGuid().ToString(), ServiceDelivery.Telephone));
                    break;
            }
        }

        return list;
    }

    private static List<OpenReferralEligibilityRecord> GetEligibilities(string whoFor, int minAge, int maxAge)
    {
        List<OpenReferralEligibilityRecord> list = new()
        {
            new OpenReferralEligibilityRecord(Guid.NewGuid().ToString(), whoFor, maxAge, minAge)
        };

        return list;
    }

    private async Task<List<OpenReferralService_TaxonomyRecord>> GetOpenReferralTaxonomies(List<string>? taxonomySelection)
    {
        List<OpenReferralService_TaxonomyRecord> openReferralTaxonomyRecords = new();

        PaginatedList<OpenReferralTaxonomyRecord> taxonomies = await _openReferralOrganisationAdminClientService.GetTaxonomyList(1, 9999);

        if (taxonomies != null && taxonomySelection != null)
        {
            foreach (string taxonomyKey in taxonomySelection)
            {
                OpenReferralTaxonomyRecord? taxonomy = taxonomies.Items.FirstOrDefault(x => x.Id == taxonomyKey);
                if (taxonomy != null)
                {
                    openReferralTaxonomyRecords.Add(new OpenReferralService_TaxonomyRecord(Guid.NewGuid().ToString(), taxonomy));
                }
            }
        }

        return openReferralTaxonomyRecords;
    }

    private static List<OpenReferralLanguageRecord> GetLanguages(List<string>? viewModellanguages)
    {
        List<OpenReferralLanguageRecord> languages = new();

        if (viewModellanguages != null)
        {
            foreach (string lang in viewModellanguages)
            {
                languages.Add(new OpenReferralLanguageRecord(Guid.NewGuid().ToString(), lang));
            }
        }

        return languages;
    }
}
