using System;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using FluentAssertions;
using Moq;
using Xunit;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.UnitTests.Pages.OrganisationAdmin;

public class OrganisationAdminDeleteServicePage
{
    private readonly DeleteServiceModel pageModel;

    public OrganisationAdminDeleteServicePage()
    {
        var mockLocalOfferClientService = new Mock<ILocalOfferClientService>();
        var mockISessionService = new Mock<ISessionService>();
        var mockIRedisCacheService = new Mock<IRedisCacheService>();
        pageModel = new DeleteServiceModel(mockLocalOfferClientService.Object, mockISessionService.Object, mockIRedisCacheService.Object);
    }

    [Fact]
    public void ValidationShouldPass_EvenWhenNoOptionSelected()
    {
        /*As per AC, even when no option selected, it should not fail 
         * and rather navigate to 'Option not deleted' page*/

        //Arrange
        pageModel.SelectedOption = String.Empty;
        var servcieId = "abc";

        // Act
        var result = pageModel.OnPost(servcieId);

        // Assert
        pageModel.ModelState.IsValid.Should().BeTrue();
    }
}
