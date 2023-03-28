using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;

public interface IViewModelToApiModelHelper
{
    Task<ServiceDto> GenerateUpdateServiceDto(OrganisationViewModel viewModel);
}

public class ViewModelToApiModelHelper : IViewModelToApiModelHelper
{
    private readonly IOrganisationAdminClientService _organisationAdminClientService;
    public ViewModelToApiModelHelper(IOrganisationAdminClientService organisationAdminClientService)
    {
        _organisationAdminClientService = organisationAdminClientService;
    }

    public async Task<ServiceDto> GenerateUpdateServiceDto(OrganisationViewModel viewModel)
    {
        var service = await _organisationAdminClientService.GetService(viewModel.ServiceId);

        if (service is null)
        {
            service = new ServiceDto
            {
                OrganisationId = viewModel.Id,
                Name = viewModel.ServiceName!,
                ServiceOwnerReferenceId = viewModel.ServiceOwnerReferenceId ?? Guid.NewGuid().ToString(),
                ServiceType = ServiceType.InformationSharing,
                DeliverableType = DeliverableType.NotSet,
                Status = ServiceStatusType.NotSet,
                ServiceAreas = GetListServiceAreas(),
                Locations = GetLocations(viewModel),
                Taxonomies = await GetTaxonomies(viewModel),
                Contacts = new List<ContactDto>(),
                Eligibilities= new List<EligibilityDto>()
            };
        }

        UpdateValues(service, viewModel);

        return service;

    }

    private static void UpdateValues(ServiceDto serviceDto, OrganisationViewModel viewModel)
    {
        serviceDto.Description = viewModel.ServiceDescription;
        serviceDto.CanFamilyChooseDeliveryLocation = string.Compare(viewModel.Familychoice, "Yes", StringComparison.OrdinalIgnoreCase) == 0;
        SetDeliveryTypes(serviceDto, viewModel);
        SetEligibility(serviceDto, viewModel);
        SetCost(serviceDto, viewModel);
        SetLanguages(serviceDto, viewModel);
        SetContactDetailsToService(serviceDto, viewModel);
    }

    private static void SetCost(ServiceDto serviceDto, OrganisationViewModel viewModel)
    {
        if(viewModel.IsPayedFor != "Yes")
        {
            serviceDto.CostOptions = new List<CostOptionDto>();
            return;
        }

        var costOption = serviceDto.CostOptions.FirstOrDefault();
        if (costOption is null)
        {
            costOption = new CostOptionDto();
        }

        costOption.Option = viewModel.PayUnit ?? string.Empty;
        costOption.Amount = viewModel.Cost;

        serviceDto.CostOptions = new List<CostOptionDto> { costOption };

    }

    private static void SetDeliveryTypes(ServiceDto serviceDto, OrganisationViewModel viewModel)
    {
        var serviceDeliveries = new List<ServiceDeliveryDto>();

        foreach (var serviceDeliveryName in viewModel.ServiceDeliverySelection!)
        {
            var existingServiceDelivery = serviceDto.ServiceDeliveries.Where(x => x.Name.ToString() == serviceDeliveryName).FirstOrDefault();

            if (existingServiceDelivery is not null)
            {
                serviceDeliveries.Add(existingServiceDelivery);
            }
            else
            {
                var serviceDelivery = new ServiceDeliveryDto { Name = ParseToEnum<ServiceDeliveryType>(serviceDeliveryName)};
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
            var existingItem = serviceDto.Eligibilities.Where(x => x.EligibilityType.ToString() == eligibilityName).FirstOrDefault();

            if (existingItem is null)
            {
                existingItem = new EligibilityDto { MaximumAge = 0, MinimumAge = 0 };
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
            var existingItem = serviceDto.Languages.Where(x => x.Name == languageName).FirstOrDefault();

            if (existingItem is null)
            {
                existingItem = new LanguageDto { Name = languageName };
                if (viewModel.ServiceId.HasValue)
                    existingItem.ServiceId = viewModel.ServiceId.Value;
            }

            languages.Add(existingItem);
        }

        serviceDto.Languages = languages;

    }

    //  At present the UI can only handle one contact per service even though the database structure allows multiple 
    //  contacts. This will put the contact object in the relevant place base on delivery method (Inperson, Phone etc)
    private static void SetContactDetailsToService(ServiceDto service, OrganisationViewModel viewModel)
    {
        if (service.ServiceDeliveries == null || !service.ServiceDeliveries.Any())
            return;

        List<ContactDto>? contacts = new List<ContactDto>();

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

    private static List<ContactDto> GetUpdatedContactList(List<ContactDto> existingContacts, OrganisationViewModel viewModel)
    {
        var telephone = viewModel.Telephone ?? string.Empty;
        var textphone = viewModel.Textphone ?? string.Empty;
        var website = viewModel.Website ?? string.Empty;
        var email = viewModel.Email ?? string.Empty;

        var existingContact = existingContacts?.Where(x =>
            (x.Telephone == telephone || string.IsNullOrEmpty(telephone)) &&
            (x.TextPhone == textphone || string.IsNullOrEmpty(textphone)) &&
            (x.Url == website || string.IsNullOrEmpty(website)) &&
            (x.Email == email || string.IsNullOrEmpty(email))).FirstOrDefault();

        if (existingContact is null)
        {
            existingContact = new ContactDto
            {
                Title = "Service",
                Name = "Telephone",
                Telephone = telephone,
                TextPhone = textphone,
                Url = website,
                Email = email
            };
        }

        return new List<ContactDto> { existingContact };
    }

    private static List<ServiceAreaDto> GetListServiceAreas()
    {
        var serviceArea = new ServiceAreaDto
        {
            ServiceAreaName = "Local", 
            Extent = null, 
            Uri = "http://statistics.data.gov.uk/id/statistical-geography/K02000001"
        };

        return new List<ServiceAreaDto> { serviceArea };
    }

    private static List<LocationDto> GetLocations(OrganisationViewModel viewModel)
    {
        var location = new LocationDto 
        { 
            Name = "Our Location",
            Latitude = viewModel.Latitude ?? 0.0D,
            Longitude = viewModel.Longtitude ?? 0.0D,
            Address1 = viewModel.Address_1 ?? string.Empty,
            City = viewModel.City ?? string.Empty,
            PostCode = viewModel.Postal_code ?? string.Empty,
            Country = "England",
            StateProvince = viewModel.State_province ?? string.Empty,
            LocationType = LocationType.NotSet,
            Contacts= new List<ContactDto>()
        };

        return new List<LocationDto> { location };
    }

    private async Task<List<TaxonomyDto>> GetTaxonomies(OrganisationViewModel viewModel)
    {
        List<TaxonomyDto> taxonomyRecords = new();

        var taxonomies = await _organisationAdminClientService.GetTaxonomyList(1, 9999);

        foreach (var taxonomyKey in viewModel.TaxonomySelection!)
        {
            var taxonomy = taxonomies.Items.FirstOrDefault(x => x.Id == taxonomyKey);
            if (taxonomy != null)
            {
                taxonomyRecords.Add(taxonomy);
            }
        }

        return taxonomyRecords;
    }

    private static T ParseToEnum<T>(string value)
    {
        return (T)Enum.Parse(typeof(T), value, true);
    }

}
