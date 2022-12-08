﻿using FamilyHubs.ServiceDirectory.Shared.Builders;
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
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServiceAtLocations;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServiceDeliverysEx;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServices;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServiceTaxonomys;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralTaxonomys;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OrganisationType;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.ServiceType;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using FamilyHubs.SharedKernel;
using System.Data;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Dataupload;


public interface IDatauploadService
{
    Task<List<string>> UploadToApi(string organisationId, string path);
}

public class DatauploadService : IDatauploadService
{
    private readonly IOpenReferralOrganisationAdminClientService _openReferralOrganisationAdminClientService;
    private readonly IPostcodeLocationClientService _postcodeLocationClientService;

    private List<OpenReferralOrganisationDto> _organisations  = new();
    private List<OpenReferralOrganisationWithServicesDto> _organisationsWithServices = new();
    private List<OpenReferralTaxonomyDto> _taxonomies = new();
    private List<string> _errors = new List<string>();
    private Dictionary<string, PostcodeApiModel> dicPostCodes = new Dictionary<string, PostcodeApiModel>();

    public DatauploadService(IOpenReferralOrganisationAdminClientService openReferralOrganisationAdminClientService, IPostcodeLocationClientService postcodeLocationClientService)
    {
        _openReferralOrganisationAdminClientService = openReferralOrganisationAdminClientService;
        _postcodeLocationClientService = postcodeLocationClientService;
    }

    public async Task<List<string>> UploadToApi(string organisationId, string path)
    {
        PaginatedList<OpenReferralTaxonomyDto>  taxonomies = await _openReferralOrganisationAdminClientService.GetTaxonomyList(1, 999999999);
        _taxonomies.AddRange(taxonomies.Items);
        DataTable dtExcelTable = ExcelReader.GetRequestsDataFromExcel(path);
        await ProcessRows(organisationId, dtExcelTable);
        return _errors;
    }

    private async Task ProcessRows(string organisationId, DataTable dtExcelTable)
    {
        int rowNumber = -1;
        foreach (DataRow dtRow in dtExcelTable.Rows) 
        {
            rowNumber++;
            var organisationType = dtRow["Organisation Type"].ToString();
            OrganisationTypeDto organisationTypeDto;
            string? organisationName = null;
            switch (dtRow["Organisation Type"].ToString())
            {
                case "Local Authority":
                    organisationTypeDto = new OrganisationTypeDto("1", "LA", "Local Authority");
                    organisationName = dtRow["Local authority"] != null ? dtRow["Local authority"].ToString() : string.Empty;
                    break;
                case "Voluntary and Community Sector":
                    organisationTypeDto = new OrganisationTypeDto("2", "VCFS", "Voluntary, Charitable, Faith Sector");
                    organisationName = dtRow["Name of organisation"] != null ? dtRow["Name of organisation"].ToString() : string.Empty;
                    break;
                case "Family Hub":
                    organisationTypeDto = new OrganisationTypeDto("3", "FamilyHub", "Family Hub");
                    organisationName = dtRow["Name of organisation"] != null ? dtRow["Name of organisation"].ToString() : string.Empty;
                    break;
                default:
                    organisationTypeDto = new OrganisationTypeDto("4", "Company", "Public / Private Company eg: Child Care Centre");
                    organisationName = dtRow["Name of organisation"] != null ? dtRow["Name of organisation"].ToString() : string.Empty;
                    break;
                
            }

            
            if (string.IsNullOrWhiteSpace(organisationName))
            {
                _errors.Add($"Name of organisation missing row:{rowNumber}");
                continue;
            }


            OpenReferralOrganisationWithServicesDto? openReferralOrganisationDto = await GetOrganisation(organisationName);
            if (openReferralOrganisationDto == null)
            {
                _errors.Add($"Failed to find organisation: {organisationName} row:{rowNumber}");
                continue;
            }

            bool isNewService = true;
            var service = openReferralOrganisationDto?.Services?.FirstOrDefault(x => x.Name == dtRow["Name of service"].ToString());
            if (service != null)
            {
                isNewService = false;
            }
            service = await GetServiceFromRow(rowNumber, dtRow, service, organisationTypeDto, openReferralOrganisationDto?.Id ?? string.Empty);

            if(isNewService) 
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

    private async Task<OpenReferralServiceDto?> GetServiceFromRow(int rownumber, DataRow dtRow, OpenReferralServiceDto? service, OrganisationTypeDto organisationTypeDto, string organisationId)
    {
        var description = dtRow["More Details (service description)"]?.ToString();

        List<OpenReferralServiceAtLocationDto> locations = await GetLocationDto(rownumber, dtRow, service);
        if (!locations.Any())
            return null;

        ServicesDtoBuilder builder = new ServicesDtoBuilder();
        var result = builder.WithMainProperties(id: service?.Id ?? Guid.NewGuid().ToString(),
                                   serviceType: GetServiceType(organisationTypeDto ?? new OrganisationTypeDto("2", "VCFS", "Voluntary, Charitable, Faith Sector")),
                                   organisationId: organisationId ?? string.Empty,
                                   name: dtRow["Name of service"].ToString() ?? string.Empty,
                                   description: description?.ToString(),
                                   accreditations: null,
                                   assured_date: null,
                                   attending_access: null,
                                   attending_type: null,
                                   deliverable_type: null,
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

        if (service != null && service.Contacts != null) 
        {
            var contact = service.Contacts?.FirstOrDefault();
            if (contact != null) 
            {
                contactId = contact.Id;
                var phone = contact.Phones?.FirstOrDefault();
                if (phone != null) 
                {
                    phoneNumberId = phone.Id;
                }
            }
        }

        return new List<OpenReferralContactDto>()
        {
            new OpenReferralContactDto(
                contactId,
                "",
                "",
                new List<OpenReferralPhoneDto>()
                {
                    new OpenReferralPhoneDto(phoneNumberId, dtRow["Contact phone"]?.ToString() ?? string.Empty)
                }
                )
        };
    }

    private List<OpenReferralEligibilityDto> GetEligibilities(DataRow dtRow, OpenReferralServiceDto? service)
    {
        string eligibilty = "Child";
        if (dtRow["Is this service for children and young people?"].ToString() != "Yes")
        {
            eligibilty = "Adult";
        }

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

        if (!int.TryParse(dtRow["Age from"].ToString(), out int maxage))
        {
            maxage = 25;
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
                var taxonomy = _taxonomies.FirstOrDefault(x => x.Name == part);
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
        var languages = dtRow["Cost"].ToString();
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
                
                list.Add(new OpenReferralLanguageDto(languageId, part));
            }
        }
        
        return list;
    }

    private List<OpenReferralCostOptionDto> GetCosts(DataRow dtRow, OpenReferralServiceDto? service)
    {
        List<OpenReferralCostOptionDto> list = new();
        if (string.Compare(dtRow["Cost"].ToString(),"Yes", StringComparison.OrdinalIgnoreCase) != 0)
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
                            amount_description: string.Empty, 
                            amount: ammount, 
                            linkId: null, 
                            option: dtRow["Cost per"].ToString(), 
                            valid_from: null,
                            valid_to: null
                            ));

        return list;
    }
    private ServiceTypeDto GetServiceType(OrganisationTypeDto organisationTypeDto)
    {
        switch(organisationTypeDto.Name)
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
        foreach(string part in parts) 
        {
            
            if (string.Compare(part, "In person", StringComparison.OrdinalIgnoreCase) == 0)
            {
                list.Add(new OpenReferralServiceDeliveryExDto(GetServiceDeliveryId(service, ServiceDelivery.InPerson), ServiceDelivery.InPerson));
            }
            else if (string.Compare(part, "online", StringComparison.OrdinalIgnoreCase) == 0)
            {
                list.Add(new OpenReferralServiceDeliveryExDto(GetServiceDeliveryId(service, ServiceDelivery.Online), ServiceDelivery.Online));
            }
            else if (string.Compare(part, "by telephone", StringComparison.OrdinalIgnoreCase) == 0)
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
        if (service != null && service.Service_at_locations != null)
        { 
            var serviceAtLocation = service.Service_at_locations.FirstOrDefault(x => x.Location?.Physical_addresses?.FirstOrDefault(x => x.Postal_code == dtRow["Postcode"].ToString()) != null);
            if (serviceAtLocation != null) 
            { 
                serviceAtLocationId = serviceAtLocation.Id;
                if (serviceAtLocation.Location != null)
                {
                    locationId= serviceAtLocation.Location.Id;
                    if (serviceAtLocation.Location.Physical_addresses != null)
                    {
                        var address = serviceAtLocation.Location.Physical_addresses.FirstOrDefault(x => x.Postal_code == dtRow["Postcode"].ToString());
                        if (address != null) 
                        {
                            addressId = address.Id;
                        }
                    }
                }

                if(serviceAtLocation.Regular_schedule!= null) 
                {
                    var regularSchedule = serviceAtLocation.Regular_schedule.FirstOrDefault();
                    if (regularSchedule != null) 
                    { 
                        regularScheduleId = regularSchedule.Id;
                    }
                }
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
                            dtRow["Address line 1"].ToString() + dtRow["Address line 2"].ToString(),
                            dtRow["Town or City"].ToString(),
                            dtRow["Postcode"].ToString() ?? string.Empty,
                            "England",
                            dtRow["County"].ToString()
                            )
                    }
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

    private async Task<OpenReferralOrganisationWithServicesDto?> GetOrganisation(string organisationName)
    {
        if (_organisations == null || !_organisations.Any())
        {
            _organisations = await _openReferralOrganisationAdminClientService.GetListOpenReferralOrganisations();
        }

        var organisation = _organisations.FirstOrDefault(x => organisationName.Contains(x.Name ?? string.Empty));
        if (organisation== null) 
        {
            return null;
        }

        var organisationWithServics = await _openReferralOrganisationAdminClientService.GetOpenReferralOrganisationById(organisation.Id);
        if (!_organisationsWithServices.Contains(organisationWithServics))
        {
            _organisationsWithServices.Add(organisationWithServics);
        }

        return organisationWithServics;
    }
}
