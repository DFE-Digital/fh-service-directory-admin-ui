using FamilyHubs.ServiceDirectory.Shared.Builders;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using System.Data;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.DataUpload;


public interface IDataUploadService
{
    Task<List<string>> UploadToApi(string organisationId, BufferedSingleFileUploadDb fileUpload, bool useSpreadsheetServiceId = false);
}

public class DataUploadService : IDataUploadService
{
    private readonly IOrganisationAdminClientService _OrganisationAdminClientService;
    private readonly IPostcodeLocationClientService _postcodeLocationClientService;

    private bool _useSpreadsheetServiceId = true;
    private List<OrganisationDto> _organisations = new();
    private readonly List<OrganisationWithServicesDto> _organisationsWithServices = new();
    private readonly List<TaxonomyDto> _taxonomies = new();
    private readonly List<string> _errors = new List<string>();
    private readonly Dictionary<string, PostcodesIoResponse> _postCodesCache = new Dictionary<string, PostcodesIoResponse>();

    public DataUploadService(IOrganisationAdminClientService OrganisationAdminClientService, IPostcodeLocationClientService postcodeLocationClientService)
    {
        _OrganisationAdminClientService = OrganisationAdminClientService;
        _postcodeLocationClientService = postcodeLocationClientService;
    }

    public async Task<List<string>> UploadToApi(string organisationId, BufferedSingleFileUploadDb fileUpload, bool useSpreadsheetServiceId = false)
    {
        _useSpreadsheetServiceId = useSpreadsheetServiceId;
        var taxonomies = await _OrganisationAdminClientService.GetTaxonomyList(1, 999999999);
        _taxonomies.AddRange(taxonomies.Items);
        var dtExcelTable = await ExcelReader.GetRequestsDataFromExcel(fileUpload);
        await ProcessRows(dtExcelTable);
        return _errors;
    }

    private async Task ProcessRows(DataTable dtExcelTable)
    {
        var rowNumber = 5;

        foreach (DataRow dtRow in dtExcelTable.Rows)
        {
            rowNumber++;

            var localAuthority = await GetOrganisationsWithOutServices(dtRow["Local authority"].ToString() ?? string.Empty);
            if (localAuthority == null)
            {
                _errors.Add($"Failed to find local authority row:{rowNumber}");
                continue;
            }

            OrganisationTypeDto organisationTypeDto;
            string? organisationName;
            switch (dtRow["Organisation Type"].ToString()?.ToLower())
            {
                case "local authority":
                    organisationTypeDto = new OrganisationTypeDto("1", "LA", "Local Authority");
                    organisationName = dtRow["Local authority"].ToString();
                    break;
                case "voluntary and community sector":
                    organisationTypeDto = new OrganisationTypeDto("2", "VCFS", "Voluntary, Charitable, Faith Sector");
                    organisationName = dtRow["Name of organisation"].ToString();
                    break;
                case "family hub":
                    organisationTypeDto = new OrganisationTypeDto("3", "FamilyHub", "Family Hub");
                    organisationName = dtRow["Local authority"].ToString();
                    break;
                default:
                    organisationTypeDto = new OrganisationTypeDto("4", "Company", "Public / Private Company eg: Child Care Centre");
                    organisationName = dtRow["Name of organisation"].ToString();
                    break;

            }

            if (organisationTypeDto.Name != "LA" && organisationTypeDto.Name != "FamilyHub")
            {
                if (string.IsNullOrWhiteSpace(organisationName))
                {
                    _errors.Add($"Name of organisation missing row:{rowNumber}");
                    continue;
                }
            }


            var newOrganisation = false;
            OrganisationWithServicesDto? OrganisationDto;
            if (organisationTypeDto.Name == "LA" || organisationTypeDto.Name == "FamilyHub")
            {
                OrganisationDto = await GetOrganisation(dtRow["Local authority"].ToString() ?? string.Empty);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(organisationName))
                {
                    _errors.Add($"Name of organisation missing row:{rowNumber}");
                    continue;
                }
                OrganisationDto = await GetOrganisation(organisationName);
                if (OrganisationDto == null)
                {
                    OrganisationDto = new OrganisationWithServicesDto
                    (
                        Guid.NewGuid().ToString(),
                        organisationTypeDto,
                        organisationName,
                        organisationName,
                        null,
                        dtRow["Website"].ToString(),
                        dtRow["Website"].ToString()
                    )
                    {
                        AdminAreaCode = localAuthority.AdminAreaCode
                    };

                    newOrganisation = true;
                }
            }

            if (newOrganisation)
            {
                var service = await GetServiceFromRow(rowNumber, dtRow, null, organisationTypeDto, OrganisationDto?.Id ?? string.Empty);
                if (OrganisationDto != null && service != null)
                {
                    OrganisationDto.Services = new List<ServiceDto>()
                    {
                        service
                    };

                    try
                    {
                        //Create Organisation
                        var _ = await _OrganisationAdminClientService.CreateOrganisation(OrganisationDto);
                    }
                    catch
                    {
                        _errors.Add($"Failed to create organisation with service row:{rowNumber}");
                    }

                }
            }
            else
            {
                var isNewService = true;
                ServiceDto? service;
                if (_useSpreadsheetServiceId)
                {
                    if ((string.IsNullOrEmpty(dtRow["Service unique identifier"].ToString())))
                    {
                        _errors.Add($"Service unique identifier missing row:{rowNumber}");
                        continue;
                    }

                    service = OrganisationDto?.Services?.FirstOrDefault(x => x.Id == $"{OrganisationDto.AdminAreaCode?.Remove(0, 1)}{dtRow["Service unique identifier"]}");

                }
                else
                {
                    service = OrganisationDto?.Services?.FirstOrDefault(x => x.Name == dtRow["Name of service"].ToString());
                }

                if (service != null)
                {
                    isNewService = false;
                }
                service = await GetServiceFromRow(rowNumber, dtRow, service, organisationTypeDto, OrganisationDto?.Id ?? string.Empty);

                if (isNewService)
                {
                    if (service != null)
                    {
                        try
                        {
                            var _ = await _OrganisationAdminClientService.CreateService(service);
                        }
                        catch
                        {
                            _errors.Add($"Failed to create service row:{rowNumber}");
                        }

                    }
                }
                else
                {
                    if (service != null)
                    {
                        try
                        {
                            var _ = await _OrganisationAdminClientService.UpdateService(service);
                        }
                        catch
                        {
                            _errors.Add($"Failed to update service row:{rowNumber}");
                        }

                    }
                }
            }
        }
    }

    private async Task<ServiceDto?> GetServiceFromRow(int rowNumber, DataRow dtRow, ServiceDto? service, OrganisationTypeDto organisationTypeDto, string organisationId)
    {
        var description = dtRow["More Details (service description)"].ToString();

        var locations = await GetLocationDto(rowNumber, dtRow, service);
        if (!locations.Any())
            return null;

        var serviceId = service?.Id ?? Guid.NewGuid().ToString();
        if (string.IsNullOrEmpty(dtRow["Service unique identifier"].ToString()))
        {
            _errors.Add($"Service unique identifier missing row:{rowNumber}");
            return null;
        }
        if (service == null && _useSpreadsheetServiceId && !string.IsNullOrEmpty(dtRow["Service unique identifier"].ToString()))
        {
            var organisation = await GetOrganisationsWithOutServices(dtRow["Local authority"].ToString() ?? string.Empty);
            serviceId = organisation is not null ?
            $"{organisation.AdminAreaCode?.Remove(0, 1)}{dtRow["Service unique identifier"]}" : Guid.NewGuid().ToString();
        }

        var builder = new ServicesDtoBuilder();
        var result = builder.WithMainProperties(serviceId,
                                   GetServiceType(organisationTypeDto),
                                   organisationId,
                                   dtRow["Name of service"].ToString() ?? string.Empty,
                                   description,
                                   null,
                                   null,
                                   null,
                                   dtRow["Delivery method"].ToString(),
                                   dtRow["Delivery method"].ToString(),
                                   "active",
                                   string.Empty,
                                   false)
                        .WithServiceDelivery(GetDeliveryTypes(dtRow["Delivery method"].ToString() ?? string.Empty, service))
                        .WithServiceAtLocations(locations)
                        .WithContact(GetContacts(dtRow, service))
                        .WithCostOption(GetCosts(dtRow, service))
                        .WithLanguages(GetLanguages(dtRow, service))
                        .WithServiceTaxonomies(GetTaxonomies(dtRow))
                        .WithEligibility(GetEligibilities(dtRow, service))
                        .Build();

        return result;

    }

    private List<ContactDto> GetContacts(DataRow dtRow, ServiceDto? service)
    {
        var contactId = Guid.NewGuid().ToString();
        var Contacts = service?.Contacts != null ? service.Contacts.ToList() : new List<ContactDto>();
        if (service != null && service.Contacts != null)
        {
            var contact = service.Contacts?.FirstOrDefault(x => x.Name == "Telephone");
            if (contact != null)
            {
                contactId = contact.Id;
            }
        }

        if (!string.IsNullOrEmpty(dtRow["Contact phone"].ToString()))
        {
            Contacts.Add(new ContactDto(
            contactId,
            "",
            "Telephone",
            dtRow["Contact phone"].ToString() ?? string.Empty,
            dtRow["Contact sms"].ToString() ?? string.Empty,
            dtRow["Website"].ToString(),
            dtRow["Contact email"].ToString()
            ));
        }

        return Contacts;
    }

    private List<EligibilityDto> GetEligibilities(DataRow dtRow, ServiceDto? service)
    {
        var eligibilityId = Guid.NewGuid().ToString();
        var list = (service != null && service.Eligibilities != null) ? service.Eligibilities.ToList() : new();

        if (!int.TryParse(dtRow["Age from"].ToString(), out var minimumAge))
        {
            minimumAge = 0;
        }

        if (!int.TryParse(dtRow["Age to"].ToString(), out var maximumAge))
        {
            maximumAge = 127;
        }

        var eligibility = "Child";
        if (minimumAge >= 18)
        {
            eligibility = "Adult";
        }

        if (service != null && service.Eligibilities != null)
        {
            var eligibleItem = service.Eligibilities?.Count == 1 ? service.Eligibilities?.First() : service.Eligibilities?.FirstOrDefault(x => x.MinimumAge == minimumAge && x.MaximumAge == maximumAge);
            if (eligibleItem != null)
            {
                eligibilityId = eligibleItem.Id;
            }
        }

        list.Add(new EligibilityDto(eligibilityId, eligibility, maximumAge, minimumAge));

        return list;
    }

    private List<ServiceTaxonomyDto> GetTaxonomies(DataRow dtRow)
    {
        List<ServiceTaxonomyDto> list = new();
        var categories = dtRow["Sub-category"].ToString();
        if (!string.IsNullOrEmpty(categories))
        {
            var parts = categories.Split('|');
            foreach (var part in parts)
            {
                var taxonomy = _taxonomies.FirstOrDefault(x => x.Name.ToLower() == part.Trim().ToLower());
                if (taxonomy != null)
                {
                    list.Add(new ServiceTaxonomyDto(Guid.NewGuid().ToString(), taxonomy));
                }

            }
        }
        return list;
    }

    private List<LanguageDto> GetLanguages(DataRow dtRow, ServiceDto? service)
    {
        var list = (service != null && service.Languages != null) ? service.Languages.ToList() : new List<LanguageDto>();
        var languages = dtRow["Language"].ToString();
        if (!string.IsNullOrEmpty(languages))
        {
            var parts = languages.Split('|');
            foreach (var part in parts)
            {
                var languageId = Guid.NewGuid().ToString();
                if (service != null && service.Languages != null)
                {
                    var originalLanguage = service.Languages.FirstOrDefault(x => x.Name == part);
                    if (originalLanguage != null)
                    {
                        languageId = originalLanguage.Id;
                    }
                }

                list.Add(new LanguageDto(languageId, part.Trim()));
            }
        }

        return list;
    }

    private List<CostOptionDto> GetCosts(DataRow dtRow, ServiceDto? service)
    {
        var list = service?.CostOptions?.Count > 1 ? service.CostOptions.ToList() : new();

        if (string.IsNullOrEmpty(dtRow["Cost (£ in pounds)"].ToString()) &&
            string.IsNullOrEmpty(dtRow["Cost per"].ToString()) &&
            string.IsNullOrEmpty(dtRow["Cost Description"].ToString()))
        {
            return list;
        }

        if (!decimal.TryParse(dtRow["Cost (£ in pounds)"].ToString(), out var amount))
        {
            amount = 0.0M;
        }

        var costId = Guid.NewGuid().ToString();
        if (service != null && service.CostOptions != null)
        {
            var costOption = (amount != 0.0M && string.IsNullOrEmpty(dtRow["Cost per"].ToString())) ? service.CostOptions.FirstOrDefault(t => t.Amount == amount && t.AmountDescription == dtRow["Cost per"].ToString()) : service.CostOptions.FirstOrDefault(t => (t.Option == dtRow["Cost Description"].ToString()));
            if (costOption != null)
            {
                costId = costOption.Id;
            }
        }

        list.Add(new CostOptionDto(
                            costId,
                            dtRow["Cost per"].ToString() ?? string.Empty,
                            amount,
                            null,
                            dtRow["Cost Description"].ToString(),
                            null,
                            null
                            ));

        return list;
    }
    private ServiceTypeDto GetServiceType(OrganisationTypeDto organisationTypeDto)
    {
        switch (organisationTypeDto.Name)
        {
            case "LA":
            case "FamilyHub":
                return new ServiceTypeDto("2", "Family Experience", "");


            default:
                return new ServiceTypeDto("1", "Information Sharing", "");

        }
    }

    private string GetServiceDeliveryId(ServiceDto? service, ServiceDeliveryType serviceDelivery)
    {
        var id = Guid.NewGuid().ToString();
        if (service != null && service.ServiceDeliveries != null)
        {
            var serviceDeliveryItem = service.ServiceDeliveries.FirstOrDefault(x => x.Name == serviceDelivery);
            if (serviceDeliveryItem != null)
            {
                id = serviceDeliveryItem.Id;
            }
        }

        return id;

    }

    private List<ServiceDeliveryDto> GetDeliveryTypes(string rowDeliveryTypes, ServiceDto? service)
    {
        List<ServiceDeliveryDto> list = new();
        var parts = rowDeliveryTypes.Split('|');
        foreach (var part in parts)
        {

            if (string.Compare(part, "In person", StringComparison.OrdinalIgnoreCase) == 0)
            {
                list.Add(new ServiceDeliveryDto(GetServiceDeliveryId(service, ServiceDeliveryType.InPerson), ServiceDeliveryType.InPerson));
            }
            else if (string.Compare(part, "online", StringComparison.OrdinalIgnoreCase) == 0)
            {
                list.Add(new ServiceDeliveryDto(GetServiceDeliveryId(service, ServiceDeliveryType.Online), ServiceDeliveryType.Online));
            }
            else if (string.Compare(part, "Telephone", StringComparison.OrdinalIgnoreCase) == 0)
            {
                list.Add(new ServiceDeliveryDto(GetServiceDeliveryId(service, ServiceDeliveryType.Telephone), ServiceDeliveryType.Telephone));
            }
        }

        return list;
    }

    private async Task<List<ServiceAtLocationDto>> GetLocationDto(int rowNumber, DataRow dtRow, ServiceDto? service)
    {
        var postcode = dtRow["Postcode"].ToString() ?? string.Empty;
        if (string.IsNullOrEmpty(postcode))
        {
            _errors.Add($"Postcode missing row: {rowNumber}");
            return new List<ServiceAtLocationDto>();
        }
        PostcodesIoResponse postcodeApiModel;

        try
        {
            if (_postCodesCache.ContainsKey(postcode))
            {
                postcodeApiModel = _postCodesCache[postcode];
            }
            else
            {
                postcodeApiModel = await _postcodeLocationClientService.LookupPostcode(dtRow["Postcode"].ToString() ?? string.Empty);
                _postCodesCache[postcode] = postcodeApiModel;
            }

        }
        catch
        {
            _errors.Add($"Failed to find postcode: {postcode} row: {rowNumber}");
            return new List<ServiceAtLocationDto>();
        }

        var serviceAtLocationId = Guid.NewGuid().ToString();
        var locationId = Guid.NewGuid().ToString();
        var addressId = Guid.NewGuid().ToString();
        var regularScheduleId = Guid.NewGuid().ToString();
        var linkTaxonomyId = Guid.NewGuid().ToString();
        if (service != null && service.ServiceAtLocations != null)
        {
            var serviceAtLocation = service.ServiceAtLocations.FirstOrDefault(x =>
                x.Location.Name == dtRow["Location name"].ToString() &&
                x.Location.PhysicalAddresses?.FirstOrDefault(l => l.PostCode == dtRow["Postcode"].ToString()) != null);

            if (service.ServiceAtLocations.Count == 1) serviceAtLocation = service.ServiceAtLocations.First();

            if (serviceAtLocation != null)
            {
                serviceAtLocationId = serviceAtLocation.Id;
                locationId = serviceAtLocation.Location.Id;
                if (serviceAtLocation.Location.PhysicalAddresses != null)
                {
                    var address = serviceAtLocation.Location.Physical_addresses.Count > 1 ? serviceAtLocation.Location.Physical_addresses.FirstOrDefault(x =>
                         x.Postal_code == dtRow["Postcode"].ToString()) : serviceAtLocation.Location.Physical_addresses.FirstOrDefault();
                    if (address != null)
                    {
                        addressId = address.Id;
                    }
                }

                if (serviceAtLocation.Location.LinkTaxonomies is { Count: > 0 })
                {
                    var linkTaxonomy = serviceAtLocation.Location.LinkTaxonomies.FirstOrDefault();
                    if (linkTaxonomy != null)
                    {
                        linkTaxonomyId = linkTaxonomy.Id;
                    }
                }

                if (serviceAtLocation.RegularSchedules != null)
                {
                    var regularSchedule = serviceAtLocation.RegularSchedules.FirstOrDefault();
                    if (regularSchedule != null)
                    {
                        regularScheduleId = regularSchedule.Id;
                    }
                }
            }
        }

        var addressLines = dtRow["Address line 1"].ToString();
        if (!string.IsNullOrEmpty(dtRow["Address line 2"].ToString()))
        {
            addressLines += " | " + dtRow["Address line 2"];
        }

        List<LinkTaxonomyDto> linkTaxonomyList = new();
        if (dtRow["Organisation Type"].ToString()?.ToLower() == "family hub")
        {
            var taxonomy = _taxonomies.FirstOrDefault(x => x.Name == "FamilyHub");
            if (taxonomy != null)
            {
                linkTaxonomyList.Add(new LinkTaxonomyDto(linkTaxonomyId, "Location", locationId, taxonomy));
            }

        }


        var serviceAtLocations = new List<ServiceAtLocationDto>();
        var regularScheduleDto = new List<RegularScheduleDto>();
        if (!string.IsNullOrEmpty(dtRow["Opening hours description"].ToString()))
        {
            regularScheduleDto.Add(new RegularScheduleDto(
                          regularScheduleId,
                          dtRow["Opening hours description"].ToString() ?? string.Empty,
                          null,
                          null,
                          null,
                          null,
                          null,
                          null,
                          null,
                          null,
                          null));
        }


        serviceAtLocations.Add(

            new ServiceAtLocationDto(
                serviceAtLocationId,
                new LocationDto(
                    locationId,
                    dtRow["Location name"].ToString() ?? string.Empty,
                    dtRow["Location description"].ToString(),
                    postcodeApiModel.Result.Latitude,
                    postcodeApiModel.Result.Longitude,
                    new List<PhysicalAddressDto>()
                    {
                        new PhysicalAddressDto(
                            addressId,
                            addressLines ?? string.Empty,
                            dtRow["Town or City"].ToString(),
                            dtRow["Postcode"].ToString() ?? string.Empty,
                            "England",
                            dtRow["County"].ToString()
                            )
                    }, linkTaxonomyList
                ),
                regularScheduleDto,
                new List<HolidayScheduleDto>()
                )
        );

        service?.ServiceAtLocations?.Add(serviceAtLocations.First());

        return service?.ServiceAtLocations?.ToList() ?? serviceAtLocations;
    }


    private async Task<OrganisationDto?> GetOrganisationsWithOutServices(string organisationName)
    {
        if (!_organisations.Any() || _organisations.Count(x => x.Name == organisationName) == 0)
        {
            _organisations = await _OrganisationAdminClientService.GetListOrganisations();
        }

        var organisation = _organisations.FirstOrDefault(x => organisationName.Contains(x.Name ?? string.Empty));
        if (organisation == null)
        {
            return null;
        }
        return organisation;
    }



    private async Task<OrganisationWithServicesDto?> GetOrganisation(string organisationName)
    {
        if (!_organisations.Any() || _organisations.Count(x => x.Name == organisationName) == 0)
        {
            _organisations = await _OrganisationAdminClientService.GetListOrganisations();
        }

        var organisation = _organisations.FirstOrDefault(x => string.Equals(x.Name, organisationName, StringComparison.InvariantCultureIgnoreCase));
        if (organisation == null)
        {
            return null;
        }

        var organisationWithServices = _organisationsWithServices.FirstOrDefault(o => o.Id == organisation.Id);

        if (organisationWithServices is null || organisationWithServices.Services is { Count: >= 0 })
        {
            organisationWithServices = await _OrganisationAdminClientService.GetOrganisationById(organisation.Id);

            _organisationsWithServices.Add(organisationWithServices);
        }

        organisationWithServices.AdminAreaCode = organisation.AdminAreaCode;

        return organisationWithServices;
    }
}
