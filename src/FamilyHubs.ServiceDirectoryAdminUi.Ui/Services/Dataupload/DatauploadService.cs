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
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralPhones;
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
using FamilyHubs.SharedKernel;
using System.Data;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Dataupload;


public interface IDatauploadService
{
    Task<List<string>> UploadToApi(string organisationId, BufferedSingleFileUploadDb fileUpload, bool useSpreadsheetServiceId = false);
}

public class DatauploadService : IDatauploadService
{
    private readonly IOpenReferralOrganisationAdminClientService _openReferralOrganisationAdminClientService;
    private readonly IPostcodeLocationClientService _postcodeLocationClientService;

    private bool _useSpreadsheetServiceId = true;
    private List<OpenReferralOrganisationDto> _organisations = new();
    private List<OpenReferralOrganisationWithServicesDto> _organisationsWithServices = new();
    private List<OpenReferralTaxonomyDto> _taxonomies = new();
    private List<string> _errors = new List<string>();
    private Dictionary<string, PostcodeApiModel> dicPostCodes = new Dictionary<string, PostcodeApiModel>();

    public DatauploadService(IOpenReferralOrganisationAdminClientService openReferralOrganisationAdminClientService, IPostcodeLocationClientService postcodeLocationClientService)
    {
        _openReferralOrganisationAdminClientService = openReferralOrganisationAdminClientService;
        _postcodeLocationClientService = postcodeLocationClientService;
    }

    public async Task<List<string>> UploadToApi(string organisationId, BufferedSingleFileUploadDb fileUpload, bool useSpreadsheetServiceId = false)
    {
        _useSpreadsheetServiceId = useSpreadsheetServiceId;
        PaginatedList<OpenReferralTaxonomyDto> taxonomies = await _openReferralOrganisationAdminClientService.GetTaxonomyList(1, 999999999);
        _taxonomies.AddRange(taxonomies.Items);
        DataTable dtExcelTable = await ExcelReader.GetRequestsDataFromExcel(fileUpload);
        await ProcessRows(organisationId, dtExcelTable);
        return _errors;
    }

    private async Task ProcessRows(string organisationId, DataTable dtExcelTable)
    {
        int rowNumber = 6;
        foreach (DataRow dtRow in dtExcelTable.Rows)
        {
            rowNumber++;

            OpenReferralOrganisationDto? localAuthority = await GetOrganisationsWithOutServices(dtRow["Local authority"]?.ToString() ?? string.Empty);
            if (localAuthority == null)
            {
                _errors.Add($"Failed to find local authority row:{rowNumber}");
                continue;
            }


            var organisationType = dtRow["Organisation Type"].ToString();
            OrganisationTypeDto organisationTypeDto;
            string? organisationName = null;
            switch (dtRow["Organisation Type"].ToString()?.ToLower())
            {
                case "local authority":
                    organisationTypeDto = new OrganisationTypeDto("1", "LA", "Local Authority");
                    organisationName = dtRow["Local authority"] != null ? dtRow["Local authority"].ToString() : string.Empty;
                    break;
                case "voluntary and community sector":
                    organisationTypeDto = new OrganisationTypeDto("2", "VCFS", "Voluntary, Charitable, Faith Sector");
                    organisationName = dtRow["Name of organisation"] != null ? dtRow["Name of organisation"].ToString() : string.Empty;
                    break;
                case "family hub":
                    organisationTypeDto = new OrganisationTypeDto("3", "FamilyHub", "Family Hub");
                    organisationName = dtRow["Local authority"] != null ? dtRow["Local authority"].ToString() : string.Empty;
                    break;
                default:
                    organisationTypeDto = new OrganisationTypeDto("4", "Company", "Public / Private Company eg: Child Care Centre");
                    organisationName = dtRow["Name of organisation"] != null ? dtRow["Name of organisation"].ToString() : string.Empty;
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


            bool newOrganisation = false;
            OpenReferralOrganisationWithServicesDto? openReferralOrganisationDto;
            if (organisationTypeDto.Name == "LA" || organisationTypeDto.Name == "FamilyHub")
            {
                openReferralOrganisationDto = await GetOrganisation(dtRow["Local authority"]?.ToString() ?? string.Empty);
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
                        uri: dtRow["Website"]?.ToString(),
                        url: dtRow["Website"]?.ToString(),
                        services: null
                    );

                    openReferralOrganisationDto.AdministractiveDistrictCode = localAuthority.AdministractiveDistrictCode;
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
                        var id = await _openReferralOrganisationAdminClientService.CreateOrganisation(openReferralOrganisationDto);
                    }
                    catch
                    {
                        _errors.Add($"Failed to create organisation with service row:{rowNumber}");
                    }

                }
            }
            else
            {
                bool isNewService = true;
                OpenReferralServiceDto? service = null;
                if (_useSpreadsheetServiceId)
                {
                    if ((dtRow["Service unique identifier"] == null || string.IsNullOrEmpty(dtRow["Service unique identifier"].ToString())))
                    {
                        _errors.Add($"Service unique identifier missing row:{rowNumber}");
                        continue;
                    }

                    service = openReferralOrganisationDto?.Services?.FirstOrDefault(x => x.Id == $"{openReferralOrganisationDto?.AdministractiveDistrictCode?.Remove(0, 1)}{dtRow["Service unique identifier"].ToString()}");
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
                            var id = await _openReferralOrganisationAdminClientService.CreateService(service);
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
                            var id = await _openReferralOrganisationAdminClientService.UpdateService(service);
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

    private async Task<OpenReferralServiceDto?> GetServiceFromRow(int rownumber, DataRow dtRow, OpenReferralServiceDto? service, OrganisationTypeDto organisationTypeDto, string organisationId)
    {
        var description = dtRow["More Details (service description)"]?.ToString();

        List<OpenReferralServiceAtLocationDto> locations = await GetLocationDto(rownumber, dtRow, service);
        if (!locations.Any())
            return null;

        string serviceId = service?.Id ?? Guid.NewGuid().ToString();
        if (dtRow["Service unique identifier"] == null || string.IsNullOrEmpty(dtRow["Service unique identifier"].ToString()))
        {
            _errors.Add($"Service unique identifier missing row:{rownumber}");
            return null;
        }
        if (service == null && _useSpreadsheetServiceId && dtRow["Service unique identifier"] != null && !string.IsNullOrEmpty(dtRow["Service unique identifier"].ToString()))
        {
            var organisation = await GetOrganisationsWithOutServices(dtRow["Local authority"]?.ToString() ?? string.Empty);
            serviceId = organisation is not null ?
            //$"{organisation?.AdministractiveDistrictCode?.Remove(0, 1)}{dtRow["Service unique identifier"].ToString()}" ?? Guid.NewGuid().ToString() : Guid.NewGuid().ToString();
            $"{dtRow["Service unique identifier"].ToString()}" ?? Guid.NewGuid().ToString() : Guid.NewGuid().ToString();
        }

        ServicesDtoBuilder builder = new ServicesDtoBuilder();
        var result = builder.WithMainProperties(id: serviceId,
                                   serviceType: GetServiceType(organisationTypeDto ?? new OrganisationTypeDto("2", "VCFS", "Voluntary, Charitable, Faith Sector")),
                                   organisationId: organisationId ?? string.Empty,
                                   name: dtRow["Name of service"].ToString() ?? string.Empty,
                                   description: description?.ToString(),
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

        //result.RegularSchedules = new List<OpenReferralRegularScheduleDto>()
        //{
        //    new OpenReferralRegularScheduleDto(
        //        id: Guid.NewGuid().ToString(),
        //        description: dtRow["Opening hours description"].ToString() ?? string.Empty,
        //        opens_at: null,
        //        closes_at: null,
        //        byday: null,
        //        bymonthday: null,
        //        dtstart: null,
        //        freq: null,
        //        interval: null,
        //        valid_from: null,
        //        valid_to: null)
        //};

        return result;

    }

    private List<OpenReferralContactDto> GetContacts(DataRow dtRow, OpenReferralServiceDto? service)
    {
        string contactId = Guid.NewGuid().ToString();
        string phoneNumberId = Guid.NewGuid().ToString();
        string textNumberId = Guid.NewGuid().ToString();

        if (service != null && service.Contacts != null)
        {
            var contact = service.Contacts?.FirstOrDefault(x => x.Name == "Telephone");
            if (contact != null)
            {
                contactId = contact.Id;
                var phone = contact.Phones?.FirstOrDefault();
                if (phone != null)
                {
                    phoneNumberId = phone.Id;
                }
            }

            contact = service.Contacts?.FirstOrDefault(x => x.Name == "Textphone");
            if (contact != null)
            {
                contactId = contact.Id;
                var phone = contact.Phones?.FirstOrDefault();
                if (phone != null)
                {
                    textNumberId = phone.Id;
                }
            }
        }

        var openReferralContacts  = new List<OpenReferralContactDto>();

        if (dtRow["Contact phone"] is not null && !string.IsNullOrEmpty(dtRow["Contact phone"].ToString()))
        {
            openReferralContacts.Add(new OpenReferralContactDto(
                contactId,
                "",
                "Telephone",
                new List<OpenReferralPhoneDto>()
                {
                    new OpenReferralPhoneDto(phoneNumberId, dtRow["Contact phone"]?.ToString() ?? string.Empty)
                }
                )

                );
        }

        if (dtRow["Contact sms"] is not null && !string.IsNullOrEmpty(dtRow["Contact sms"].ToString()))
        {
            openReferralContacts.Add(new OpenReferralContactDto(
                textNumberId,
                "",
                "Textphone",
                new List<OpenReferralPhoneDto>()
                {
                    new OpenReferralPhoneDto(textNumberId, dtRow["Contact sms"]?.ToString() ?? string.Empty)
                }
                )

                );
        }

        return openReferralContacts;
    }

    private List<OpenReferralEligibilityDto> GetEligibilities(DataRow dtRow, OpenReferralServiceDto? service)
    {
        string eligabilityId = Guid.NewGuid().ToString();
        if (service != null && service.Eligibilities != null)
        {
            var eligibileItem = service.Eligibilities?.FirstOrDefault(x => x.Eligibility == "Child" || x.Eligibility == "Adult");
            if (eligibileItem != null)
            {
                eligabilityId = eligibileItem.Id;
            }
        }

        if (!int.TryParse(dtRow["Age from"].ToString(), out int minage))
        {
            minage = 0;
        }

        if (!int.TryParse(dtRow["Age to"].ToString(), out int maxage))
        {
            maxage = 127;
        }

        string eligibilty = "Child";
        if (minage >= 18)
        {
            eligibilty = "Adult";
        }

        List<OpenReferralEligibilityDto> list = new()
        {
            new OpenReferralEligibilityDto(eligabilityId, eligibilty, maxage, minage)
        };

        return list;
    }

    private List<OpenReferralServiceTaxonomyDto> GetTaxonomies(DataRow dtRow)
    {
        List<OpenReferralServiceTaxonomyDto> list = new();
        var categories = dtRow["Sub-category"].ToString();
        if (!string.IsNullOrEmpty(categories))
        {
            string[] parts = categories.Split('|');
            foreach (string part in parts)
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
        List<OpenReferralLanguageDto> list = new();
        var languages = dtRow["Language"].ToString();
        if (!string.IsNullOrEmpty(languages))
        {
            string[] parts = languages.Split('|');
            foreach (string part in parts)
            {
                string languageId = Guid.NewGuid().ToString();
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
        List<OpenReferralCostOptionDto> list = new();
        if (string.IsNullOrEmpty(dtRow["Cost (£ in pounds)"]?.ToString()) &&
            string.IsNullOrEmpty(dtRow["Cost per"]?.ToString()) &&
            string.IsNullOrEmpty(dtRow["Cost Description"]?.ToString()))
        {
            return list;
        }

        if (!decimal.TryParse(dtRow["Cost (£ in pounds)"].ToString(), out decimal ammount))
        {
            ammount = 0.0M;
        }

        var costId = Guid.NewGuid().ToString();
        if (service != null && service.Cost_options != null)
        {
            var costoption = service.Cost_options.FirstOrDefault();
            if (costoption != null)
            {
                costId = costoption.Id;
            }
        }

        list.Add(new OpenReferralCostOptionDto(
                            costId,
                            amount_description: dtRow["Cost per"].ToString() ?? string.Empty,
                            amount: ammount,
                            linkId: null,
                            option: dtRow["Cost Description"]?.ToString(),
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
        string id = Guid.NewGuid().ToString();
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
        string[] parts = rowDeliveryTypes.Split('|');
        foreach (string part in parts)
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

    private async Task<List<OpenReferralServiceAtLocationDto>> GetLocationDto(int rownumber, DataRow dtRow, OpenReferralServiceDto? service)
    {
        string postcode = dtRow["Postcode"]?.ToString() ?? string.Empty;
        if (string.IsNullOrEmpty(postcode))
        {
            _errors.Add($"Postcode missing row: {rownumber}");
            return new List<OpenReferralServiceAtLocationDto>();
        }
        PostcodeApiModel postcodeApiModel;

        try
        {
            if (dicPostCodes.ContainsKey(postcode))
            {
                postcodeApiModel = dicPostCodes[postcode];
            }
            else
            {
                postcodeApiModel = await _postcodeLocationClientService.LookupPostcode(dtRow["Postcode"].ToString() ?? string.Empty);
                dicPostCodes[postcode] = postcodeApiModel;
            }

        }
        catch
        {
            _errors.Add($"Failed to find postcode: {postcode} row: {rownumber}");
            return new List<OpenReferralServiceAtLocationDto>();
        }

        string serviceAtLocationId = Guid.NewGuid().ToString();
        string locationId = Guid.NewGuid().ToString();
        string addressId = Guid.NewGuid().ToString();
        string regularScheduleId = Guid.NewGuid().ToString();
        string linkTaxonomyId = Guid.NewGuid().ToString();
        if (service != null && service.Service_at_locations != null)
        {
            var serviceAtLocation = service.Service_at_locations.FirstOrDefault(x =>
                x.Location.Name == dtRow["Location name"].ToString() &&
                x.Location?.Physical_addresses?.FirstOrDefault(l => l.Postal_code == dtRow["Postcode"].ToString()) != null);
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

                if (serviceAtLocation.Location.LinkTaxonomies != null && serviceAtLocation.Location.LinkTaxonomies.Count > 0)
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

        string addressLines = dtRow["Address line 1"]?.ToString() ?? string.Empty;
        if (dtRow["Address line 2"] != null && !string.IsNullOrEmpty(dtRow["Address line 2"].ToString()))
        {
            addressLines += " | " + dtRow["Address line 2"].ToString();
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



        return new List<OpenReferralServiceAtLocationDto>()
        {
            new OpenReferralServiceAtLocationDto(
                serviceAtLocationId,
                new OpenReferralLocationDto(
                    locationId,
                    dtRow["Location name"].ToString() ?? string.Empty,
                    dtRow["Location description"].ToString(),
                    postcodeApiModel.result.latitude,
                    postcodeApiModel.result.longitude,
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
                    },linkTaxonomyList
                ),
                new List<OpenReferralRegularScheduleDto>()
                {
                    new OpenReferralRegularScheduleDto(
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
                        valid_to: null)
                },
                new List<OpenReferralHolidayScheduleDto>()
                )
        };
    }


    private async Task<OpenReferralOrganisationDto?> GetOrganisationsWithOutServices(string organisationName)
    {
        if (_organisations == null || !_organisations.Any() || _organisations.Count(x => x.Name == organisationName) == 0)
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
        if (organisationWithServices == null)
        {
            organisationWithServices = await _openReferralOrganisationAdminClientService.GetOpenReferralOrganisationById(organisation.Id);

            _organisationsWithServices.Add(organisationWithServices);
        }

        organisationWithServices.AdministractiveDistrictCode = organisation.AdministractiveDistrictCode;

        organisationWithServices.AdministractiveDistrictCode = organisation.AdministractiveDistrictCode;

        return organisationWithServices;
    }

}
