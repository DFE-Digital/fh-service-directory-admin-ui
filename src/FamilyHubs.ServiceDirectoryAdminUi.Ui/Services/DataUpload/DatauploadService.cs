using FamilyHubs.ServiceDirectory.Shared.Builders;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.DataUpload.Helpers;

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
    private readonly List<ContactDto> _contacts = new();
    private readonly IExcelReader _excelReader;

    public DataUploadService(
        IOrganisationAdminClientService OrganisationAdminClientService, 
        IPostcodeLocationClientService postcodeLocationClientService,
        IExcelReader excelReader)
    {
        _OrganisationAdminClientService = OrganisationAdminClientService;
        _postcodeLocationClientService = postcodeLocationClientService;
        _excelReader = excelReader;
    }

    public async Task<List<string>> UploadToApi(string organisationId, BufferedSingleFileUploadDb fileUpload, bool useSpreadsheetServiceId = false)
    {
        _useSpreadsheetServiceId = useSpreadsheetServiceId;
        var taxonomies = await _OrganisationAdminClientService.GetTaxonomyList(1, 999999999);
        _taxonomies.AddRange(taxonomies.Items);
        var uploadData = await _excelReader.GetRequestsDataFromExcel(fileUpload);
        await ProcessRows(uploadData);
        return _errors;
    }

    private async Task ProcessRows(Dictionary<int, DataUploadRow> uploadData)
    {

        foreach (KeyValuePair<int, DataUploadRow> item in uploadData)
        {
            var rowNumber = item.Key;
            var dtRow = item.Value;

            var localAuthority = await GetOrganisationsWithOutServices(dtRow.LocalAuthority!);
            if (localAuthority == null)
            {
                _errors.Add($"Failed to find local authority row:{rowNumber}");
                continue;
            }

            OrganisationTypeDto organisationTypeDto;
            string? organisationName;

            if(!OrganisationHelper.TryResolveOrganisationType(dtRow, out organisationTypeDto, out organisationName))
            {
                _errors.Add($"Name of organisation missing row:{rowNumber}");
                continue;
            }

            var newOrganisation = false;
            OrganisationWithServicesDto? OrganisationDto;
            if (organisationTypeDto.Name == "LA" || organisationTypeDto.Name == "FamilyHub")
            {
                OrganisationDto = await GetOrganisation(dtRow.LocalAuthority!);
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
                        dtRow.Website,
                        dtRow.Website
                    )
                    {
                        AdminAreaCode = localAuthority.AdminAreaCode
                    };

                    newOrganisation = true;
                }
            }

            _contacts.AddRange(ContactHelper.GetAllContactsFromOrganisation(OrganisationDto));

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
                    if ((string.IsNullOrEmpty(dtRow.ServiceUniqueId)))
                    {
                        _errors.Add($"Service unique identifier missing row:{rowNumber}");
                        continue;
                    }

                    service = OrganisationDto?.Services?.FirstOrDefault(x => x.Id == $"{OrganisationDto.AdminAreaCode?.Remove(0, 1)}{dtRow.ServiceUniqueId}");

                }
                else
                {
                    service = OrganisationDto?.Services?.FirstOrDefault(x => x.Name == dtRow.NameOfService);
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

    private async Task<ServiceDto?> GetServiceFromRow(int rowNumber, DataUploadRow dtRow, ServiceDto? service, OrganisationTypeDto organisationTypeDto, string organisationId)
    {
        var description = dtRow.ServiceDescription;

        var locations = await GetLocationDto(rowNumber, dtRow, service);
        
        var serviceId = service?.Id ?? Guid.NewGuid().ToString();
        if (string.IsNullOrEmpty(dtRow.ServiceUniqueId))
        {
            _errors.Add($"Service unique identifier missing row:{rowNumber}");
            return null;
        }
        if (service == null && _useSpreadsheetServiceId && !string.IsNullOrEmpty(dtRow.ServiceUniqueId))
        {
            var organisation = await GetOrganisationsWithOutServices(dtRow.LocalAuthority ?? string.Empty);
            serviceId = organisation is not null ?
            $"{organisation.AdminAreaCode?.Remove(0, 1)}{dtRow.ServiceUniqueId}" : Guid.NewGuid().ToString();
        }

        var builder = new ServicesDtoBuilder();
        var result = builder.WithMainProperties(serviceId,
                                   ServiceHelper.GetServiceType(organisationTypeDto),
                                   organisationId,
                                   dtRow.NameOfService!,
                                   description,
                                   null,
                                   null,
                                   null,
                                   dtRow.DeliveryMethod,
                                   dtRow.DeliveryMethod,
                                   "active",
                                   string.Empty,
                                   false)
                        .WithServiceDelivery(ServiceHelper.GetDeliveryTypes(dtRow.DeliveryMethod ?? string.Empty, service))
                        .WithServiceAtLocations(locations)
                        .WithLinkContact(ContactHelper.GetLinkContacts(serviceId, LinkContactTypes.SERVICE, dtRow, service?.LinkContacts, _contacts, rowNumber, _errors))
                        .WithCostOption(ServiceHelper.GetCosts(dtRow, service))
                        .WithLanguages(ServiceHelper.GetLanguages(dtRow, service))
                        .WithServiceTaxonomies(GetTaxonomies(dtRow))
                        .WithEligibility(ServiceHelper.GetEligibilities(dtRow, service))
                        .Build();

        return result;

    }

    private List<ServiceTaxonomyDto> GetTaxonomies(DataUploadRow dtRow)
    {
        List<ServiceTaxonomyDto> list = new();
        var categories = dtRow.SubCategory;
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

    private async Task<List<ServiceAtLocationDto>> GetLocationDto(int rowNumber, DataUploadRow dtRow, ServiceDto? service)
    {
        var postcode = dtRow.Postcode;
        if (string.IsNullOrEmpty(postcode))
        {
            var deliveryMethod = dtRow.DeliveryMethod;
            if (deliveryMethod != null && deliveryMethod.Contains("In person"))
            {
                _errors.Add($"Postcode missing row: {rowNumber}");
            }

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
                postcodeApiModel = await _postcodeLocationClientService.LookupPostcode(postcode);
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
        ICollection<LinkContactDto>? linkContacts = new List<LinkContactDto>();

        if (service != null && service.ServiceAtLocations != null)
        {
            var serviceAtLocation = service.ServiceAtLocations.FirstOrDefault(x =>
                x.Location.Name == dtRow.LocationName &&
                x.Location.PhysicalAddresses?.FirstOrDefault(l => l.PostCode == dtRow.Postcode) != null);

            if (service.ServiceAtLocations.Count == 1) serviceAtLocation = service.ServiceAtLocations.First();

            if (serviceAtLocation != null)
            {
                serviceAtLocationId = serviceAtLocation.Id;
                locationId = serviceAtLocation.Location.Id;
                linkContacts = serviceAtLocation.LinkContacts;
                if (serviceAtLocation.Location.PhysicalAddresses != null)
                {
                    var address = serviceAtLocation.Location.PhysicalAddresses.Count > 1 ? serviceAtLocation.Location.PhysicalAddresses.FirstOrDefault(x =>
                         x.PostCode == dtRow.Postcode) : serviceAtLocation.Location.PhysicalAddresses.FirstOrDefault();
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

        var addressLines = dtRow.AddressLineOne;
        if (!string.IsNullOrEmpty(dtRow.AddressLineTwo))
        {
            addressLines += " | " + dtRow.AddressLineTwo;
        }

        List<LinkTaxonomyDto> linkTaxonomyList = new();
        if (dtRow.OrganisationType?.ToLower() == "family hub")
        {
            var taxonomy = _taxonomies.FirstOrDefault(x => x.Name == "FamilyHub");
            if (taxonomy != null)
            {
                linkTaxonomyList.Add(new LinkTaxonomyDto(linkTaxonomyId, "Location", locationId, taxonomy));
            }

        }


        var serviceAtLocations = new List<ServiceAtLocationDto>();
        var regularScheduleDto = new List<RegularScheduleDto>();
        if (!string.IsNullOrEmpty(dtRow.OpeningHoursDescription))
        {
            regularScheduleDto.Add(new RegularScheduleDto(
                          regularScheduleId,
                          dtRow.OpeningHoursDescription ?? string.Empty,
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

        var location = new LocationDto(
                    locationId,
                    dtRow.LocationName!,
                    dtRow.LocationDescription,
                    postcodeApiModel.Result.Latitude,
                    postcodeApiModel.Result.Longitude,
                    new List<PhysicalAddressDto>()
                    {
                        new PhysicalAddressDto(
                            addressId,
                            addressLines ?? string.Empty,
                            dtRow.TownOrCity,
                            dtRow.Postcode!,
                            "England",
                            dtRow.County
                            )
                    }, linkTaxonomyList,
                    new List<LinkContactDto>()
                );

        serviceAtLocations.Add(
            new ServiceAtLocationDto(
                serviceAtLocationId,
                location,
                regularScheduleDto,
                new List<HolidayScheduleDto>(),
                ContactHelper.GetLinkContacts(serviceAtLocationId, LinkContactTypes.SERVICE_AT_LOCATION, dtRow, linkContacts, _contacts, rowNumber, _errors)
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