using System.Collections.Generic;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.OrganisationAdmin;
using FluentAssertions;
using Moq;
using Xunit;

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests.Pages.OrganisationAdmin;

public class OrganisationAdminWhatLanguage
{
    private readonly WhatLanguageModel _pageModel;

    public OrganisationAdminWhatLanguage()
    {
        var mockIRedisCacheService = new Mock<IRedisCacheService>();
        _pageModel = new WhatLanguageModel(mockIRedisCacheService.Object);
    }

    [Fact]
    public void ValidationShouldFail_WhenNoOptionSelected()
    {
        //Arrange
        _pageModel.LanguageCode = new List<string>();

        // Act
        _pageModel.OnPostNextPage();

        // Assert
        _pageModel.ValidationValid.Should().BeFalse();
    }

}
