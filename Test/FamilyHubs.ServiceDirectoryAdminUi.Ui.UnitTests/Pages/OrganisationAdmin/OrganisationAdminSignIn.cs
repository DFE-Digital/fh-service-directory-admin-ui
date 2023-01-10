using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using Xunit;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using Moq;
using Microsoft.Extensions.Configuration;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using System.Collections.Generic;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.UnitTests
{
    public class OrganisationAdminSignIn
    {
        private SignInModel _signInModel;
        private IConfiguration _configuration;
        public OrganisationAdminSignIn()
        {
            var mockSessionService = new Mock<ISessionService>();
            var mockIRedisCacheService = new Mock<IRedisCacheService>();
            var inMemorySettings = new Dictionary<string, string> {{"PasswordHash", "$2a$11$/.pVvygVsBaYWY9Dyl0Steol6kiFpIoOOnei5ZltSa4BSWYXTf9n." } };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();
            _signInModel = new SignInModel(mockSessionService.Object, mockIRedisCacheService.Object, _configuration);
        }

        [Fact]
        public void RedirectToOrganizationPage_WithCorrectPassword()
        {
            // Arrange
            _signInModel.Password = "Brockett";


            // Act
            var result = _signInModel.OnPost() as RedirectToRouteResult;

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void ValidationShouldFail_WhenInCorrectPasswordEntered()
        {
            // Arrange
            _signInModel.Password = "incorrect";

            // Act
            var result = _signInModel.OnPost() as RedirectToRouteResult;

            // Assert
            _signInModel.ModelState.ErrorCount.Should().BeGreaterThan(0);
        }
    }
}