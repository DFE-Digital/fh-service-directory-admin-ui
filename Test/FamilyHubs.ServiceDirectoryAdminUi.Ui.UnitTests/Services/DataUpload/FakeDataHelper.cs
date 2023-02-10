using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.DataUpload;
using System.Collections.Generic;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.UnitTests.Services.DataUpload
{
    internal static class FakeDataHelper
    {
        internal const string EXISTING_ORGANISATION_ID = "6354b975-cc43-470a-862f-5b9ddc9ded70";
        internal const string EXISTING_LOCAL_AUTHORITY_NAME = "Test Existing Council";
        internal const string TO_BE_UPDATED_SERVICE_NAME = "Test Service To be Updated Name";
        internal const string TO_BE_CREATED_SERVICE_NAME = "Test Service To be Created Name";
        internal const string ADMIN_AREA_CODE_FOR_EXISTING_ORGANISATION = "E12345678";
        internal const string ADMIN_AREA_CODE_FOR_NEW_ORGANISATION = "E87654321";

        internal static OrganisationDto GetFakeExistingOrganisationDto()
        {
            return new OrganisationDto(
                EXISTING_ORGANISATION_ID, 
                new OrganisationTypeDto("1", "LA", "Local Authority"),
                EXISTING_LOCAL_AUTHORITY_NAME,
                "Test Description",
                null,
                null,
                null);
        }

        internal static OrganisationWithServicesDto GetOrganisationWithServicesDto(OrganisationDto organisationDto)
        {
            var services = new List<ServiceDto>{ GetExistingServiceDto() };

            return new OrganisationWithServicesDto(
                organisationDto.Id,
                organisationDto.OrganisationType,
                organisationDto.Name,
                organisationDto.Description,
                organisationDto.Logo,
                organisationDto.Uri,
                organisationDto.Url,
                services);
        }

        internal static List<DataUploadRow> GetTestDataTableToUpdateExistingOrganisation()
        {
            var dataTable = new List<DataUploadRow>();

            dataTable.Add(GetDataRowForUpdateService());
            dataTable.Add(GetDataRowForCreateService());

            return dataTable;
        }

        internal static DataUploadRow GetSampleRow()
        {
            var row = new DataUploadRow();

            row.ExcelRowId = 8;
            row.ServiceUniqueId = "123";
            row.LocalAuthority = EXISTING_LOCAL_AUTHORITY_NAME;
            row.OrganisationType = "Local Authority";
            row.NameOfOrganisation = string.Empty;
            row.NameOfService = "Sample Service";
            row.DeliveryMethod = "In person";
            row.LocationName = "Test Location Name";
            row.LocationDescription = string.Empty;
            row.AddressLineOne = "1 Address Street";
            row.AddressLineTwo = "AddressLineTwo";
            row.TownOrCity = "CityTest";
            row.County = string.Empty;
            row.Postcode = "T4 4ST";
            row.ContactEmail = string.Empty;
            row.ContactPhone = "0123 456 7890";
            row.Website = string.Empty;
            row.ContactSms = string.Empty;
            row.SubCategory = "Activities";
            row.CostInPounds = "115.00";
            row.CostPer = "Week";
            row.CostDescription = string.Empty;
            row.Language = string.Empty;
            row.AgeFrom = "18";
            row.AgeTo = "32";
            row.OpeningHoursDescription = "Monday to Friday 07:30 - 18:00";
            row.ServiceDescription = "More Details for Update Service";

            return row;

        }


        //  Data should update an existing service in an existing organisation
        private static DataUploadRow GetDataRowForUpdateService()
        {
            var row = new DataUploadRow();

            row.ExcelRowId = 6;
            row.ServiceUniqueId = "121";
            row.LocalAuthority = EXISTING_LOCAL_AUTHORITY_NAME;
            row.OrganisationType = "Local Authority";
            row.NameOfOrganisation = string.Empty;
            row.NameOfService = TO_BE_UPDATED_SERVICE_NAME;
            row.DeliveryMethod = "In person";
            row.LocationName = "Test Location Name";
            row.LocationDescription = string.Empty;
            row.AddressLineOne = "1 Address Street";
            row.AddressLineTwo = "AddressLineTwo";
            row.TownOrCity = "CityTest";
            row.County = string.Empty;
            row.Postcode = "T4 4ST";
            row.ContactEmail = string.Empty;
            row.ContactPhone = "0123 456 7890";
            row.Website = string.Empty;
            row.ContactSms = string.Empty;
            row.SubCategory = "Activities";
            row.CostInPounds = "115.00";
            row.CostPer = "Week";
            row.CostDescription = string.Empty;
            row.Language = string.Empty;
            row.AgeFrom = "18";
            row.AgeTo = "32";
            row.OpeningHoursDescription = "Monday to Friday 07:30 - 18:00";
            row.ServiceDescription = "More Details for Update Service";

            return row;

        }

        //  Data should create a new service in an existing organisation
        private static DataUploadRow GetDataRowForCreateService()
        {
            var row = new DataUploadRow();

            row.ExcelRowId = 7;
            row.ServiceUniqueId = "122";
            row.LocalAuthority = EXISTING_LOCAL_AUTHORITY_NAME;
            row.OrganisationType = "Local Authority";
            row.NameOfOrganisation = string.Empty;
            row.NameOfService = TO_BE_CREATED_SERVICE_NAME;
            row.DeliveryMethod = "online";
            row.LocationName = "Test Location Name Two";
            row.LocationDescription = string.Empty;
            row.AddressLineOne = "2 Address Street";
            row.AddressLineTwo = "AddressLineThree";
            row.TownOrCity = "TestCity";
            row.County = "TestCounty";
            row.Postcode = "T3 3ST";
            row.ContactEmail = "email@test.com";
            row.ContactPhone = "0123 456 7890";
            row.Website = "http://website.com";
            row.ContactSms = "0987 654 3210";
            row.SubCategory = "Activities";
            row.CostInPounds = "150.00";
            row.CostPer = "Month";
            row.CostDescription = "CostDescription";
            row.Language = "English";
            row.AgeFrom = "3";
            row.AgeTo = "11";
            row.OpeningHoursDescription = "Monday to Friday 07:30 - 18:00";
            row.ServiceDescription = "More Details for Create Service";

            return row;
        }

        private static ServiceDto GetExistingServiceDto() 
        {

            return new ServiceDto(
                "121",
                new ServiceTypeDto("2", "Family Experience", ""),
                EXISTING_ORGANISATION_ID,
                TO_BE_UPDATED_SERVICE_NAME, // name
                "description",              // description
                null,                       // accreditations
                null,                       // DateTime ? assuredDate
                null,                       // string ? attendingAccess
                null,                       // string ? attendingType
                null,                       // string ? deliverableType
                null,                       // string ? status,
                null,                       // string ? fees,
                false,                      // bool canFamilyChooseDeliveryLocation,
                null,                       // ICollection<ServiceDeliveryDto> ? serviceDeliveries,
                null,                       // ICollection<EligibilityDto> ? eligibilities,
                null,                       // ICollection<FundingDto> ? fundings,
                new List<CostOptionDto>(),  // cost Options
                null,                       // ICollection<LanguageDto> ? languages,
                null,                       // ICollection<ServiceAreaDto> ? serviceAreas,
                null,                       // ICollection<ServiceAtLocationDto> ? serviceAtLocations,
                null,                       // ICollection<ServiceTaxonomyDto> ? serviceTaxonomies,
                null,                       // ICollection<RegularScheduleDto> ? regularSchedules
                null,                       // ICollection<HolidayScheduleDto> ? holidaySchedules
                null                        //ICollection<LinkContactDto> ? linkContacts
            );
        }

    }
}
