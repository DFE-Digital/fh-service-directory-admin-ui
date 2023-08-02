using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages.ManagePermissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Client;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests.Areas.AccountAdmin.ManagePermissions
{
    public class EditEmailTests
    {
        private readonly Mock<IIdamClient> _mockIdamClient;
        private readonly Mock<IEmailService> _mockEmailService;

        public EditEmailTests()
        {
            _mockIdamClient = new Mock<IIdamClient>();
            _mockEmailService = new Mock<IEmailService>();
        }

        [Fact]
        public void OnGet_BackPathSet()
        {
            //  Arrange
            const string accountId = "1234";
            var sut = new EditEmailModel(_mockIdamClient.Object, _mockEmailService.Object) { EmailAddress = string.Empty, AccountId = accountId};

            //  Act
            sut.OnGet();

            //  Assert
            Assert.Equal($"/AccountAdmin/ManagePermissions/{accountId}", sut.BackButtonPath);
        }

        [Fact]
        public async Task OnPost_InvalidEmail_ReturnsPage()
        {
            //  Arrange
            const string accountId = "1234";
            var sut = new EditEmailModel(_mockIdamClient.Object, _mockEmailService.Object) { EmailAddress = string.Empty, AccountId = accountId };

            //  Act
            var result = await sut.OnPost();

            //  Assert
            Assert.IsType<PageResult>(result);
            Assert.True(sut.HasValidationError);
        }

        [Fact]
        public async Task OnPost_InvokesUpdateMethod()
        {
            //  Arrange
            const string accountId = "1234";
            const string email = "some.guy@test.com";
            var account = new AccountDto { Id = 1234, Email = "oldEmail", Name = "name" , 
                Claims = new List<AccountClaimDto>() { new AccountClaimDto() { Name = "role", Value = "LaManager" } } };
            _mockIdamClient.Setup(m => m.GetAccountById(1234)).Returns(Task.FromResult((AccountDto?)account));

            var sut = new EditEmailModel(_mockIdamClient.Object, _mockEmailService.Object) { EmailAddress = email, AccountId = accountId };

            //  Act
            var result = await sut.OnPost();

            //  Assert
            Assert.IsType<RedirectToPageResult>(result);
            Assert.False(sut.HasValidationError);
            _mockIdamClient.Verify(m => m.UpdateAccount(It.IsAny<UpdateAccountDto>()), Times.Once);
        }


        [Fact]
        public async Task OnPost_EmialNotificationIsSent()
        {
            //  Arrange
            const string accountId = "1234";
            const string email = "some.guy@test.com";
            var account = new AccountDto
            {
                Id = 1234,
                Email = "oldEmail",
                Name = "name",
                Claims = new List<AccountClaimDto>() { new AccountClaimDto() { Name = "role", Value = "LaManager" } }
            };
            _mockIdamClient.Setup(m => m.GetAccountById(1234)).Returns(Task.FromResult((AccountDto?)account));
            _mockEmailService.Setup(x => x.SendAccountEmailUpdatedEmail(It.IsAny<EmailChangeNotificationModel>()));

            var sut = new EditEmailModel(_mockIdamClient.Object, _mockEmailService.Object) { EmailAddress = email, AccountId = accountId };            

            //  Act
            var result = await sut.OnPost();

            //  Assert            
            _mockEmailService.Verify(m => m.SendAccountEmailUpdatedEmail(
                It.Is<EmailChangeNotificationModel>(x => x.EmailAddress == email && x.Role == "LaManager")), Times.Once);
        }
    }
}
