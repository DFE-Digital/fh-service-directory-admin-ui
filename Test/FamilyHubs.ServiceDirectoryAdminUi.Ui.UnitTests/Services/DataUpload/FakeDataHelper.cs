using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.DataUpload;
using System.Collections.Generic;
using System.Data;

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


        internal static DataTable GetTestDataTableToUpdateExistingOrganisation()
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add(string.Empty);
            dataTable.Columns.Add(ColumnHeaders.SERVICE_UNIQUE_IDENTIFIER);
            dataTable.Columns.Add(ColumnHeaders.LOCAL_AUTHORITY);
            dataTable.Columns.Add(ColumnHeaders.ORGANISATION_TYPE);
            dataTable.Columns.Add(ColumnHeaders.NAME_OF_ORGANISATION);
            dataTable.Columns.Add(ColumnHeaders.NAME_OF_SERVICE);
            dataTable.Columns.Add(ColumnHeaders.DELIVERY_METHOD);
            dataTable.Columns.Add(ColumnHeaders.LOCATION_NAME);
            dataTable.Columns.Add(ColumnHeaders.LOCATION_DESCRIPTION);
            dataTable.Columns.Add(ColumnHeaders.ADDRESS_LINE_ONE);
            dataTable.Columns.Add(ColumnHeaders.ADDRESS_LINE_TWO);
            dataTable.Columns.Add(ColumnHeaders.TOWN_OR_CITY);
            dataTable.Columns.Add(ColumnHeaders.COUNTY);
            dataTable.Columns.Add(ColumnHeaders.POSTCODE);
            dataTable.Columns.Add(ColumnHeaders.CONTACT_EMAIL);
            dataTable.Columns.Add(ColumnHeaders.CONTACT_PHONE);
            dataTable.Columns.Add(ColumnHeaders.WEBSITE);
            dataTable.Columns.Add(ColumnHeaders.CONTACT_SMS);
            dataTable.Columns.Add(ColumnHeaders.SUB_CATEGORY);
            dataTable.Columns.Add(ColumnHeaders.COST_IN_POUNDS);
            dataTable.Columns.Add(ColumnHeaders.COST_PER);
            dataTable.Columns.Add(ColumnHeaders.COST_DESCRIPTION);
            dataTable.Columns.Add(ColumnHeaders.LANGUAGE);
            dataTable.Columns.Add(ColumnHeaders.AGE_FROM);
            dataTable.Columns.Add(ColumnHeaders.AGE_TO);
            dataTable.Columns.Add(ColumnHeaders.OPENING_HOURS_DESCRIPTION);
            dataTable.Columns.Add(ColumnHeaders.MORE_DETAILS_SERVICE_DESCRIPTION);

            AddDataRowToTable(dataTable, GetTestRowOne());
            AddDataRowToTable(dataTable, GetTestRowTwo());

            return dataTable;
        }

        //  Data should update an existing service in an existing organisation
        private static string[] GetTestRowOne()
        {
            return new string[]
            {
                string.Empty,                       // Empty Row
                "121",                              // Id
                EXISTING_LOCAL_AUTHORITY_NAME,      // Local Authority
                "Local Authority",                  // Organisation Type
                string.Empty,                       // Name of Organisation
                TO_BE_UPDATED_SERVICE_NAME,         // Name of Service
                "In person",                        // Delivery Method
                "Test Location Name",               // Location Name
                string.Empty,                       // Location Description
                "1 Address Street",                 // Address 1
                "AddressLineTwo",                   // Address 2
                "CityTest",                         // City
                string.Empty,                       // County
                "T4 4ST",                           // Postcode
                string.Empty,                       // Email
                "0123 456 7890",                    // Phone
                string.Empty,                       // Website
                string.Empty,                       // SMS
                "Activities",                       // SubCategory
                "115.00",                           // Cost
                "Week",                             // Cost Per
                string.Empty,                       // Cost Description
                string.Empty,                       // Langauge
                "18",                               // Age From
                "32",                               // Age To
                "Monday to Friday 07:30 - 18:00",   // Open Hours
                "More Details for Update Service"   // More Details
            };
        }

        //  Data should create a new service in an existing organisation
        private static string[] GetTestRowTwo()
        {
            return new string[]
            {
                string.Empty,                       // Empty Row
                "122",                              // Id
                EXISTING_LOCAL_AUTHORITY_NAME,      // Local Authority
                "Local Authority",                  // Organisation Type
                string.Empty,                       // Name of Organisation
                TO_BE_CREATED_SERVICE_NAME,         // Name of Service
                "online",                           // Delivery Method
                "Test Location Name",               // Location Name
                string.Empty,                       // Location Description
                "2 Address Street",                 // Address 1
                "AddressLineThree",                 // Address 2
                "TestCity",                         // City
                "TestCounty",                       // County
                "T3 3ST",                           // Postcode
                "email@test.com",                   // Email
                "0123 456 7890",                    // Phone
                "http://website.com",               // Website
                "0987 654 3210",                    // SMS
                "Activities",                       // SubCategory
                "150.00",                           // Cost
                "Month",                            // Cost Per
                "CostDescription",                  // Cost Description
                "English",                          // Langauge
                "3",                                // Age From
                "11",                               // Age To
                "Monday to Friday 07:30 - 18:00",   // Open Hours
                "More Details For Create Service"   // More Details
            };
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

        private static void AddDataRowToTable(DataTable dataTable, string[] cells)
        {
            var dataRow = dataTable.NewRow();

            for(int i = 0; i< cells.Length; i++)
            {
                dataRow[i] = cells[i];
            }

            dataTable.Rows.Add(dataRow);
        }
    }
}
