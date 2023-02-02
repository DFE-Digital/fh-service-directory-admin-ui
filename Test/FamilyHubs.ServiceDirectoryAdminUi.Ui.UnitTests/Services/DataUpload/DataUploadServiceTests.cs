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
using System.Linq.Expressions;
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
            dataTable.Columns[ColumnHeaders.LOCAL_AUTHORITY]!.Expression = "''"; // Invalidate column in all rows
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

            dataTable.Columns[ColumnHeaders.ORGANISATION_TYPE]!.Expression = "'voluntary and community sector'";
            dataTable.Columns[ColumnHeaders.NAME_OF_ORGANISATION]!.Expression = "''"; // Invalidate column in all rows
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

            var sut = new DataUploadService(
                _mockOrganisationAdminClientService.Object,
                _mockPostcodeLocationClientService.Object,
                mockExcelReader.Object);

            //  Act
            await sut.UploadToApi(FakeDataHelper.EXISTING_ORGANISATION_ID, _fileUpload, false);

            //  Assert
            _mockOrganisationAdminClientService.Verify(
                m => m.CreateService(
                    It.Is<ServiceDto>(service => 
                        service.Name == FakeDataHelper.TO_BE_CREATED_SERVICE_NAME &&
                        service.OrganisationId == FakeDataHelper.EXISTING_ORGANISATION_ID)
                )
            );
        }

        [Fact]
        public async Task UploadToApi_To_UpdateExistingOrganisation_UpdateServiceCalled()
        {
            //  Arrange
            var dataTable = FakeDataHelper.GetTestDataTableToUpdateExistingOrganisation();
            var mockExcelReader = GetMockExcelReader(dataTable);

            var sut = new DataUploadService(
                _mockOrganisationAdminClientService.Object,
                _mockPostcodeLocationClientService.Object,
                mockExcelReader.Object);

            //  Act
            await sut.UploadToApi(FakeDataHelper.EXISTING_ORGANISATION_ID, _fileUpload, false);

            //  Assert
            _mockOrganisationAdminClientService.Verify(
                m => m.UpdateService(
                    It.Is<ServiceDto>(service =>
                        service.Name == FakeDataHelper.TO_BE_UPDATED_SERVICE_NAME &&
                        service.OrganisationId == FakeDataHelper.EXISTING_ORGANISATION_ID)
                )
            );
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

        private Mock<IExcelReader> GetMockExcelReader(DataTable dataTable)
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

        private PaginatedList<TaxonomyDto> GetTestTaxonomies()
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

    }
}
