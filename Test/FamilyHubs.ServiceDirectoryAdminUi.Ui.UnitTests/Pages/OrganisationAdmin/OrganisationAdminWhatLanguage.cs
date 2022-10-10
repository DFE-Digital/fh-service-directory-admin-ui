using FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
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

public class OrganisationAdminWhatLanguage
{
    private WhatLanguageModel pageModel;

    public OrganisationAdminWhatLanguage()
    {
        var mockISessionService = new Mock<ISessionService>();
        pageModel = new WhatLanguageModel(mockISessionService.Object);
    }

    [Fact]
    public void ValidationShouldFail_WhenNoOptionSelected()
    {
        //Arrange
        pageModel.LanguageCode = new List<string>();
        pageModel.LanguageCode.Add(string.Empty);
        pageModel.LanguageCode[0] = null;

        // Act
        var result = pageModel.OnPostNextPage() as IActionResult;

        // Assert
        //pageModel.ModelState.IsValid.Should().BeFalse();
        pageModel.ValidationValid.Should().BeFalse();
    }

    //[Fact]
    //public void ValidationShouldNotFailWhenAnOptionSelected()
    //{
    //    //Arrange
    //    pageModel.ServiceDeliverySelection.Add("Online");

    //    // Act
    //    var result = pageModel.OnPost() as RedirectToPageResult;

    //    // Assert
    //    pageModel.ModelState.IsValid.Should().BeTrue();
    //}
}
