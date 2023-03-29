using System.Collections.Generic;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests.Services.DataUpload;

internal static class FakeDataHelper
{
    internal const long ExistingOrganisationId = 1;
    private const string ExistingLocalAuthorityName = "Test Existing Council";
    internal const string ToBeUpdatedServiceName = "Test Service To be Updated Name";
    internal const string ToBeCreatedServiceName = "Test Service To be Created Name";
    internal const string AdminAreaCodeForExistingOrganisation = "E12345678";
    internal const string AdminAreaCodeForNewOrganisation = "E87654321";

    internal static OrganisationDto GetFakeExistingOrganisationDto()
    {
        var dto = new OrganisationDto
        {
            AdminAreaCode = AdminAreaCodeForExistingOrganisation,
            Name = ExistingLocalAuthorityName,
            Description = "Test Description",
            OrganisationType = OrganisationType.LA,
            Id = ExistingOrganisationId
        };
        return dto;
    }

    internal static OrganisationWithServicesDto GetOrganisationWithServicesDto(OrganisationDto organisationDto)
    {
        var services = new List<ServiceDto> { GetExistingServiceDto() };

        var dto = new OrganisationWithServicesDto
        {
            Id = organisationDto.Id,
            OrganisationType = organisationDto.OrganisationType,
            Name = organisationDto.Name,
            Description = organisationDto.Description,
            Logo = organisationDto.Logo,
            Uri = organisationDto.Uri,
            Url = organisationDto.Url,
            AdminAreaCode = organisationDto.AdminAreaCode,
            Services = services
        };
        return dto;
    }

    internal static List<DataUploadRowDto> GetTestDataTableToUpdateExistingOrganisation()
    {
        var dataTable = new List<DataUploadRowDto>
        {
            GetDataRowForUpdateService(),
            GetDataRowForCreateService()
        };

        return dataTable;
    }

    internal static DataUploadRowDto GetSampleRow()
    {
        var row = new DataUploadRowDto
        {
            ExcelRowId = 8,
            ServiceOwnerReferenceId = "123",
            LocalAuthority = ExistingLocalAuthorityName,
            OrganisationType = OrganisationType.LA,
            NameOfOrganisation = string.Empty,
            NameOfService = "Sample Service",
            DeliveryMethod = ServiceDeliveryType.InPerson,
            LocationName = "Test Location Name",
            LocationDescription = string.Empty,
            AddressLineOne = "1 Address Street",
            AddressLineTwo = "AddressLineTwo",
            TownOrCity = "CityTest",
            County = "Test",
            Postcode = "T4 4ST",
            ContactEmail = string.Empty,
            ContactPhone = "0123 456 7890",
            Website = string.Empty,
            ContactSms = string.Empty,
            SubCategory = "Activities",
            CostInPounds = "115.00",
            CostPer = "Week",
            CostDescription = string.Empty,
            Language = string.Empty,
            AgeFrom = "18",
            AgeTo = "32",
            OpeningHoursDescription = "Monday to Friday 07:30 - 18:00",
            ServiceDescription = "More Details for Update Service"
        };

        return row;

    }

    //  Data should update an existing service in an existing organisation
    private static DataUploadRowDto GetDataRowForUpdateService()
    {
        var row = new DataUploadRowDto
        {
            ExcelRowId = 6,
            ServiceOwnerReferenceId = "121",
            LocalAuthority = ExistingLocalAuthorityName,
            OrganisationType = OrganisationType.LA,
            NameOfOrganisation = string.Empty,
            NameOfService = ToBeUpdatedServiceName,
            DeliveryMethod = ServiceDeliveryType.InPerson,
            LocationName = "Test Location Name",
            LocationDescription = string.Empty,
            AddressLineOne = "1 Address Street",
            AddressLineTwo = "AddressLineTwo",
            TownOrCity = "CityTest",
            County = "County",
            Postcode = "T4 4ST",
            ContactEmail = string.Empty,
            ContactPhone = "0123 456 7890",
            Website = string.Empty,
            ContactSms = string.Empty,
            SubCategory = "Activities",
            CostInPounds = "115.00",
            CostPer = "Week",
            CostDescription = string.Empty,
            Language = string.Empty,
            AgeFrom = "18",
            AgeTo = "32",
            OpeningHoursDescription = "Monday to Friday 07:30 - 18:00",
            ServiceDescription = "More Details for Update Service"
        };

        return row;

    }

    //  Data should create a new service in an existing organisation
    private static DataUploadRowDto GetDataRowForCreateService()
    {
        var row = new DataUploadRowDto
        {
            ExcelRowId = 7,
            ServiceOwnerReferenceId = "122",
            LocalAuthority = ExistingLocalAuthorityName,
            OrganisationType = OrganisationType.LA,
            NameOfOrganisation = string.Empty,
            NameOfService = ToBeCreatedServiceName,
            DeliveryMethod = ServiceDeliveryType.Online,
            LocationName = "Test Location Name Two",
            LocationDescription = string.Empty,
            AddressLineOne = "2 Address Street",
            AddressLineTwo = "AddressLineThree",
            TownOrCity = "TestCity",
            County = "TestCounty",
            Postcode = "T3 3ST",
            ContactEmail = "email@test.com",
            ContactPhone = "0123 456 7890",
            Website = "http://website.com",
            ContactSms = "0987 654 3210",
            SubCategory = "Activities",
            CostInPounds = "150.00",
            CostPer = "Month",
            CostDescription = "CostDescription",
            Language = "English",
            AgeFrom = "3",
            AgeTo = "11",
            OpeningHoursDescription = "Monday to Friday 07:30 - 18:00",
            ServiceDescription = "More Details for Create Service"
        };

        return row;
    }

    private static ServiceDto GetExistingServiceDto()
    {
        var dto = new ServiceDto
        {
            Name = ToBeUpdatedServiceName,
            ServiceType = ServiceType.FamilyExperience,
            ServiceOwnerReferenceId = "12345678121",
            Id = 121,
            Description = "description"
        };
        return dto;
    }
}