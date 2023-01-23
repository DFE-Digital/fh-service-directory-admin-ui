using FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.UnitTests.Pages.OrganisationAdmin;

public class OrganisationAdminServiceDescription
{
    private readonly ServiceDescriptionModel pageModel;

    public OrganisationAdminServiceDescription()
    {
        var mockISessionService = new Mock<ISessionService>();
        var mockIRedisCacheService = new Mock<IRedisCacheService>();
        pageModel = new ServiceDescriptionModel(mockISessionService.Object, mockIRedisCacheService.Object);
    }

    [Fact]
    public void ValidationShouldFail_WhenMoreThan500CharactersEntered()
    {
        //Arrange
        pageModel.Description = "ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ" +
                                "ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ" +
                                "ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ" +
                                "ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ" +
                                "ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ" +
                                "ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ" +
                                "ABCSDFGHJKLMNOPQRSTUVWXYZ ABCSDFGHJKLMNOPQRSTUVWXYZ";

        // Act
        var result = pageModel.OnPost();

        // Assert
        pageModel.ModelState.IsValid.Should().BeFalse();
    }

}
