using FamilyHubs.ServiceDirectory.Shared.Builders;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralContacts;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralCostOptions;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralEligibilitys;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralHolidaySchedule;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralLanguages;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralLinkTaxonomies;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralLocations;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralOrganisations;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralPhysicalAddresses;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralRegularSchedule;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServiceAtLocations;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServiceDeliverysEx;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServices;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServiceTaxonomys;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralTaxonomys;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OrganisationType;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.ServiceType;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using System.Data;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.DataUpload;


public interface IDataUploadService
{
    Task<List<string>> UploadToApi(string organisationId, BufferedSingleFileUploadDb fileUpload, bool useSpreadsheetServiceId = false);
}

public class DataUploadService : IDataUploadService
{
    private readonly IOpenReferralOrganisationAdminClientService _openReferralOrganisationAdminClientService;
    private readonly IPostcodeLocationClientService _postcodeLocationClientService;

    private bool _useSpreadsheetServiceId = true;
    private List<OpenReferralOrganisationDto> _organisations = new();
    private readonly List<OpenReferralOrganisationWithServicesDto> _organisationsWithServices = new();
    private readonly List<OpenReferralTaxonomyDto> _taxonomies = new();
    private readonly List<string> _errors = new List<string>();
    private readonly Dictionary<string, PostcodesIoResponse> _postCodesCache = new Dictionary<string, PostcodesIoResponse>();

    public DataUploadService(IOpenReferralOrganisationAdminClientService openReferralOrganisationAdminClientService, IPostcodeLocationClientService postcodeLocationClientService)
    {
        _openReferralOrganisationAdminClientService = openReferralOrganisationAdminClientService;
        _postcodeLocationClientService = postcodeLocationClientService;
    }

    public async Task<List<string>> UploadToApi(string organisationId, BufferedSingleFileUploadDb fileUpload, bool useSpreadsheetServiceId = false)
    {
        _useSpreadsheetServiceId = useSpreadsheetServiceId;
        var taxonomies = await _openReferralOrganisationAdminClientService.GetTaxonomyList(1, 999999999);
        _taxonomies.AddRange(taxonomies.Items);
        var dtExcelTable = await ExcelReader.GetRequestsDataFromExcel(fileUpload);
        await ProcessRows(dtExcelTable);
        return _errors;
    }

    private async Task ProcessRows(DataTable dtExcelTable)
    {
        var rowNumber = 6;

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
            OpenReferralOrganisationWithServicesDto? openReferralOrganisationDto;
            if (organisationTypeDto.Name == "LA" || organisationTypeDto.Name == "FamilyHub")
            {
                openReferralOrganisationDto = await GetOrganisation(dtRow["Local authority"].ToString() ?? string.Empty);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(organisationName))
                {
                    _errors.Add($"Name of organisation missing row:{rowNumber}");
                    continue;
                }
                openReferralOrganisationDto = await GetOrganisation(organisationName);
                if (openReferralOrganisationDto == null)
                {
                    openReferralOrganisationDto = new OpenReferralOrganisationWithServicesDto
                    (
                        id: Guid.NewGuid().ToString(),
                        organisationType: organisationTypeDto,
                        name: organisationName,
                        description: organisationName,
                        logo: null,
                        uri: dtRow["Website"].ToString(),
                        url: dtRow["Website"].ToString(),
                        services: null
                    )
                    {
                        AdminAreaCode = localAuthority.AdminAreaCode
                    };

                    newOrganisation = true;
                }
            }

            if (newOrganisation)
            {
                var service = await GetServiceFromRow(rowNumber, dtRow, null, organisationTypeDto, openReferralOrganisationDto?.Id ?? string.Empty);
                if (openReferralOrganisationDto != null && service != null)
                {
                    openReferralOrganisationDto.Services = new List<OpenReferralServiceDto>()
                    {
                        service
                    };

                    try
                    {
                        //Create Organisation
                        var _ = await _openReferralOrganisationAdminClientService.CreateOrganisation(openReferralOrganisationDto);
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
                OpenReferralServiceDto? service;
                if (_useSpreadsheetServiceId)
                {
                    if ((string.IsNullOrEmpty(dtRow["Service unique identifier"].ToString())))
                    {
                        _errors.Add($"Service unique identifier missing row:{rowNumber}");
                        continue;
                    }

                    service = openReferralOrganisationDto?.Services?.FirstOrDefault(x => x.Id == $"{openReferralOrganisationDto.AdminAreaCode?.Remove(0, 1)}{dtRow["Service unique identifier"]}");

                }
                else
                {
                    service = openReferralOrganisationDto?.Services?.FirstOrDefault(x => x.Name == dtRow["Name of service"].ToString());
                }

                if (service != null)
                {
                    isNewService = false;
                }
                service = await GetServiceFromRow(rowNumber, dtRow, service, organisationTypeDto, openReferralOrganisationDto?.Id ?? string.Empty);

                if (isNewService)
                {
                    if (service != null)
                    {
                        try
                        {
                            var _ = await _openReferralOrganisationAdminClientService.CreateService(service);
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
                            var _ = await _openReferralOrganisationAdminClientService.UpdateService(service);
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

    private async Task<OpenReferralServiceDto?> GetServiceFromRow(int rowNumber, DataRow dtRow, OpenReferralServiceDto? service, OrganisationTypeDto organisationTypeDto, string organisationId)
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
        var result = builder.WithMainProperties(id: serviceId,
                                   serviceType: GetServiceType(organisationTypeDto),
                                   organisationId: organisationId,
                                   name: dtRow["Name of service"].ToString() ?? string.Empty,
                                   description: description,
                                   accreditations: null,
                                   assured_date: null,
                                   attending_access: null,
                                   attending_type: dtRow["Delivery method"].ToString(),
                                   deliverable_type: dtRow["Delivery method"].ToString(),
                                   status: "active",
                                   url: dtRow["Website"].ToString(),
                                   email: dtRow["Contact email"].ToString(),
                                   fees: string.Empty,
                                   canFamilyChooseDeliveryLocation: false)
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

    private List<OpenReferralContactDto> GetContacts(DataRow dtRow, OpenReferralServiceDto? service)
    {
        var contactId = Guid.NewGuid().ToString();
        var openReferralContacts = (service != null && service.Contacts != null) ? service.Contacts.ToList() : new List<OpenReferralContactDto>();
        if (service != null && service.Contacts != null)
        {
            var contact = service.Contacts?.FirstOrDefault(x => x.Name == "Telephone");
            if (contact != null)
            {
                contactId = contact.Id;
            }

            if (!string.IsNullOrEmpty(dtRow["Contact phone"].ToString()))
            {


                openReferralContacts.Add(new OpenReferralContactDto(
                contactId,
                "",
                "Telephone",
                dtRow["Contact phone"].ToString() ?? string.Empty,
                dtRow["Contact sms"].ToString() ?? string.Empty
                ));

            }
        }

        return openReferralContacts;
    }

    private List<OpenReferralEligibilityDto> GetEligibilities(DataRow dtRow, OpenReferralServiceDto? service)
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
            var eligibleItem = service.Eligibilities?.FirstOrDefault(x => x.Minimum_age == minimumAge && x.Maximum_age == maximumAge);
            if (eligibleItem != null)
            {
                eligibilityId = eligibleItem.Id;
            }
        }

        list.Add(new OpenReferralEligibilityDto(eligibilityId, eligibility, maximumAge, minimumAge));

        return list;
    }

    private List<OpenReferralServiceTaxonomyDto> GetTaxonomies(DataRow dtRow)
    {
        List<OpenReferralServiceTaxonomyDto> list = new();
        var categories = dtRow["Sub-category"].ToString();
        if (!string.IsNullOrEmpty(categories))
        {
            var parts = categories.Split('|');
            foreach (var part in parts)
            {
                var taxonomy = _taxonomies.FirstOrDefault(x => x.Name.ToLower() == part.Trim().ToLower());
                if (taxonomy != null)
                {
                    list.Add(new OpenReferralServiceTaxonomyDto(Guid.NewGuid().ToString(), taxonomy));
                }

            }
        }
        return list;
    }

    private List<OpenReferralLanguageDto> GetLanguages(DataRow dtRow, OpenReferralServiceDto? service)
    {
        var list = (service != null && service.Languages != null) ? service.Languages.ToList() : new List<OpenReferralLanguageDto>();
        var languages = dtRow["Language"].ToString();
        if (!string.IsNullOrEmpty(languages))
        {
            string[] parts = languages.Split('|');
            foreach (var part in parts)
            {
                var languageId = Guid.NewGuid().ToString();
                if (service != null && service.Languages != null)
                {
                    var originalLanguage = service.Languages.FirstOrDefault(x => x.Language == part);
                    if (originalLanguage != null)
                    {
                        languageId = originalLanguage.Id;
                    }
                }

                list.Add(new OpenReferralLanguageDto(languageId, part.Trim()));
            }
        }

        return list;
    }

    private List<OpenReferralCostOptionDto> GetCosts(DataRow dtRow, OpenReferralServiceDto? service)
    {
        var list = (service != null && service.Cost_options != null) ? service.Cost_options.ToList() : new();
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
        if (service != null && service.Cost_options != null)
        {
            var costOption = (amount != 0.0M && string.IsNullOrEmpty(dtRow["Cost per"].ToString())) ? service.Cost_options.FirstOrDefault(t => t.Amount == amount && t.Amount_description == dtRow["Cost per"].ToString()) : service.Cost_options.FirstOrDefault(t => (t.Option == dtRow["Cost Description"].ToString()));
            if (costOption != null)
            {
                costId = costOption.Id;
            }
        }

        list.Add(new OpenReferralCostOptionDto(
                            costId,
                            amount_description: dtRow["Cost per"].ToString() ?? string.Empty,
                            amount: amount,
                            linkId: null,
                            option: dtRow["Cost Description"].ToString(),
                            valid_from: null,
                            valid_to: null
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

    private string GetServiceDeliveryId(OpenReferralServiceDto? service, ServiceDelivery serviceDelivery)
    {
        var id = Guid.NewGuid().ToString();
        if (service != null && service.ServiceDelivery != null)
        {
            var serviceDeliveryItem = service.ServiceDelivery.FirstOrDefault(x => x.ServiceDelivery == serviceDelivery);
            if (serviceDeliveryItem != null)
            {
                id = serviceDeliveryItem.Id;
            }
        }

        return id;

    }

    private List<OpenReferralServiceDeliveryExDto> GetDeliveryTypes(string rowDeliveryTypes, OpenReferralServiceDto? service)
    {
        List<OpenReferralServiceDeliveryExDto> list = new();
        var parts = rowDeliveryTypes.Split('|');
        foreach (var part in parts)
        {

            if (string.Compare(part, "In person", StringComparison.OrdinalIgnoreCase) == 0)
            {
                list.Add(new OpenReferralServiceDeliveryExDto(GetServiceDeliveryId(service, ServiceDelivery.InPerson), ServiceDelivery.InPerson));
            }
            else if (string.Compare(part, "online", StringComparison.OrdinalIgnoreCase) == 0)
            {
                list.Add(new OpenReferralServiceDeliveryExDto(GetServiceDeliveryId(service, ServiceDelivery.Online), ServiceDelivery.Online));
            }
            else if (string.Compare(part, "Telephone", StringComparison.OrdinalIgnoreCase) == 0)
            {
                list.Add(new OpenReferralServiceDeliveryExDto(GetServiceDeliveryId(service, ServiceDelivery.Telephone), ServiceDelivery.Telephone));
            }
        }

        return list;
    }

    private async Task<List<OpenReferralServiceAtLocationDto>> GetLocationDto(int rowNumber, DataRow dtRow, OpenReferralServiceDto? service)
    {
        var postcode = dtRow["Postcode"].ToString() ?? string.Empty;
        if (string.IsNullOrEmpty(postcode))
        {
            _errors.Add($"Postcode missing row: {rowNumber}");
            return new List<OpenReferralServiceAtLocationDto>();
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
            return new List<OpenReferralServiceAtLocationDto>();
        }

        var serviceAtLocationId = Guid.NewGuid().ToString();
        var locationId = Guid.NewGuid().ToString();
        var addressId = Guid.NewGuid().ToString();
        var regularScheduleId = Guid.NewGuid().ToString();
        var linkTaxonomyId = Guid.NewGuid().ToString();
        if (service != null && service.Service_at_locations != null)
        {
            var serviceAtLocation = service.Service_at_locations.FirstOrDefault(x =>
                x.Location.Name == dtRow["Location name"].ToString() &&
                x.Location.Physical_addresses?.FirstOrDefault(l => l.Postal_code == dtRow["Postcode"].ToString()) != null);
            if (serviceAtLocation != null)
            {
                serviceAtLocationId = serviceAtLocation.Id;
                locationId = serviceAtLocation.Location.Id;
                if (serviceAtLocation.Location.Physical_addresses != null)
                {
                    var address = serviceAtLocation.Location.Physical_addresses.FirstOrDefault(x =>
                        x.Postal_code == dtRow["Postcode"].ToString());
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

                if (serviceAtLocation.Regular_schedule != null)
                {
                    var regularSchedule = serviceAtLocation.Regular_schedule.FirstOrDefault();
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

        List<OpenReferralLinkTaxonomyDto> linkTaxonomyList = new();
        if (dtRow["Organisation Type"].ToString()?.ToLower() == "family hub")
        {
            var taxonomy = _taxonomies.FirstOrDefault(x => x.Name == "FamilyHub");
            if (taxonomy != null)
            {
                linkTaxonomyList.Add(new OpenReferralLinkTaxonomyDto(linkTaxonomyId, "Location", locationId, taxonomy));
            }

        }


        var serviceAtLocations = new List<OpenReferralServiceAtLocationDto>();
        var regularScheduleDto = new List<OpenReferralRegularScheduleDto>();
        if (!string.IsNullOrEmpty(dtRow["Opening hours description"].ToString()))
        {
            regularScheduleDto.Add(new OpenReferralRegularScheduleDto(
                          id: regularScheduleId,
                          description: dtRow["Opening hours description"].ToString() ?? string.Empty,
                          opens_at: null,
                          closes_at: null,
                          byday: null,
                          bymonthday: null,
                          dtstart: null,
                          freq: null,
                          interval: null,
                          valid_from: null,
                          valid_to: null));
        }


        serviceAtLocations.Add(

            new OpenReferralServiceAtLocationDto(
                serviceAtLocationId,
                new OpenReferralLocationDto(
                    locationId,
                    dtRow["Location name"].ToString() ?? string.Empty,
                    dtRow["Location description"].ToString(),
                    postcodeApiModel.Result.Latitude,
                    postcodeApiModel.Result.Longitude,
                    new List<OpenReferralPhysicalAddressDto>()
                    {
                        new OpenReferralPhysicalAddressDto(
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
                new List<OpenReferralHolidayScheduleDto>()
                )
        );

        service?.Service_at_locations?.Add(serviceAtLocations.First());

        return (service?.Service_at_locations != null) ? service.Service_at_locations.ToList() : serviceAtLocations;
    }


    private async Task<OpenReferralOrganisationDto?> GetOrganisationsWithOutServices(string organisationName)
    {
        if (!_organisations.Any() || _organisations.Count(x => x.Name == organisationName) == 0)
        {
            _organisations = await _openReferralOrganisationAdminClientService.GetListOpenReferralOrganisations();
        }

        var organisation = _organisations.FirstOrDefault(x => organisationName.Contains(x.Name ?? string.Empty));
        if (organisation == null)
        {
            return null;
        }
        return organisation;
    }



    private async Task<OpenReferralOrganisationWithServicesDto?> GetOrganisation(string organisationName)
    {
        if (!_organisations.Any() || _organisations.Count(x => x.Name == organisationName) == 0)
        {
            _organisations = await _openReferralOrganisationAdminClientService.GetListOpenReferralOrganisations();
        }

        var organisation = _organisations.FirstOrDefault(x => organisationName.Contains(x.Name ?? string.Empty));
        if (organisation == null)
        {
            return null;
        }

        var organisationWithServices = _organisationsWithServices.FirstOrDefault(o => o.Id == organisation.Id);
        if (organisationWithServices is null || organisationWithServices?.Services is { Count: >= 0 })
        {
            organisationWithServices = await _openReferralOrganisationAdminClientService.GetOpenReferralOrganisationById(organisation.Id);

            _organisationsWithServices.Add(organisationWithServices);
        }

        if (organisationWithServices != null)
        {
            organisationWithServices.AdminAreaCode = organisation.AdminAreaCode;
        }

        return organisationWithServices;
    }
}
