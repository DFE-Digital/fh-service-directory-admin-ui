using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages.ManagePermissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Client;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests.Areas.AccountAdmin.ManagePermissions
{
    public class EditEmailTests
    {
        private readonly Mock<IIdamClient> _mockIdamClient;

        public EditEmailTests()
        {
            _mockIdamClient = new Mock<IIdamClient>();
        }

        [Fact]
        public void OnGet_BackPathSet()
        {
            //  Arrange
            const string accountId = "1234";
            var sut = new EditEmailModel(_mockIdamClient.Object) { EmailAddress = string.Empty, AccountId = accountId};

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
            var sut = new EditEmailModel(_mockIdamClient.Object) { EmailAddress = string.Empty, AccountId = accountId };

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
            var account = new AccountDto { Id = 1234, Email = "oldEmail", Name = "name" };
            _mockIdamClient.Setup(m => m.GetAccountById(1234)).Returns(Task.FromResult((AccountDto?)account));

            var sut = new EditEmailModel(_mockIdamClient.Object) { EmailAddress = email, AccountId = accountId };

            //  Act
            var result = await sut.OnPost();

            //  Assert
            Assert.IsType<RedirectToPageResult>(result);
            Assert.False(sut.HasValidationError);
            _mockIdamClient.Verify(m => m.UpdateAccount(It.IsAny<UpdateAccountDto>()), Times.Once);
        }
    }
}
