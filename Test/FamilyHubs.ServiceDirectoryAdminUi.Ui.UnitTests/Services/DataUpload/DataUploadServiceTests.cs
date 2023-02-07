using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.DataUpload;
using FamilyHubs.SharedKernel;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.UnitTests.Services.DataUpload
{
    public class DataUploadServiceTests
    {
        private BufferedSingleFileUploadDb _fileUpload;
        private Mock<IOrganisationAdminClientService> _mockOrganisationAdminClientService;
        private Mock<IPostcodeLocationClientService> _mockPostcodeLocationClientService;
        private OrganisationDto _existingOrganisation;

        public DataUploadServiceTests() 
        {
            _existingOrganisation = FakeDataHelper.GetFakeExistingOrganisationDto();
            _fileUpload = new BufferedSingleFileUploadDb();

            _mockOrganisationAdminClientService = GetMockOrganisationAdminClientService();
            _mockPostcodeLocationClientService = GetMockPostcodeLocationClientService(); 
        }

        [Fact]
        public async Task UploadToApi_AllRowsInvalid_LocalAuthority_ReturnsExpectedErrors()
        {
            //  Arrange
            var dataTable = FakeDataHelper.GetTestDataTableToUpdateExistingOrganisation();
            
            foreach (var row in dataTable)
            {
                row.Value.LocalAuthority = string.Empty;//Invalidate property
            }

            var mockExcelReader = GetMockExcelReader(dataTable);

            var sut = new DataUploadService(
                _mockOrganisationAdminClientService.Object,
                _mockPostcodeLocationClientService.Object,
                mockExcelReader.Object);

            //  Act
            var result = await sut.UploadToApi(FakeDataHelper.EXISTING_ORGANISATION_ID, _fileUpload);

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
                row.Value.OrganisationType = "voluntary and community sector";
                row.Value.NameOfOrganisation = string.Empty; // Invalidate column in all rows
            }
            var mockExcelReader = GetMockExcelReader(dataTable);

            var sut = new DataUploadService(
                _mockOrganisationAdminClientService.Object,
                _mockPostcodeLocationClientService.Object,
                mockExcelReader.Object);

            //  Act
            var result = await sut.UploadToApi(FakeDataHelper.EXISTING_ORGANISATION_ID, _fileUpload);

            //  Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("Name of organisation missing row:6", result[0]);
        }

        [Fact]
        public async Task UploadToApi_To_UpdateExistingOrganisation_CreateServiceCalled()
        {
            //  Arrange
            var dataTable = FakeDataHelper.GetTestDataTableToUpdateExistingOrganisation();
            var mockExcelReader = GetMockExcelReader(dataTable);
            ServiceDto actualServiceDto = null!;
            _mockOrganisationAdminClientService.Setup(m => m.CreateService(It.IsAny<ServiceDto>())).Callback((ServiceDto p) => actualServiceDto = p);

            var sut = new DataUploadService(
                _mockOrganisationAdminClientService.Object,
                _mockPostcodeLocationClientService.Object,
                mockExcelReader.Object);

            //  Act
            await sut.UploadToApi(FakeDataHelper.EXISTING_ORGANISATION_ID, _fileUpload, false);

            //  Assert
            Assert.NotNull(actualServiceDto); // Fails if Create Service is never called
            Assert.Equal("Family Experience", actualServiceDto.ServiceType.Name);
            Assert.Equal(FakeDataHelper.EXISTING_ORGANISATION_ID, actualServiceDto.OrganisationId);
            Assert.Equal(FakeDataHelper.TO_BE_CREATED_SERVICE_NAME, actualServiceDto.Name);
            Assert.Equal("More Details for Create Service", actualServiceDto.Description);
            Assert.Equal(ServiceDeliveryType.Online, actualServiceDto.ServiceDeliveries?.First().Name);
            AssertAddress(actualServiceDto, "2 Address Street | AddressLineThree", "TestCity", "T3 3ST", "TestCounty");
            Assert.Equal("active", actualServiceDto.Status);
            AssertContact(actualServiceDto.LinkContacts, "0123 456 7890", "0987 654 3210", "http://website.com", "email@test.com"); // Because Online contact on service object
            AssertCostOptions(actualServiceDto, "Month", 150, "CostDescription");
            Assert.Equal("English", actualServiceDto.Languages?.First().Name);
            AssertTaxonomy(actualServiceDto, "Activities");
            AssertEligibilities(actualServiceDto, "Child", 11, 3);
        }

        [Fact]
        public async Task UploadToApi_To_UpdateExistingOrganisation_UpdateServiceCalled()
        {
            //  Arrange
            var dataTable = FakeDataHelper.GetTestDataTableToUpdateExistingOrganisation();
            var mockExcelReader = GetMockExcelReader(dataTable);
            ServiceDto actualServiceDto = null!;
            _mockOrganisationAdminClientService.Setup(m => m.UpdateService(It.IsAny<ServiceDto>())).Callback((ServiceDto p) => actualServiceDto = p);


            var sut = new DataUploadService(
                _mockOrganisationAdminClientService.Object,
                _mockPostcodeLocationClientService.Object,
                mockExcelReader.Object);

            //  Act
            await sut.UploadToApi(FakeDataHelper.EXISTING_ORGANISATION_ID, _fileUpload, false);

            //  Assert
            Assert.NotNull(actualServiceDto); // Fails if Update Service is never called
            Assert.Equal("Family Experience", actualServiceDto.ServiceType.Name);
            Assert.Equal(FakeDataHelper.EXISTING_ORGANISATION_ID, actualServiceDto.OrganisationId);
            Assert.Equal(FakeDataHelper.TO_BE_UPDATED_SERVICE_NAME, actualServiceDto.Name);
            Assert.Equal("More Details for Update Service", actualServiceDto.Description);
            Assert.Equal(ServiceDeliveryType.InPerson, actualServiceDto.ServiceDeliveries?.First().Name);
            AssertAddress(actualServiceDto, "1 Address Street | AddressLineTwo", "CityTest", "T4 4ST", string.Empty);
            Assert.Equal("active", actualServiceDto.Status);
            AssertContact(actualServiceDto.ServiceAtLocations?.First().LinkContacts, "0123 456 7890", string.Empty, string.Empty, string.Empty); // Because InPerson contact on ServiceAtLocations object
            AssertCostOptions(actualServiceDto, "Week", 115, string.Empty);
            AssertTaxonomy(actualServiceDto, "Activities");
            AssertEligibilities(actualServiceDto, "Adult", 32, 18);
        }

        private Mock<IOrganisationAdminClientService> GetMockOrganisationAdminClientService()
        {
            var mock = new Mock<IOrganisationAdminClientService>();

            var taxonomyResult = Task.FromResult(GetTestTaxonomies());
            mock.Setup(m => m.GetTaxonomyList(It.IsAny<int>(), It.IsAny<int>())).Returns(taxonomyResult);

            var organisationsResult = Task.FromResult(new List<OrganisationDto> { _existingOrganisation });
            mock.Setup(m => m.GetListOrganisations()).Returns(organisationsResult);

            var organisationWithServicesResult = Task.FromResult(FakeDataHelper.GetOrganisationWithServicesDto(_existingOrganisation));
            mock.Setup(m => m.GetOrganisationById(FakeDataHelper.EXISTING_ORGANISATION_ID)).Returns(organisationWithServicesResult);

            return mock;
        }

        private Mock<IExcelReader> GetMockExcelReader(Dictionary<int, DataUploadRow> dataTable)
        {
            var mock = new Mock<IExcelReader>();

            var dataTableResult = Task.FromResult(dataTable);
            mock.Setup(m => m.GetRequestsDataFromExcel(It.IsAny<BufferedSingleFileUploadDb>())).Returns(dataTableResult);

            return mock;
        }

        private Mock<IPostcodeLocationClientService> GetMockPostcodeLocationClientService()
        {
            var mock = new Mock<IPostcodeLocationClientService>();

            var postcodeResponseForExistingOrganisation = new PostcodesIoResponse
            {
                Result = new PostcodeInfo()
                {
                    Codes = new Codes { AdminCounty = FakeDataHelper.ADMIN_AREA_CODE_FOR_EXISTING_ORGANISATION },
                    Latitude = 50,
                    Longitude = 50,
                    Postcode = "T3 3ST"
                }
            }; 

            var postcodeResponseForNewOrganisation = new PostcodesIoResponse
            {
                Result = new PostcodeInfo()
                {
                    Codes = new Codes { AdminCounty = FakeDataHelper.ADMIN_AREA_CODE_FOR_NEW_ORGANISATION },
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
            List<TaxonomyDto> list = new()
            {
                new TaxonomyDto(
                            "TaxonomyGuid1",
                            "Activities",
                            "Activities",
                            "TaxonomyParentGuid"
                            ),
                new TaxonomyDto(
                            "TaxonomyGuid2",
                            "Holiday clubs and schemes",
                            "Holiday clubs and schemes",
                            "TaxonomyParentGuid"
                            )
            };
            return new PaginatedList<TaxonomyDto>(list, list.Count, 1, list.Count);
        }

        private static void AssertAddress(ServiceDto service, string address1, string city, string postcode, string county)
        {
            var serviceAtLocation = service.ServiceAtLocations?.First();
            var location = serviceAtLocation?.Location;
            var physicalAddresses = location?.PhysicalAddresses?.First();

            Assert.Equal(address1, physicalAddresses?.Address1);
            Assert.Equal(city, physicalAddresses?.City);
            Assert.Equal(postcode, physicalAddresses?.PostCode);
            Assert.Equal("England", physicalAddresses?.Country);
            Assert.Equal(county, physicalAddresses?.StateProvince);

        }

        private static void AssertContact(ICollection<LinkContactDto>? contacts, string telephone, string sms, string url, string email)
        {
            Assert.NotNull(contacts);
            var contact = contacts.First().Contact;

            Assert.Equal(telephone, contact.Telephone);
            Assert.Equal(sms, contact.TextPhone);
            Assert.Equal(url, contact.Url);
            Assert.Equal(email, contact.Email);
        }
    
        private static void AssertCostOptions(ServiceDto service, string description, decimal amount, string option)
        {
            var costOption = service.CostOptions?.First();

            Assert.NotNull(costOption);
            Assert.Equal(description, costOption.AmountDescription);
            Assert.Equal(amount, costOption.Amount);
            Assert.Equal(option, costOption.Option);
        }

        private static void AssertTaxonomy(ServiceDto service, string expectedTaxonomyName)
        {
            var taxonomy = GetTestTaxonomies().Items.Where(x=>x.Name == expectedTaxonomyName).First();
            if(taxonomy is null)
            {
                throw new ArgumentException($"{expectedTaxonomyName} is not valid for the test");
            }

            var actualTaxonomy = service.ServiceTaxonomies?.First().Taxonomy;
            
            Assert.NotNull(actualTaxonomy);
            Assert.Equal(taxonomy, actualTaxonomy);
        }

        private static void AssertEligibilities(ServiceDto service, string description, int max, int min)
        {
            var eligibilities = service.Eligibilities?.First();

            Assert.NotNull(eligibilities);
            Assert.Equal(description, eligibilities.EligibilityDescription);
            Assert.Equal(max, eligibilities.MaximumAge);
            Assert.Equal(min, eligibilities.MinimumAge);
        }
    }
}
