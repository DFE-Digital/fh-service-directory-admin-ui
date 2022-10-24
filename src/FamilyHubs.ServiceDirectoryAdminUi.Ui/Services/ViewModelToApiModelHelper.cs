using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralContacts;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralCostOptions;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralEligibilitys;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralHolidaySchedule;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralLanguages;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralLocations;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralOrganisations;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralPhones;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralPhysicalAddresses;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralRegularSchedule;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServiceAreas;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServiceAtLocations;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServiceDeliverysEx;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServices;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServiceTaxonomys;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralTaxonomys;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OrganisationType;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using FamilyHubs.SharedKernel;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;

public interface IViewModelToApiModelHelper
{
    Task<OpenReferralOrganisationWithServicesDto> GetOrganisation(OrganisationViewModel viewModel);
}

public class ViewModelToApiModelHelper : IViewModelToApiModelHelper
{
    private readonly IOpenReferralOrganisationAdminClientService _openReferralOrganisationAdminClientService;
    public ViewModelToApiModelHelper(IOpenReferralOrganisationAdminClientService openReferralOrganisationAdminClientService)
    {
        _openReferralOrganisationAdminClientService = openReferralOrganisationAdminClientService;
    }

    public async Task<OpenReferralOrganisationWithServicesDto> GetOrganisation(OrganisationViewModel viewModel)
    {
        OpenReferralOrganisationWithServicesDto updateOrganisation = await _openReferralOrganisationAdminClientService.GetOpenReferralOrganisationById(viewModel.Id.ToString());

        var currentService = updateOrganisation?.Services?.FirstOrDefault(x => x.Id == viewModel.ServiceId);

        var contactIdTelephone = Guid.NewGuid().ToString();
        var contactIdTextphone = Guid.NewGuid().ToString();
        if (currentService != null)
        {
            var originalContactTelephone = currentService.Contacts?.FirstOrDefault(c => c.Name == "Telephone");
            var originalContactTextphone = currentService.Contacts?.FirstOrDefault(c => c.Name == "Textphone");
            if (originalContactTelephone != null)
            {
                contactIdTelephone = originalContactTelephone.Id;
            }
            if (originalContactTextphone != null)
            {
                contactIdTextphone = originalContactTextphone.Id;
            }
        }

        OrganisationTypeDto organisationTypeDto = new(string.Empty, string.Empty, string.Empty);
        if (updateOrganisation != null)
            organisationTypeDto = new OrganisationTypeDto(updateOrganisation.OrganisationType.Id, updateOrganisation.OrganisationType.Name, updateOrganisation.OrganisationType.Description);

        var organisation = new OpenReferralOrganisationWithServicesDto(
            viewModel.Id.ToString(),
            organisationTypeDto,
            viewModel.Name,
            viewModel.Description,
            viewModel.Logo,
            new Uri(viewModel.Url ?? string.Empty).ToString(),
            viewModel.Url,
            new List<OpenReferralServiceDto>()
        {
            new OpenReferralServiceDto(
                viewModel.ServiceId ?? Guid.NewGuid().ToString(),
                new ServiceDirectory.Shared.Models.Api.ServiceType.ServiceTypeDto("1", "Information Sharing", ""),
                viewModel.Id.ToString(),
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
                GetDeliveryTypes(viewModel.ServiceDeliverySelection, currentService?.ServiceDelivery),
                GetEligibilities(viewModel.WhoForSelection ?? new List<string>(), viewModel.MinAge ?? 0, viewModel.MaxAge ?? 0, currentService?.Eligibilities),
                new List<OpenReferralContactDto>()
                {
                    new OpenReferralContactDto(
                        contactIdTelephone,
                        "Service",
                        "Telephone",
                        new List<OpenReferralPhoneDto>()
                        {
                            new OpenReferralPhoneDto(contactIdTelephone, viewModel.Telephone ?? string.Empty)
                        }
                    ),
                    new OpenReferralContactDto(
                        contactIdTextphone,
                        "Service",
                        "Textphone",
                        new List<OpenReferralPhoneDto>()
                        {
                            new OpenReferralPhoneDto(contactIdTextphone, viewModel.Textphone ?? string.Empty)
                        }
                    )
                },
                GetCost(viewModel.IsPayedFor == "Yes", viewModel.PayUnit ?? string.Empty, viewModel.Cost, currentService?.Cost_options),
                GetLanguages(viewModel.Languages)
                , new List<OpenReferralServiceAreaDto>()
                {
                    new OpenReferralServiceAreaDto(Guid.NewGuid().ToString(), "Local", null, "http://statistics.data.gov.uk/id/statistical-geography/K02000001")

                }
                ,
                GetServiceAtLocation(viewModel, currentService?.Service_at_locations),
                await GetOpenReferralTaxonomies(viewModel?.TaxonomySelection, currentService?.Service_taxonomys)
                )
            });

        return organisation;


    }

    private static List<OpenReferralServiceAtLocationDto> GetServiceAtLocation(OrganisationViewModel viewModel, ICollection<OpenReferralServiceAtLocationDto>? currentServiceAtLocations)
    {
        var id = Guid.NewGuid().ToString();
        var locationId = Guid.NewGuid().ToString();
        var physicalAddressId = Guid.NewGuid().ToString();
        if (currentServiceAtLocations != null && currentServiceAtLocations.Any())
        {
            var currentServiceAtLocation = currentServiceAtLocations.FirstOrDefault(x => x.Location.Name == "Our Location");
            if (currentServiceAtLocation != null)
            {
                id = currentServiceAtLocation.Id;
                locationId = currentServiceAtLocation.Location.Id;
                if(currentServiceAtLocation.Location.Physical_addresses != null && currentServiceAtLocation.Location.Physical_addresses.Count == 1)
                {
                    physicalAddressId = currentServiceAtLocation.Location.Physical_addresses.First().Id;
                }
            }
        }

        return new List<OpenReferralServiceAtLocationDto>()
        {
            new OpenReferralServiceAtLocationDto(
                id,
                new OpenReferralLocationDto(
                    locationId,
                    "Our Location",
                    "",
                    viewModel?.Latitude ?? 0.0D,
                    viewModel?.Longtitude ?? 0.0D,
                    new List<OpenReferralPhysicalAddressDto>()
                    {
                        new OpenReferralPhysicalAddressDto(
                            physicalAddressId,
                            viewModel?.Address_1 ?? string.Empty,
                            viewModel?.City ?? string.Empty,
                            viewModel?.Postal_code ?? string.Empty,
                            "England",
                            viewModel?.State_province ?? string.Empty
                            )
                    }
                ),
                new List<OpenReferralRegularScheduleDto>(),
                new List<OpenReferralHolidayScheduleDto>()
                )
        };
    }

    private static List<OpenReferralCostOptionDto> GetCost(bool isPayedFor, string payUnit, decimal? cost, ICollection<OpenReferralCostOptionDto>? costOptions)
    {
        List<OpenReferralCostOptionDto> list = new();

        if (isPayedFor && cost != null)
        {
            var id = Guid.NewGuid().ToString();
            if (costOptions != null && costOptions.Count == 1)
            {
                id = costOptions.First().Id;
            }
            list.Add(new OpenReferralCostOptionDto(id, payUnit ?? string.Empty, cost.Value, null, null, null, null));
        }

        return list;
    }

    private static List<OpenReferralServiceDeliveryExDto> GetDeliveryTypes(List<string>? serviceDeliverySelection, ICollection<OpenReferralServiceDeliveryExDto>? currentServiceDeliveries)
    {
        List<OpenReferralServiceDeliveryExDto> list = new();
        if (serviceDeliverySelection == null)
            return list;

        foreach (var serviceDelivery in serviceDeliverySelection)
        {
            switch (serviceDelivery)
            {
                case "1":
                    list.Add(GetDeliveryType(ServiceDelivery.InPerson, currentServiceDeliveries));
                    break;
                case "2":
                    list.Add(GetDeliveryType(ServiceDelivery.Online, currentServiceDeliveries));
                    break;
                case "3":
                    list.Add(GetDeliveryType(ServiceDelivery.Telephone, currentServiceDeliveries));
                    break;
            }
        }

        return list;
    }

    private static OpenReferralServiceDeliveryExDto GetDeliveryType(ServiceDelivery serviceDelivery, ICollection<OpenReferralServiceDeliveryExDto>? currentServiceDeliveries)
    {
        if (currentServiceDeliveries != null)
        {
            var item = currentServiceDeliveries.FirstOrDefault(x => x.ServiceDelivery == serviceDelivery);
            if (item != null)
            {
                return item;
            }
        }
        return new OpenReferralServiceDeliveryExDto(Guid.NewGuid().ToString(), serviceDelivery);
    }

    private static List<OpenReferralEligibilityDto> GetEligibilities(List<string> whoFor, int minAge, int maxAge, ICollection<OpenReferralEligibilityDto>? currentEligibilities)
    {
        //TODO - uncommemyt and do properly when existing service already has eligibilities
        //string id = Guid.NewGuid().ToString();
        //if (currentEligibilities != null)
        //{
        //    var eligibility = currentEligibilities.FirstOrDefault(x => x.Eligibility == whoFor);
        //    if (eligibility != null)
        //    {
        //        id = eligibility.Id;
        //    }
        //}

        List<OpenReferralEligibilityDto> list = new();

        if (whoFor != null && whoFor.Any())
        {
            foreach (var item in whoFor)
            {
                list.Add(
                    new OpenReferralEligibilityDto
                    {
                        Id = Guid.NewGuid().ToString(),
                        Eligibility = item,
                        Maximum_age = maxAge,
                        Minimum_age = minAge
                    });
            }
        }

        return list;
    }

    private async Task<List<OpenReferralServiceTaxonomyDto>> GetOpenReferralTaxonomies(List<string>? taxonomySelection, ICollection<OpenReferralServiceTaxonomyDto>? currentServiceTaxonomies)
    {
        List<OpenReferralServiceTaxonomyDto> openReferralTaxonomyRecords = new();

        PaginatedList<OpenReferralTaxonomyDto> taxonomies = await _openReferralOrganisationAdminClientService.GetTaxonomyList(1, 9999);

        if (taxonomies != null && taxonomySelection != null)
        {
            foreach (string taxonomyKey in taxonomySelection)
            {
                OpenReferralTaxonomyDto? taxonomy = taxonomies.Items.FirstOrDefault(x => x.Id == taxonomyKey);
                if (taxonomy != null)
                {
                    if (currentServiceTaxonomies != null)
                    {
                        var item = currentServiceTaxonomies.FirstOrDefault(x => x.Id == taxonomyKey);
                        if (item != null)
                        {
                            openReferralTaxonomyRecords.Add(item);
                            continue;
                        }
                    }
                    openReferralTaxonomyRecords.Add(new OpenReferralServiceTaxonomyDto(Guid.NewGuid().ToString(), taxonomy));
                }
            }
        }

        return openReferralTaxonomyRecords;
    }

    private static List<OpenReferralLanguageDto> GetLanguages(List<string>? viewModellanguages)
    {
        List<OpenReferralLanguageDto> languages = new();

        if (viewModellanguages != null)
        {
            foreach (string lang in viewModellanguages)
            {
                languages.Add(new OpenReferralLanguageDto(Guid.NewGuid().ToString(), lang));
            }
        }

        return languages;
    }
}
