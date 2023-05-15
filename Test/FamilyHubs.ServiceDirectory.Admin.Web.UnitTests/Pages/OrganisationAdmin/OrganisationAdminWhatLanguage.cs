using System.Collections.Generic;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.Areas.OrganisationAdmin.Pages;
using FluentAssertions;
using Moq;
using Xunit;

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests.Pages.OrganisationAdmin;

public class OrganisationAdminWhatLanguage
{
    private readonly WhatLanguageModel _pageModel;

    public OrganisationAdminWhatLanguage()
    {
        var mockICacheService = new Mock<ICacheService>();
        _pageModel = new WhatLanguageModel(mockICacheService.Object);
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
