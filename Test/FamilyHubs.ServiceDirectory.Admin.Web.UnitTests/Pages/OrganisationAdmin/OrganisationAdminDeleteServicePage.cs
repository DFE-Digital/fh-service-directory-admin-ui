using System.Threading.Tasks;
using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Web.Areas.OrganisationAdmin.Pages;
using FluentAssertions;
using Moq;
using Xunit;

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests.Pages.OrganisationAdmin;

public class OrganisationAdminDeleteServicePage
{
    private readonly DeleteServiceModel _pageModel;

    public OrganisationAdminDeleteServicePage()
    {
        var mockLocalOfferClientService = new Mock<IServiceDirectoryClient>();
        _pageModel = new DeleteServiceModel(mockLocalOfferClientService.Object);
    }

    [Fact]
    public async Task ValidationShouldPass_EvenWhenNoOptionSelected()
    {
        /*As per AC, even when no option selected, it should not fail 
         * and rather navigate to 'Option not deleted' page*/

        //Arrange
        _pageModel.SelectedOption = string.Empty;
        const long ServiceId = 123;

        // Act
        await _pageModel.OnPost(ServiceId);

        // Assert
        _pageModel.ModelState.IsValid.Should().BeTrue();
    }
}
