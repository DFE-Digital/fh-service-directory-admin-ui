using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services.DataUpload;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.ServiceDirectory.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests.Services.DataUpload;

public class DataUploadServiceTests
{
    private readonly ILogger<DataUploadService> _mockLogger;
    private readonly BufferedSingleFileUploadDb _fileUpload;
    private readonly Mock<IServiceDirectoryClient> _mockServiceDirectoryClient;
    private readonly Mock<IPostcodeLocationClientService> _mockPostcodeLocationClientService;
    private readonly OrganisationDto _existingOrganisation;

    public DataUploadServiceTests()
    {
        var formFile = new FormFile(new MemoryStream(), 0, 0, "Test", "Test");

        _mockLogger = Mock.Of<ILogger<DataUploadService>>();
        _existingOrganisation = FakeDataHelper.GetFakeExistingOrganisationDto();
        _fileUpload = new BufferedSingleFileUploadDb
        {
            FormFile = formFile
        };

        _mockServiceDirectoryClient = GetMockOrganisationAdminClientService();
        _mockPostcodeLocationClientService = GetMockPostcodeLocationClientService();
    }

    [Fact]
    public async Task UploadToApi_AllRowsInvalid_LocalAuthority_ReturnsExpectedErrors()
    {
        //  Arrange
        var dataTable = FakeDataHelper.GetTestDataTableToUpdateExistingOrganisation();

        foreach (var row in dataTable)
        {
            row.LocalAuthority = string.Empty;//Invalidate property
        }

        var mockExcelReader = GetMockExcelReader(dataTable);

        var sut = new DataUploadService(
            _mockLogger,
            _mockServiceDirectoryClient.Object,
            _mockPostcodeLocationClientService.Object,
            mockExcelReader.Object);

        //  Act
        var result = await sut.UploadToApi(_fileUpload);

        //  Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Failed to find local authority row:6", result[0]);
    }

    [Fact]
    public async Task UploadToApi_CannotResolveOrganisationName_ReturnsExpectedErrors()
    {
        //  Arrange
        var dataTable = FakeDataHelper.GetTestDataTableToUpdateExistingOrganisation();

        foreach (var row in dataTable)
        {
            row.OrganisationType = OrganisationType.VCFS;
            row.NameOfOrganisation = string.Empty; // Invalidate column in all rows
        }
        var mockExcelReader = GetMockExcelReader(dataTable);

        var sut = new DataUploadService(
            _mockLogger,
            _mockServiceDirectoryClient.Object,
            _mockPostcodeLocationClientService.Object,
            mockExcelReader.Object);

        //  Act
        var result = await sut.UploadToApi(_fileUpload);

        //  Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Name of organisation missing for ServiceOwnerReferenceId:121", result[0]);
    }

    [Fact]
    public async Task UploadToApi_To_UpdateExistingOrganisation_CreateServiceCalled()
    {
        //  Arrange
        var dataTable = FakeDataHelper.GetTestDataTableToUpdateExistingOrganisation();
        var mockExcelReader = GetMockExcelReader(dataTable);
        ServiceDto actualServiceDto = null!;
        _mockServiceDirectoryClient.Setup(m => m.CreateService(It.IsAny<ServiceDto>())).Callback((ServiceDto p) => actualServiceDto = p);

        var sut = new DataUploadService(
            _mockLogger,
            _mockServiceDirectoryClient.Object,
            _mockPostcodeLocationClientService.Object,
            mockExcelReader.Object);

        //  Act
        await sut.UploadToApi(_fileUpload);

        //  Assert
        Assert.NotNull(actualServiceDto); // Fails if Create Service is never called
        Assert.Equal(ServiceType.FamilyExperience, actualServiceDto.ServiceType);
        Assert.Equal(FakeDataHelper.ExistingOrganisationId, actualServiceDto.OrganisationId);
        Assert.Equal(FakeDataHelper.ToBeCreatedServiceName, actualServiceDto.Name);
        Assert.Equal("More Details for Create Service", actualServiceDto.Description);
        Assert.Equal(ServiceDeliveryType.Online, actualServiceDto.ServiceDeliveries.First().Name);
        AssertAddress(actualServiceDto, "2 Address Street", "AddressLineThree", "TestCity", "T3 3ST", "TestCounty");
        Assert.Equal(ServiceStatusType.Active, actualServiceDto.Status);
        AssertContact(actualServiceDto.Contacts, "0123 456 7890", "0987 654 3210", "http://website.com", "email@test.com"); // Because Online contact on service object
        AssertCostOptions(actualServiceDto, "CostDescription", 150, "Month" );
        Assert.Equal("English", actualServiceDto.Languages.First().Name);
        AssertTaxonomy(actualServiceDto, "Activities");
        AssertEligibilities(actualServiceDto, EligibilityType.Child, 11, 3);
    }

    [Fact]
    public async Task UploadToApi_To_UpdateExistingOrganisation_UpdateServiceCalled()
    {
        //  Arrange
        var dataTable = FakeDataHelper.GetTestDataTableToUpdateExistingOrganisation();
        var mockExcelReader = GetMockExcelReader(dataTable);
        ServiceDto actualServiceDto = null!;
        _mockServiceDirectoryClient.Setup(m => m.UpdateService(It.IsAny<ServiceDto>())).Callback((ServiceDto p) => actualServiceDto = p);


        var sut = new DataUploadService(
            _mockLogger,
            _mockServiceDirectoryClient.Object,
            _mockPostcodeLocationClientService.Object,
            mockExcelReader.Object);

        //  Act
        await sut.UploadToApi(_fileUpload);

        //  Assert
        Assert.NotNull(actualServiceDto); // Fails if Update Service is never called
        Assert.Equal(ServiceType.FamilyExperience, actualServiceDto.ServiceType);
        Assert.Equal(FakeDataHelper.ExistingOrganisationId, actualServiceDto.OrganisationId);
        Assert.Equal(FakeDataHelper.ToBeUpdatedServiceName, actualServiceDto.Name);
        Assert.Equal("More Details for Update Service", actualServiceDto.Description);
        Assert.Equal(ServiceDeliveryType.InPerson, actualServiceDto.ServiceDeliveries.First().Name);
        AssertAddress(actualServiceDto, "1 Address Street", "AddressLineTwo", "CityTest", "T4 4ST", "County");
        Assert.Equal(ServiceStatusType.Active, actualServiceDto.Status);
        AssertContact(actualServiceDto.Locations.First().Contacts, "0123 456 7890", string.Empty, string.Empty, string.Empty); // Because InPerson contact on Locations object
        AssertCostOptions(actualServiceDto, string.Empty, 115, "Week");
        AssertTaxonomy(actualServiceDto, "Activities");
        AssertEligibilities(actualServiceDto, EligibilityType.Adult, 32, 18);
    }

    //  This test checks that multiple rows in the spreadsheet are for the same service, then the locations are added Not overwritten
    //  In this test, the simulated excel table will contain 10 rows for the same service, but with 5 unique locations
    [Fact]
    public async Task UploadToApi_MultipleLocationsAddedToSameService()
    {
        //  Arrange
        var dataTable = new List<DataUploadRowDto>();

        for (var i = 0; i < 5; i++)
        {
            var newLocationRow = FakeDataHelper.GetSampleRow();
            newLocationRow.LocationName = $"Location:{i}";
            dataTable.Add(newLocationRow);

            var duplicateLocationRow = FakeDataHelper.GetSampleRow();
            duplicateLocationRow.LocationName = $"Location:{i}";
            dataTable.Add(duplicateLocationRow);
        }

        var mockExcelReader = GetMockExcelReader(dataTable);
        ServiceDto actualServiceDto = null!;
        _mockServiceDirectoryClient.Setup(m => m.CreateService(It.IsAny<ServiceDto>())).Callback((ServiceDto p) => actualServiceDto = p);

        var sut = new DataUploadService(
            _mockLogger,
            _mockServiceDirectoryClient.Object,
            _mockPostcodeLocationClientService.Object,
            mockExcelReader.Object);

        //  Act
        await sut.UploadToApi(_fileUpload);

        //  Assert
        Assert.NotNull(actualServiceDto);

        Assert.Equal(5, actualServiceDto.Locations.Count);

        for (var i = 0; i < 5; i++)
        {
            Assert.True(actualServiceDto.Locations?.Where(x => x.Name == $"Location:{i}").Any());
        }

    }

    private Mock<IServiceDirectoryClient> GetMockOrganisationAdminClientService()
    {
        var mock = new Mock<IServiceDirectoryClient>();

        var taxonomyResult = Task.FromResult(GetTestTaxonomies());
        mock.Setup(m => m.GetTaxonomyList(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<TaxonomyType>())).Returns(taxonomyResult);

        var organisationsResult = Task.FromResult(new List<OrganisationDto> { _existingOrganisation });
        mock.Setup(m => m.GetOrganisations(CancellationToken.None)).Returns(organisationsResult);

        var organisationWithServicesResult = FakeDataHelper.GetOrganisationWithServicesDto(_existingOrganisation);
        mock.Setup(m => m.GetOrganisationById(FakeDataHelper.ExistingOrganisationId)).Returns(Task.FromResult(organisationWithServicesResult)!);

        mock.Setup(m => m.CreateService(It.IsAny<ServiceDto>())).Callback((ServiceDto service) =>
        {
            organisationWithServicesResult.Services.Add(service);
        });

        mock.Setup(m => m.UpdateService(It.IsAny<ServiceDto>())).Callback((ServiceDto service) =>
        {
            var serviceList = (List<ServiceDto>)organisationWithServicesResult.Services;
            var existingService = serviceList.First(i => i.Id == service.Id);
            var index = serviceList.IndexOf(existingService);

            if (index != -1)
            {
                serviceList[index] = service;
            }
        });

        return mock;
    }

    private static Mock<IExcelReader> GetMockExcelReader(List<DataUploadRowDto> dataTable)
    {
        var mock = new Mock<IExcelReader>();

        var dataTableResult = Task.FromResult(dataTable);
        mock.Setup(m => m.GetRequestsDataFromExcel(It.IsAny<BufferedSingleFileUploadDb>())).Returns(dataTableResult);

        return mock;
    }

    private static Mock<IPostcodeLocationClientService> GetMockPostcodeLocationClientService()
    {
        var mock = new Mock<IPostcodeLocationClientService>();

        var postcodeResponseForExistingOrganisation = new PostcodesIoResponse
        {
            Result = new PostcodeInfo
            {
                Codes = new Codes
                {
                    AdminCounty = FakeDataHelper.AdminAreaCodeForExistingOrganisation
                },
                Latitude = 50,
                Longitude = 50,
                Postcode = "T3 3ST"
            }
        };

        var postcodeResponseForNewOrganisation = new PostcodesIoResponse
        {
            Result = new PostcodeInfo
            {
                Codes = new Codes
                {
                    AdminCounty = FakeDataHelper.AdminAreaCodeForNewOrganisation
                },
                Latitude = 60,
                Longitude = 60,
                Postcode = "T4 4ST"
            }
        };

        mock.Setup(m => m.LookupPostcode("T3 3ST")).Returns(Task.FromResult(postcodeResponseForExistingOrganisation));
        mock.Setup(m => m.LookupPostcode("T4 4ST")).Returns(Task.FromResult(postcodeResponseForNewOrganisation));

        return mock;
    }

    private static PaginatedList<TaxonomyDto> GetTestTaxonomies()
    {
        var list = new List<TaxonomyDto>
        {

            new TaxonomyDto
            {
                Name = "Parent",
                Id = 1,
                TaxonomyType = TaxonomyType.ServiceCategory
            },

            new TaxonomyDto
            {
                Name = "Activities",
                ParentId = 1,
                Id = 2,
                TaxonomyType = TaxonomyType.ServiceCategory
            },

            new TaxonomyDto
            {
                Name = "Holiday clubs and schemes",
                ParentId = 1,
                Id = 3,
                TaxonomyType = TaxonomyType.ServiceCategory
            }
        };
        return new PaginatedList<TaxonomyDto>(list, list.Count, 1, list.Count);
    }

    private static void AssertAddress(ServiceDto service, string address1, string address2, string city, string postcode, string county)
    {
        var location = service.Locations.First();

        Assert.Equal(address1, location.Address1);
        Assert.Equal(address2, location.Address2);
        Assert.Equal(city, location.City);
        Assert.Equal(postcode, location.PostCode);
        Assert.Equal("England", location.Country);
        Assert.Equal(county, location.StateProvince);

    }

    private static void AssertContact(ICollection<ContactDto>? contacts, string telephone, string sms, string url, string email)
    {
        Assert.NotNull(contacts);
        var contact = contacts.First();

        Assert.Equal(telephone, contact.Telephone);
        Assert.Equal(sms, contact.TextPhone);
        Assert.Equal(url, contact.Url);
        Assert.Equal(email, contact.Email);
    }

    private static void AssertCostOptions(ServiceDto service, string description, decimal amount, string option)
    {
        var costOption = service.CostOptions.First();

        Assert.NotNull(costOption);
        Assert.Equal(description, costOption.AmountDescription);
        Assert.Equal(amount, costOption.Amount);
        Assert.Equal(option, costOption.Option);
    }

    private static void AssertTaxonomy(ServiceDto service, string expectedTaxonomyName)
    {
        var taxonomy = GetTestTaxonomies().Items.First(x => x.Name == expectedTaxonomyName);
        if (taxonomy is null)
        {
            throw new ArgumentException($"{expectedTaxonomyName} is not valid for the test");
        }

        var actualTaxonomy = service.Taxonomies.First();

        Assert.NotNull(actualTaxonomy);
        Assert.Equal(taxonomy, actualTaxonomy);
    }

    private static void AssertEligibilities(ServiceDto service, EligibilityType eligibilityType, int max, int min)
    {
        var eligibilities = service.Eligibilities.First();

        Assert.NotNull(eligibilities);
        Assert.Equal(eligibilityType, eligibilities.EligibilityType);
        Assert.Equal(max, eligibilities.MaximumAge);
        Assert.Equal(min, eligibilities.MinimumAge);
    }
}