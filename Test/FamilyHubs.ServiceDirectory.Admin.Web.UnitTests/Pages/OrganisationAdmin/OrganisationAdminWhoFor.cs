using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.OrganisationAdmin;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests.Pages.OrganisationAdmin;

public class OrganisationAdminWhoFor
{
    private readonly WhoForModel _pageModel;

    public OrganisationAdminWhoFor()
    {
        var mockIRedisCacheService = new Mock<IRedisCacheService>();
        var orgVm = new OrganisationViewModel
        {
            Id = 1,
            Name = "Test Org",
            ServiceName = "Test Service"
        };
        mockIRedisCacheService.Setup(org => org.RetrieveOrganisationWithService()).Returns(orgVm);
        _pageModel = new WhoForModel(mockIRedisCacheService.Object);
    }

    [Fact]
    public void OnPost_ValidationFails_WhenNoOptionSelected()
    {
        //Arrange
        _pageModel.Children = string.Empty;

        // Act
        _ = _pageModel.OnPost() as RedirectToPageResult;

        // Assert
        _pageModel.ModelState.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("Yes", null, null)]
    [InlineData("Yes", null, "0")]
    [InlineData("Yes", "0", null)]
    [InlineData("Yes", "0", "0")]
    [InlineData("Yes", "1", "0")]
    [InlineData("Yes", "10", "9")]
    public void OnPost_ValidationFails_WhenInvalidAgeRangeSelected(string children, string minAge, string maxAge)
    {
        //Arrange
        _pageModel.Children = children;
        _pageModel.SelectedMinAge = minAge;
        _pageModel.SelectedMaxAge = maxAge;

        // Act
        _ = _pageModel.OnPost() as RedirectToPageResult;

        // Assert
        _pageModel.ModelState.IsValid.Should().BeFalse();
    }


}