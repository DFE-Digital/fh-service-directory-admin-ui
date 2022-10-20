using FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.UnitTests.Pages.OrganisationAdmin;

public class OrganisationAdminDeleteServicePage
{
    private DeleteServiceModel pageModel;

    public OrganisationAdminDeleteServicePage()
    {
        var mockLocalOfferClientService = new Mock<ILocalOfferClientService>();
        var mockISessionService = new Mock<ISessionService>();
        pageModel = new DeleteServiceModel(mockLocalOfferClientService.Object, mockISessionService.Object);
    }

    [Fact]
    public void ValidationShouldPass_EvenWhenNoOptionSelected()
    {
        /*As per AC, even when no option selected, it should not fail 
         * and rather navigate to 'Option not deleted' page*/

        //Arrange
        pageModel.SelectedOption = String.Empty;
        string servcieId = "abc";

        // Act
        var result = pageModel.OnPost(servcieId) as Task<IActionResult>;

        // Assert
        pageModel.ModelState.IsValid.Should().BeTrue();
    }
}
