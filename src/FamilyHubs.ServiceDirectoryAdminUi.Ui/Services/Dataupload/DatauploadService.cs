using FamilyHubs.ServiceDirectory.Shared.Builders;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.DataUpload.Helpers;
using System.Data;

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
        var dtExcelTable = await _excelReader.GetRequestsDataFromExcel(fileUpload);
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

            if(!OrganisationHelper.TryResolveOrganisationType(dtRow, out organisationTypeDto, out organisationName))
            {
                _errors.Add($"Name of organisation missing row:{rowNumber}");
                continue;
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
                                   ServiceHelper.GetServiceType(organisationTypeDto),
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
                        .WithServiceDelivery(ServiceHelper.GetDeliveryTypes(dtRow["Delivery method"].ToString() ?? string.Empty, service))
                        .WithServiceAtLocations(locations)
                        .WithLinkContact(ContactHelper.GetLinkContacts(serviceId, LinkContactTypes.SERVICE, dtRow, service?.LinkContacts, _contacts))
                        .WithCostOption(ServiceHelper.GetCosts(dtRow, service))
                        .WithLanguages(ServiceHelper.GetLanguages(dtRow, service))
                        .WithServiceTaxonomies(GetTaxonomies(dtRow))
                        .WithEligibility(ServiceHelper.GetEligibilities(dtRow, service))
                        .Build();

        return result;

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

    private async Task<List<ServiceAtLocationDto>> GetLocationDto(int rowNumber, DataRow dtRow, ServiceDto? service)
    {
        var postcode = dtRow["Postcode"].ToString() ?? string.Empty;
        if (string.IsNullOrEmpty(postcode))
        {
            var deliveryMethod = dtRow["Delivery method"].ToString();
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
        ICollection<LinkContactDto>? linkContacts = new List<LinkContactDto>();

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
                linkContacts = serviceAtLocation.LinkContacts;
                if (serviceAtLocation.Location.PhysicalAddresses != null)
                {
                    var address = serviceAtLocation.Location.PhysicalAddresses.Count > 1 ? serviceAtLocation.Location.PhysicalAddresses.FirstOrDefault(x =>
                         x.PostCode == dtRow["Postcode"].ToString()) : serviceAtLocation.Location.PhysicalAddresses.FirstOrDefault();
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

        var location = new LocationDto(
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
                    }, linkTaxonomyList,
                    new List<LinkContactDto>()
                );

        serviceAtLocations.Add(
            new ServiceAtLocationDto(
                serviceAtLocationId,
                location,
                regularScheduleDto,
                new List<HolidayScheduleDto>(),
                ContactHelper.GetLinkContacts(serviceAtLocationId, LinkContactTypes.SERVICE_AT_LOCATION, dtRow, linkContacts, _contacts)
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