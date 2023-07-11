using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages.ManagePermissions;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests.Areas.AccountAdmin.ManagePermissions
{
    public class EditEmailChangedTests
    {
        private readonly Mock<IIdamClient> _mockIdamClient;

        public EditEmailChangedTests()
        {
            _mockIdamClient = new Mock<IIdamClient>();
        }

        [Fact]
        public async Task OnGet_BackPathSet()
        {
            //  Arrange
            const string accountId = "1234";
            const string userName = "UsersName";
            var account = new AccountDto { Id = 1234, Email = "email", Name = userName };
            _mockIdamClient.Setup(m => m.GetAccountById(1234)).Returns(Task.FromResult((AccountDto?)account));

            var sut = new EditEmailChangedConfirmationModel(_mockIdamClient.Object) { AccountId = accountId };

            //  Act
            await sut.OnGet();

            //  Assert
            Assert.Equal(userName, sut.UserName);
        }

    }
}
