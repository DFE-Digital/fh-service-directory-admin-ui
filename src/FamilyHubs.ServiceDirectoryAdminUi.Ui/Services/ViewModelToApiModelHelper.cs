﻿using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using FamilyHubs.SharedKernel;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;

public interface IViewModelToApiModelHelper
{
    Task<OrganisationWithServicesDto> GetOrganisation(OrganisationViewModel viewModel);
}

public class ViewModelToApiModelHelper : IViewModelToApiModelHelper
{
    private readonly IOrganisationAdminClientService _organisationAdminClientService;
    public ViewModelToApiModelHelper(IOrganisationAdminClientService organisationAdminClientService)
    {
        _organisationAdminClientService = organisationAdminClientService;
    }

    public async Task<OrganisationWithServicesDto> GetOrganisation(OrganisationViewModel viewModel)
    {
        var updateOrganisation = await _organisationAdminClientService.GetOrganisationById(viewModel.Id.ToString());

        var currentService = updateOrganisation.Services?.FirstOrDefault(x => x.Id == viewModel.ServiceId);

        var contactIdTelephone = Guid.NewGuid().ToString();

        var organisationTypeDto = new OrganisationTypeDto(updateOrganisation.OrganisationType.Id, updateOrganisation.OrganisationType.Name, updateOrganisation.OrganisationType.Description);

        var organisation = new OrganisationWithServicesDto(
            viewModel.Id.ToString(),
            organisationTypeDto,
            viewModel.Name,
            viewModel.Description,
            viewModel.Logo,
            new Uri(viewModel.Url ?? string.Empty).ToString(),
            viewModel.Url,
            new List<ServiceDto>
            {
            new ServiceDto(
                viewModel.ServiceId ?? Guid.NewGuid().ToString(),
                new ServiceTypeDto("1", "Information Sharing", ""),
                viewModel.Id.ToString(),
                viewModel.ServiceName ?? string.Empty,
                viewModel.ServiceDescription,
                null,
                null,
                null,
                null,
                string.Join(",", viewModel.InPersonSelection != null ? viewModel.InPersonSelection.ToArray() : Array.Empty<string>()),
                "pending",
                null,
                string.Compare(viewModel.Familychoice,"Yes", StringComparison.OrdinalIgnoreCase) == 0,
                GetDeliveryTypes(viewModel.ServiceDeliverySelection, currentService?.ServiceDeliveries),
                GetEligibility(viewModel.WhoForSelection ?? new List<string>(), viewModel.MinAge ?? 0, viewModel.MaxAge ?? 0),
                new List<ContactDto>
                {
                    new ContactDto(
                        contactIdTelephone,
                        "Service",
                        "Telephone",
                        viewModel.Telephone ?? string.Empty,
                        viewModel.Textphone ?? string.Empty,
                        viewModel.Website,
                        viewModel.Email
                    ),
                },
                GetCost(viewModel.IsPayedFor == "Yes", viewModel.PayUnit ?? string.Empty, viewModel.Cost, currentService?.CostOptions),
                GetLanguages(viewModel.Languages)
                , new List<ServiceAreaDto>
                {
                    new ServiceAreaDto(Guid.NewGuid().ToString(), "Local", null, "http://statistics.data.gov.uk/id/statistical-geography/K02000001")

                }
                ,
                GetServiceAtLocation(viewModel, currentService?.ServiceAtLocations),
                await GetTaxonomies(viewModel.TaxonomySelection, currentService?.ServiceTaxonomies),
                new List<RegularScheduleDto>(),
                new List<HolidayScheduleDto>()
                )
            });

        return organisation;


    }

    private static List<ServiceAtLocationDto> GetServiceAtLocation(OrganisationViewModel viewModel, ICollection<ServiceAtLocationDto>? currentServiceAtLocations)
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
                if (currentServiceAtLocation.Location.PhysicalAddresses is { Count: 1 })
                {
                    physicalAddressId = currentServiceAtLocation.Location.PhysicalAddresses.First().Id;
                }
            }
        }

        return new List<ServiceAtLocationDto>
        {
            new ServiceAtLocationDto(
                id,
                new LocationDto(
                    locationId,
                    "Our Location",
                    "",
                    viewModel.Latitude ?? 0.0D,
                    viewModel.Longtitude ?? 0.0D,
                    new List<PhysicalAddressDto>
                    {
                        new PhysicalAddressDto(
                            physicalAddressId,
                            viewModel.Address_1 ?? string.Empty,
                            viewModel.City ?? string.Empty,
                            viewModel.Postal_code ?? string.Empty,
                            "England",
                            viewModel.State_province ?? string.Empty
                            )
                    },null
                ),
                new List<RegularScheduleDto>(),
                new List<HolidayScheduleDto>()
                )
        };
    }

    private static List<CostOptionDto> GetCost(bool isPayedFor, string payUnit, decimal? cost, ICollection<CostOptionDto>? costOptions)
    {
        List<CostOptionDto> list = new();

        if (isPayedFor && cost != null)
        {
            var id = Guid.NewGuid().ToString();
            if (costOptions is { Count: 1 })
            {
                id = costOptions.First().Id;
            }
            list.Add(new CostOptionDto(id, payUnit, cost.Value, null, null, null, null));
        }

        return list;
    }

    private static List<ServiceDeliveryDto> GetDeliveryTypes(List<string>? serviceDeliverySelection, ICollection<ServiceDeliveryDto>? currentServiceDeliveries)
    {
        List<ServiceDeliveryDto> list = new();
        if (serviceDeliverySelection == null)
            return list;

        foreach (var serviceDelivery in serviceDeliverySelection)
        {
            switch (serviceDelivery)
            {
                case "1":
                    list.Add(GetDeliveryType(ServiceDeliveryType.InPerson, currentServiceDeliveries));
                    break;
                case "2":
                    list.Add(GetDeliveryType(ServiceDeliveryType.Online, currentServiceDeliveries));
                    break;
                case "3":
                    list.Add(GetDeliveryType(ServiceDeliveryType.Telephone, currentServiceDeliveries));
                    break;
            }
        }

        return list;
    }

    private static ServiceDeliveryDto GetDeliveryType(ServiceDeliveryType serviceDelivery, ICollection<ServiceDeliveryDto>? currentServiceDeliveries)
    {
        if (currentServiceDeliveries != null)
        {
            var item = currentServiceDeliveries.FirstOrDefault(x => x.Name == serviceDelivery);
            if (item != null)
            {
                return item;
            }
        }
        return new ServiceDeliveryDto(Guid.NewGuid().ToString(), serviceDelivery);
    }

    private static List<EligibilityDto> GetEligibility(List<string> whoFor, int minAge, int maxAge)
    {
        List<EligibilityDto> list = new();

        if (whoFor.Any())
        {
            foreach (var item in whoFor)
            {
                list.Add(
                    new EligibilityDto
                    {
                        Id = Guid.NewGuid().ToString(),
                        EligibilityDescription = item,
                        MaximumAge = maxAge,
                        MinimumAge = minAge
                    });
            }
        }

        return list;
    }

    private async Task<List<ServiceTaxonomyDto>> GetTaxonomies(List<string>? taxonomySelection, ICollection<ServiceTaxonomyDto>? currentServiceTaxonomies)
    {
        List<ServiceTaxonomyDto> taxonomyRecords = new();

        var taxonomies = await _organisationAdminClientService.GetTaxonomyList(1, 9999);

        if (taxonomySelection != null)
        {
            foreach (var taxonomyKey in taxonomySelection)
            {
                var taxonomy = taxonomies.Items.FirstOrDefault(x => x.Id == taxonomyKey);
                if (taxonomy != null)
                {
                    if (currentServiceTaxonomies != null)
                    {
                        var item = currentServiceTaxonomies.FirstOrDefault(x => x.Id == taxonomyKey);
                        if (item != null)
                        {
                            taxonomyRecords.Add(item);
                            continue;
                        }
                    }
                    taxonomyRecords.Add(new ServiceTaxonomyDto(Guid.NewGuid().ToString(), taxonomy));
                }
            }
        }

        return taxonomyRecords;
    }

    private static List<LanguageDto> GetLanguages(List<string>? viewModelLanguages)
    {
        List<LanguageDto> languages = new();

        if (viewModelLanguages != null)
        {
            foreach (var lang in viewModelLanguages)
            {
                languages.Add(new LanguageDto(Guid.NewGuid().ToString(), lang));
            }
        }

        return languages;
    }
}
