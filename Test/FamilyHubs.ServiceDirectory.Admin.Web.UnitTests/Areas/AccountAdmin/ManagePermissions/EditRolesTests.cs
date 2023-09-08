using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages.ManagePermissions;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests.Areas.AccountAdmin.ManagePermissions
{
    public class EditRolesTests
    {
        private readonly Mock<IIdamClient> _mockIdamClient;
        private readonly Mock<IEmailService> _mockEmailService;

        public EditRolesTests()
        {
            _mockIdamClient = new Mock<IIdamClient>();
            _mockEmailService = new Mock<IEmailService>();
        }

        [Fact]
        public async Task OnGet_BackPathSet()
        {
            //  Arrange
            const long accountId = 1;
            var account = new AccountDto
            {
                Claims = new List<AccountClaimDto>() { new AccountClaimDto() { Name = "role", Value = "LaManager" } }
            };
            _mockIdamClient.Setup(x => x.GetAccountById(It.IsAny<long>())).ReturnsAsync(account);
            var sut = new EditRolesModel(_mockIdamClient.Object, _mockEmailService.Object);

            //  Act
            await sut.OnGet(accountId);

            //  Assert
            Assert.Equal($"/AccountAdmin/ManagePermissions/{accountId}", sut.BackButtonPath);
        }

        [Fact]
        public async Task OnGet_AccountRetrievedFromIdams()
        {
            //  Arrange
            const long accountId = 1;
            var account = new AccountDto
            {
                Claims = new List<AccountClaimDto>() { new AccountClaimDto() { Name = "role", Value = "LaManager" } }
            };
            _mockIdamClient.Setup(x => x.GetAccountById(It.IsAny<long>())).ReturnsAsync(account);
            var sut = new EditRolesModel(_mockIdamClient.Object, _mockEmailService.Object);

            //  Act
            await sut.OnGet(accountId);

            //  Assert
            _mockIdamClient.Verify(x=>x.GetAccountById(accountId), Times.Once());
        }

        [Theory]
        [InlineData(RoleTypes.LaManager, true, false, false, false)]
        [InlineData(RoleTypes.LaProfessional, false,true, false, false)]
        [InlineData(RoleTypes.LaDualRole, true, true, false, false)]
        [InlineData(RoleTypes.VcsManager,false, false, true, false )]
        [InlineData(RoleTypes.VcsProfessional, false, false, false, true)]
        [InlineData(RoleTypes.VcsDualRole, false, false, true, true)]
        public async Task OnGet_UserRolesSet(string role, bool laManagerValue, bool laProfessionalValue, bool vcsManagerValue, bool vcsProfessionalValue  )
        {
            //  Arrange
            const long accountId = 1;
            var account = new AccountDto
            {
                Claims = new List<AccountClaimDto>() { new AccountClaimDto() { Name = "role", Value = role } }
            };
            _mockIdamClient.Setup(x => x.GetAccountById(It.IsAny<long>())).ReturnsAsync(account);
            var sut = new EditRolesModel(_mockIdamClient.Object, _mockEmailService.Object);

            //  Act
            await sut.OnGet(accountId);

            //  Assert            
            Assert.Equal(laManagerValue, sut.LaManager);
            Assert.Equal(laProfessionalValue, sut.LaProfessional);
            Assert.Equal(vcsManagerValue, sut.VcsManager);
            Assert.Equal(vcsProfessionalValue, sut.VcsProfessional);                       
        }


        [Theory]
        [InlineData(RoleTypes.LaManager, true, false)]
        [InlineData(RoleTypes.LaProfessional,true, false )]
        [InlineData(RoleTypes.LaDualRole, true, false)]
        [InlineData(RoleTypes.VcsManager, false, true )]
        [InlineData(RoleTypes.VcsProfessional, false, true)]
        [InlineData(RoleTypes.VcsDualRole, false, true)]
        public async Task OnGet_UserTypeSet(string role, bool isLaValue,  bool isVcsValue )
        {
            //  Arrange
            const long accountId = 1;
            var account = new AccountDto
            {
                Claims = new List<AccountClaimDto>() { new AccountClaimDto() { Name = "role", Value = role } }
            };
            _mockIdamClient.Setup(x => x.GetAccountById(It.IsAny<long>())).ReturnsAsync(account);
            var sut = new EditRolesModel(_mockIdamClient.Object, _mockEmailService.Object);

            //  Act
            await sut.OnGet(accountId);

            //  Assert
            Assert.Equal(isLaValue, sut.IsLa);
            Assert.Equal(isVcsValue, sut.IsVcs);
        }

        [Fact]
        public async Task OnPost_NoRoleSelected_ReturnsPage()
        {
            //  Arrange
            const long accountId = 1;
            var account = new AccountDto
            {
                Claims = new List<AccountClaimDto>() { new AccountClaimDto() { Name = "role", Value = "LaManager" } }
            };
            _mockIdamClient.Setup(x => x.GetAccountById(It.IsAny<long>())).ReturnsAsync(account);
            var sut = new EditRolesModel(_mockIdamClient.Object, _mockEmailService.Object);

            //  Act
            var result = await sut.OnPost(accountId);

            //  Assert
            Assert.IsType<PageResult>(result);
            Assert.True(sut.HasValidationError);
        }

        [Fact]
        public async Task OnPost_InvokesUpdateMethod()
        {
            //  Arrange
            const long accountId = 1;
            var account = new AccountDto
            {
                Claims = new List<AccountClaimDto>() { new AccountClaimDto() { Name = "role", Value = "LaManager" } }
            };
            _mockIdamClient.Setup(x => x.GetAccountById(It.IsAny<long>())).ReturnsAsync(account);
            var sut = new EditRolesModel(_mockIdamClient.Object, _mockEmailService.Object);
            sut.LaManager = true;
            //  Act
            var result = await sut.OnPost(accountId);

            //  Assert
            Assert.IsType<RedirectToPageResult>(result);
            Assert.False(sut.HasValidationError);
            _mockIdamClient.Verify(m => m.UpdateClaim(It.IsAny<UpdateClaimDto>()), Times.Once);
        }

        [Fact]
        public async Task OnPost_LaEmialNotificationIsSent()
        {
            //  Arrange
            const long accountId = 1;
            const string email = "test@test.com";
            var account = new AccountDto
            {
                Email = email,
                Claims = new List<AccountClaimDto>() { new AccountClaimDto() { Name = "role", Value = "LaManager" } }
            };
            _mockIdamClient.Setup(x => x.GetAccountById(It.IsAny<long>())).ReturnsAsync(account);
            _mockEmailService.Setup(x => x.SendLaPermissionChangeEmail(It.IsAny<PermissionChangeNotificationModel>()));

            var sut = new EditRolesModel(_mockIdamClient.Object, _mockEmailService.Object);
            //LaDualRole selected
            sut.LaManager = true;
            sut.LaProfessional = true;
            
            //  Act
            await sut.OnPost(accountId);

            //  Assert            
            _mockEmailService.Verify(m => m.SendLaPermissionChangeEmail(
                It.Is<PermissionChangeNotificationModel>(x=>x.EmailAddress == email && x.OldRole == "LaManager" && x.NewRole == "LaDualRole")), Times.Once); 
        }

        [Fact]
        public async Task OnPost_VcsEmialNotificationIsSent()
        {
            //  Arrange
            const long accountId = 1;
            const string email = "test@test.com";
            var account = new AccountDto
            {
                Email = email,
                Claims = new List<AccountClaimDto>() { new AccountClaimDto() { Name = "role", Value = "VcsManager" } }
            };
            _mockIdamClient.Setup(x => x.GetAccountById(It.IsAny<long>())).ReturnsAsync(account);
            _mockEmailService.Setup(x => x.SendVcsPermissionChangeEmail(It.IsAny<PermissionChangeNotificationModel>()));

            var sut = new EditRolesModel(_mockIdamClient.Object, _mockEmailService.Object);
            //VcsDualRole selected
            sut.VcsManager = true;
            sut.VcsProfessional = true;

            //  Act
            await sut.OnPost(accountId);

            //  Assert            
            _mockEmailService.Verify(m => m.SendVcsPermissionChangeEmail(
                It.Is<PermissionChangeNotificationModel>(x=>x.EmailAddress == email && x.OldRole == "VcsManager" && x.NewRole == "VcsDualRole")), Times.Once);
        }
    }
}
