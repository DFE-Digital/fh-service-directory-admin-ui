using System.Collections.Generic;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.UnitTests.Pages.OrganisationAdmin;

public class OrganisationAdminWhatLanguage
{
    private readonly WhatLanguageModel pageModel;

    public OrganisationAdminWhatLanguage()
    {
        var mockISessionService = new Mock<ISessionService>();
        var mockIRedisCacheService = new Mock<IRedisCacheService>();
        pageModel = new WhatLanguageModel(mockISessionService.Object, mockIRedisCacheService.Object);
    }

    [Fact]
    public void ValidationShouldFail_WhenNoOptionSelected()
    {
        //Arrange
        pageModel.LanguageCode = new List<string>();

        // Act
        var result = pageModel.OnPostNextPage();

        // Assert
        pageModel.ValidationValid.Should().BeFalse();
    }

}
