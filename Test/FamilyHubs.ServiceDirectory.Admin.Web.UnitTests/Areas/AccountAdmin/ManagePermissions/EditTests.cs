using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Constants;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages.ManagePermissions;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.SharedKernel.Identity;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests.Areas.AccountAdmin.ManagePermissions
{
    public class EditTests
    {
        private const long _organisationId = 1;
        private const long _expectedAccountId = 1234;
        private const string _expectedEmail = "anyEmail";
        private const string _expectedName = "anyName";
        private const string _expectedOrganisation = "anyOrganisation";
        private const string _expectedBackPath = "/AccountAdmin/ManagePermissions";
        private readonly Mock<IIdamClient> _mockIdamClient;
        private readonly Mock<IServiceDirectoryClient> _mockServiceDirectoryClient;
        private readonly Mock<ICacheService> _mockCacheService;

        public EditTests()
        {
            _mockIdamClient = new Mock<IIdamClient>();
            _mockServiceDirectoryClient = new Mock<IServiceDirectoryClient>();
            _mockCacheService = new Mock<ICacheService>();
        }

        [Theory]
        [InlineData(RoleTypes.LaManager, RoleDescription.LaManager)]
        [InlineData(RoleTypes.LaProfessional, RoleDescription.LaProfessional)]
        [InlineData(RoleTypes.LaDualRole, $"{RoleDescription.LaManager}, {RoleDescription.LaProfessional}")]
        [InlineData(RoleTypes.VcsManager, RoleDescription.VcsManager)]
        [InlineData(RoleTypes.VcsProfessional, RoleDescription.VcsProfessional)]
        [InlineData(RoleTypes.VcsDualRole, $"{RoleDescription.VcsManager}, {RoleDescription.VcsProfessional}")]
        public async Task OnGet_SetsValues(string accountRole, string expectedRole)
        {
            //  Arrange
            var account = GetAccountDto(_expectedAccountId, _expectedEmail, _expectedName, accountRole, _organisationId);
            _mockIdamClient.Setup(m => m.GetAccountById(_expectedAccountId)).Returns(account);

            var organisationDto = GetOrganisationDto(_organisationId, _expectedOrganisation);
            _mockServiceDirectoryClient.Setup(m => m.GetOrganisationById(_organisationId)).Returns(organisationDto);

            _mockCacheService.Setup(m => m.RetrieveLastPageName()).Returns(Task.FromResult(_expectedBackPath));

            var sut = new EditModel(_mockIdamClient.Object, _mockServiceDirectoryClient.Object, _mockCacheService.Object);
            sut.AccountId = _expectedAccountId.ToString();

            //  Act
            await sut.OnGet();

            //  Assert
            Assert.Equal(_expectedEmail, sut.Email);
            Assert.Equal(_expectedName, sut.Name);
            Assert.Equal(_expectedOrganisation, sut.Organisation);
            Assert.Equal(expectedRole, sut.Role);
            Assert.Equal(_expectedBackPath, sut.BackPath);
        }

        [Fact]
        public async Task OnGet_ThrowsIfUserIsDfeAdmin()
        {
            //  Arrange
            var account = GetAccountDto(_expectedAccountId, _expectedEmail, _expectedName, RoleTypes.DfeAdmin, _organisationId);
            _mockIdamClient.Setup(m => m.GetAccountById(_expectedAccountId)).Returns(account);

            var organisationDto = GetOrganisationDto(_organisationId, _expectedOrganisation);
            _mockServiceDirectoryClient.Setup(m => m.GetOrganisationById(_organisationId)).Returns(organisationDto);

            _mockCacheService.Setup(m => m.RetrieveLastPageName()).Returns(Task.FromResult(_expectedBackPath));

            var sut = new EditModel(_mockIdamClient.Object, _mockServiceDirectoryClient.Object, _mockCacheService.Object);
            sut.AccountId = _expectedAccountId.ToString();

            //  Act/Assert
            var exception = await Assert.ThrowsAsync<Exception>(async()=> await sut.OnGet());
            Assert.Equal("Role type not Valid", exception.Message);

        }

        private Task<AccountDto?> GetAccountDto(long id, string email, string name, string role, long organisationId)
        {
            var account = new AccountDto();
            account.Id = id;
            account.Email = email;
            account.Name = name;

            var claims = new List<AccountClaimDto>();
            claims.Add(new AccountClaimDto() { Name = FamilyHubsClaimTypes.OrganisationId, Value = organisationId.ToString() });
            claims.Add(new AccountClaimDto() { Name = FamilyHubsClaimTypes.Role, Value = role });

            account.Claims = claims;

            return Task.FromResult((AccountDto?)account);
        }

        private Task<OrganisationWithServicesDto?> GetOrganisationDto(long organisationId, string organisationName)
        {
            var organisation = new OrganisationWithServicesDto 
            { 
                AdminAreaCode = "Any",
                Description = "Any",
                Name= organisationName,
                OrganisationType = Shared.Enums.OrganisationType.NotSet,
                Id= organisationId
            };

            return Task.FromResult((OrganisationWithServicesDto?)organisation);
        }
    }
}
