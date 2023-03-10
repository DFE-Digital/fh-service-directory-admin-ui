using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using NPOI.SS.UserModel;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using System.Net.NetworkInformation;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;

public interface IViewModelToApiModelHelper
{
    Task<OrganisationWithServicesDto> GenerateUpdateServiceDto(OrganisationViewModel viewModel);
}

public class ViewModelToApiModelHelper : IViewModelToApiModelHelper
{
    private readonly IOrganisationAdminClientService _organisationAdminClientService;
    public ViewModelToApiModelHelper(IOrganisationAdminClientService organisationAdminClientService)
    {
        _organisationAdminClientService = organisationAdminClientService;
    }

    private async Task<ServiceDto> GenerateUpdateServiceDto(OrganisationViewModel viewModel)
    {

        ServiceDto existingService;
        
        if(viewModel.ServiceId is not null)
        {
            existingService = await _organisationAdminClientService.GetService(viewModel.ServiceId);
        }

        if(existingService is null)
        {
            existingService = new ServiceDto();
        }

        var service = new ServiceDto {
            Id = viewModel.ServiceId,
            ServiceType = GetServiceType(viewModel.ServiceType),
            OrganisationId = viewModel.Id,
            Name = viewModel.ServiceName ?? string.Empty,
            Description = viewModel.ServiceDescription,
            Accreditations = null,
            AssuredDate = null,
            AttendingAccess = null,
            AttendingType = null,
            DeliverableType = string.Join(",", viewModel.InPersonSelection != null ? viewModel.InPersonSelection.ToArray() : Array.Empty<string>()),
            Status = ServiceStatusType.Active,
            Fees = null,
            CanFamilyChooseDeliveryLocation = string.Compare(viewModel.Familychoice, "Yes", StringComparison.OrdinalIgnoreCase) == 0,
            ServiceDeliveries = GetDeliveryTypes(viewModel.ServiceDeliverySelection, existingService?.ServiceDeliveries),
            Eligibilities = GetEligibility(viewModel.WhoForSelection ?? new List<string>(), viewModel.MinAge ?? 0, viewModel.MaxAge ?? 0),
            Fundings = null,//fundingdto
            CostOptions = GetCost(viewModel.IsPayedFor == "Yes", viewModel.PayUnit ?? string.Empty, viewModel.Cost, currentService?.CostOptions),
            Languages = GetLanguages(viewModel.Languages),
            ServiceAreas = new List<ServiceAreaDto>
            {
                new ServiceAreaDto(Guid.NewGuid().ToString(), "Local", null, "http://statistics.data.gov.uk/id/statistical-geography/K02000001")
            },
            Locations = GetLocations(viewModel, currentService?.ServiceAtLocations),
            Taxonomies = await GetTaxonomies(viewModel.TaxonomySelection, currentService?.ServiceTaxonomies),
            RegularSchedules = new List<RegularScheduleDto>(),
            HolidaySchedules = new List<HolidayScheduleDto>(),
            Contacts = new List<ContactDto>()
        };

        AddContactDetailsToService(service, viewModel, currentService);

        return service;
    }

    private static List<LocationDto> GetLocations(OrganisationViewModel viewModel, ICollection<LocationDto>? currentServiceAtLocations)
    {
        long id = -1;
        if (currentServiceAtLocations != null && currentServiceAtLocations.Any())
        {
            var currentServiceAtLocation = currentServiceAtLocations.FirstOrDefault(x => x.Name == "Our Location");
            if (currentServiceAtLocation != null)
            {
                id = currentServiceAtLocation.Id;
            }
        }

        return new List<LocationDto>
        {
            new LocationDto
            {
                Id = id,
                Name = "Our Location",
                Latitude = viewModel.Latitude ?? 0.0D,
                Longitude = viewModel.Longtitude ?? 0.0D,
                Address1 = viewModel.Address_1 ?? string.Empty,
                City = viewModel.City ?? string.Empty,
                PostCode = viewModel.Postal_code ?? string.Empty,
                Country = "England",
                StateProvince = viewModel.State_province ?? string.Empty,
                Contacts = new List<ContactDto>(),
                RegularSchedules = new List<RegularScheduleDto>(),
                HolidaySchedules = new List<HolidayScheduleDto>()
            }
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

    private async Task<List<TaxonomyDto>> GetTaxonomies(List<long>? taxonomySelection, ICollection<TaxonomyDto>? currentServiceTaxonomies)
    {
        List<TaxonomyDto> taxonomyRecords = new();

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
                    taxonomyRecords.Add(taxonomy);
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

    //  At present the UI can only handle one contact per service even though the database structure allows multiple 
    //  contacts. This will put the contact object in the relevant place base on delivery method (Inperson, Phone etc)
    private static void AddContactDetailsToService(
        ServiceDto service, OrganisationViewModel viewModel, ServiceDto? currentService)
    {
        if (service.ServiceDeliveries == null || !service.ServiceDeliveries.Any())
            return;

        ICollection<ContactDto>? existingContacts;
        ICollection<ContactDto> newContactsList;

        switch (service.ServiceDeliveries.First().Name)
        {
            case ServiceDeliveryType.Telephone:
            case ServiceDeliveryType.Online:
                newContactsList = service.Contacts!;
                existingContacts = currentService?.Contacts;
                newContactsList.Add(AddLinkContact(existingContacts, viewModel));
                break;

            case ServiceDeliveryType.InPerson:
                existingContacts = currentService?.Locations?.First().Contacts;

                if(service.Locations is null || !service.Locations.Any())
                {
                    throw new ArgumentException("Service at location required for delivery type in person");
                }

                var serviceAtLocation = service.Locations.First()!;
                newContactsList = serviceAtLocation.Contacts!;
                newContactsList.Add(AddLinkContact(existingContacts, viewModel));                   
                break;

            case ServiceDeliveryType.NotSet:
                return;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static ContactDto AddLinkContact(ICollection<ContactDto>? existingContacts, OrganisationViewModel viewModel)
    {  
        var telephone = viewModel.Telephone ?? string.Empty;
        var textphone = viewModel.Textphone ?? string.Empty;
        var website = viewModel.Website ?? string.Empty;
        var email = viewModel.Email ?? string.Empty;

        //  Determine if Contact already exists
        var existingLinkContact = existingContacts?.Where(x => 
            x.Telephone == telephone && 
            x.TextPhone == textphone &&
            x.Url == website &&
            x.Email == email).First();

        if(existingLinkContact is not null)
        {
            return existingLinkContact;
        }

        var contact = new ContactDto
        {
            Title = "Service",
            Name = "Telephone",
            Telephone = telephone,
            TextPhone = textphone,
            Url = website,
            Email = email
        };

        return contact;
    }


    private static ServiceType GetServiceType(string? serviceTypeString)
    {
        throw new NotImplementedException();
    }

}
