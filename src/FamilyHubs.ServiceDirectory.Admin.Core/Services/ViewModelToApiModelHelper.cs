using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;

namespace FamilyHubs.ServiceDirectory.Admin.Core.Services;

public interface IViewModelToApiModelHelper
{
    Task<ServiceDto> GenerateUpdateServiceDto(OrganisationViewModel viewModel);
    ServiceDto MapViewModelToDto(OrganisationViewModel viewModel, ServiceDto? serviceDto = null);
}

public class ViewModelToApiModelHelper : IViewModelToApiModelHelper
{
    private readonly IServiceDirectoryClient _serviceDirectoryClient;
    public ViewModelToApiModelHelper(IServiceDirectoryClient serviceDirectoryClient)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
    }

    public async Task<ServiceDto> GenerateUpdateServiceDto(OrganisationViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel.ServiceId);

        var service = await _serviceDirectoryClient.GetServiceById(viewModel.ServiceId.Value);

        return MapViewModelToDto(viewModel, service);
    }

    public ServiceDto MapViewModelToDto(OrganisationViewModel viewModel, ServiceDto? serviceDto = null)
    {
        serviceDto ??= new ServiceDto
        {
            ServiceOwnerReferenceId = "",
            ServiceType = viewModel.Type == "LA" ? ServiceType.FamilyExperience : ServiceType.InformationSharing,
            Status = ServiceStatusType.Active,
            Name = ""
        };

        serviceDto.OrganisationId = viewModel.Id;
        serviceDto.Description = viewModel.ServiceDescription;
        serviceDto.ServiceOwnerReferenceId = viewModel.ServiceOwnerReferenceId ?? Guid.NewGuid().ToString();
        serviceDto.Name = viewModel.ServiceName ?? string.Empty;
        serviceDto.Description = viewModel.ServiceDescription ?? Guid.NewGuid().ToString();
        serviceDto.CanFamilyChooseDeliveryLocation = string.Compare(viewModel.FamilyChoice, "Yes", StringComparison.OrdinalIgnoreCase) == 0;
        
        SetLocation(serviceDto, viewModel);
        SetDeliveryTypes(serviceDto, viewModel);
        SetEligibility(serviceDto, viewModel);
        SetCost(serviceDto, viewModel);
        SetLanguages(serviceDto, viewModel);
        SetContactDetailsToService(serviceDto, viewModel);

        return serviceDto;
    }

    private static void SetLocation(ServiceDto serviceDto, OrganisationViewModel viewModel)
    {
        var location = serviceDto.Locations.FirstOrDefault();
        if (location is null)
        {
            location = new LocationDto
            {
                LocationType = LocationType.NotSet,
                Name = "",
                Latitude = 0,
                Longitude = 0,
                Address1 = "",
                City = "",
                PostCode = "",
                StateProvince = "",
                Country = ""
            };

            serviceDto.Locations.Add(location);
        }

        location.Name = viewModel.LocationName ?? string.Empty;
        location.Description = viewModel.LocationDescription;
        location.Latitude = viewModel.Latitude ?? 0;
        location.Longitude = viewModel.Longitude ?? 0;

        location.Address1 = viewModel.Address1 ?? string.Empty;
        location.City = viewModel.City ?? string.Empty;
        location.Country = viewModel.Country ?? string.Empty;
        location.PostCode = viewModel.PostalCode ?? string.Empty;
        location.StateProvince = viewModel.StateProvince ?? string.Empty;

        if (viewModel.RegularSchedules is not null)
        {
            foreach (var schedule in viewModel.RegularSchedules.Where(schedule => !string.IsNullOrEmpty(schedule)))
            {
                location.RegularSchedules.Add(new RegularScheduleDto
                {
                    Description = schedule
                });
            }
        }
    }

    private static void SetCost(ServiceDto serviceDto, OrganisationViewModel viewModel)
    {
        if (viewModel.IsPayedFor != "Yes")
        {
            serviceDto.CostOptions = new List<CostOptionDto>();
            return;
        }

        var costOption = serviceDto.CostOptions.FirstOrDefault() ?? new CostOptionDto();

        costOption.Option = viewModel.PayUnit ?? string.Empty;
        costOption.Amount = viewModel.Cost;

        serviceDto.CostOptions = new List<CostOptionDto> { costOption };
    }

    private static void SetDeliveryTypes(ServiceDto serviceDto, OrganisationViewModel viewModel)
    {
        var serviceDeliveries = new List<ServiceDeliveryDto>();

        foreach (var serviceDeliveryName in viewModel.ServiceDeliverySelection!)
        {
            var existingServiceDelivery = serviceDto.ServiceDeliveries.FirstOrDefault(x => x.Name.ToString() == serviceDeliveryName);

            if (existingServiceDelivery is not null)
            {
                serviceDeliveries.Add(existingServiceDelivery);
            }
            else
            {
                var serviceDelivery = new ServiceDeliveryDto
                {
                    Name = ParseToEnum<ServiceDeliveryType>(serviceDeliveryName)
                };
                if (viewModel.ServiceId.HasValue)
                    serviceDelivery.ServiceId = viewModel.ServiceId.Value;
                serviceDeliveries.Add(serviceDelivery);
            }
        }

        serviceDto.ServiceDeliveries = serviceDeliveries;
    }

    private static void SetEligibility(ServiceDto serviceDto, OrganisationViewModel viewModel)
    {
        var eligibilities = new List<EligibilityDto>();

        if (viewModel.WhoForSelection is null)
        {
            serviceDto.Eligibilities = eligibilities;
            return;
        }

        foreach (var eligibilityName in viewModel.WhoForSelection)
        {
            var existingItem = serviceDto.Eligibilities.FirstOrDefault(x => x.EligibilityType.ToString() == eligibilityName);

            if (existingItem is null)
            {
                existingItem = new EligibilityDto
                {
                    MaximumAge = 0,
                    MinimumAge = 0
                };
                if (viewModel.ServiceId.HasValue)
                    existingItem.ServiceId = viewModel.ServiceId.Value;
            }

            if (viewModel.MaxAge.HasValue)
                existingItem.MaximumAge = viewModel.MaxAge.Value;

            if (viewModel.MinAge.HasValue)
                existingItem.MinimumAge = viewModel.MinAge.Value;

            eligibilities.Add(existingItem);
        }

        serviceDto.Eligibilities = eligibilities;
    }

    private static void SetLanguages(ServiceDto serviceDto, OrganisationViewModel viewModel)
    {
        var languages = new List<LanguageDto>();

        foreach (var languageName in viewModel.Languages!)
        {
            var existingItem = serviceDto.Languages.FirstOrDefault(x => x.Name == languageName);

            if (existingItem is null)
            {
                existingItem = new LanguageDto
                {
                    Name = languageName
                };
                if (viewModel.ServiceId.HasValue)
                    existingItem.ServiceId = viewModel.ServiceId.Value;
            }

            languages.Add(existingItem);
        }

        serviceDto.Languages = languages;
    }

    //  At present the UI can only handle one contact per service even though the database structure allows multiple 
    //  contacts. This will put the contact object in the relevant place base on delivery method (In Person, Phone etc)
    private static void SetContactDetailsToService(ServiceDto service, OrganisationViewModel viewModel)
    {
        if (!service.ServiceDeliveries.Any())
            return;

        switch (service.ServiceDeliveries.First().Name)
        {
            case ServiceDeliveryType.Telephone:
            case ServiceDeliveryType.Online:
                service.Contacts = GetUpdatedContactList(service.Contacts.ToList(), viewModel);
                break;

            case ServiceDeliveryType.InPerson:
                if (service.Locations is null || !service.Locations.Any())
                {
                    throw new ArgumentException("Service at location required for delivery type in person");
                }

                service.Locations.First().Contacts = GetUpdatedContactList(service.Locations.First().Contacts.ToList(), viewModel);
                break;

            case ServiceDeliveryType.NotSet:
                return;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static List<ContactDto> GetUpdatedContactList(IEnumerable<ContactDto> existingContacts, OrganisationViewModel viewModel)
    {
        var telephone = viewModel.Telephone ?? string.Empty;
        var textPhone = viewModel.TextPhone ?? string.Empty;
        var website = viewModel.Website ?? string.Empty;
        var email = viewModel.Email ?? string.Empty;

        var existingContact = existingContacts.FirstOrDefault(x =>
            (x.Telephone == telephone || string.IsNullOrEmpty(telephone)) &&
            (x.TextPhone == textPhone || string.IsNullOrEmpty(textPhone)) &&
            (x.Url == website || string.IsNullOrEmpty(website)) &&
            (x.Email == email || string.IsNullOrEmpty(email)));

        if (existingContact is null)
        {
            existingContact = new ContactDto
            {
                Title = "Service",
                Name = "Telephone",
                Telephone = telephone,
                TextPhone = textPhone,
                Url = website,
                Email = email
            };
        }

        return new List<ContactDto> { existingContact };
    }

    private static T ParseToEnum<T>(string value)
    {
        return (T)Enum.Parse(typeof(T), value, true);
    }
}
