using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.La;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests.La;

public class PersonalDetailsTests
{
    private readonly Mock<IRequestDistributedCache> _mockRequestDistributedCache;
    private readonly PersonsDetailsModel _personalDetailsModel;

    public PersonalDetailsTests()
    {
        _mockRequestDistributedCache = new Mock<IRequestDistributedCache>();
        DefaultHttpContext httpContext = new DefaultHttpContext();
        _personalDetailsModel = new PersonsDetailsModel(_mockRequestDistributedCache.Object);
        _personalDetailsModel.PageContext.HttpContext = httpContext;
        CompiledPageActionDescriptor compiledPageActionDescriptor = new();
        _personalDetailsModel.PageContext.ActionDescriptor = compiledPageActionDescriptor;
        _personalDetailsModel.PageContext.ActionDescriptor.DisplayName = "/La/PersonsDetails";
    }

    [Theory]
    [InlineData("email", "TestUser@email.com", "")]
    [InlineData("phone", "123456", "")]
    [InlineData("textphone", "1234567", "")]
    [InlineData("nameandpostcode", "Test User", "B60 1PY")]
    public async Task ThenPersonsDetailsOnGetIsSuccessfull(string selectionType, string value1, string value2)
    {
        //Arrange
        int callback = 0;
        _mockRequestDistributedCache.Setup(x => x.GetAsync<SubjectAccessRequestViewModel>(It.IsAny<string>()))
            .Callback(() => callback++)
            .ReturnsAsync(new SubjectAccessRequestViewModel
            {
                SelectionType = selectionType, Value1 = value1, Value2 = value2
            });

        //Act
        await _personalDetailsModel.OnGet();

        // Assert
        callback.Should().Be(1);
        _personalDetailsModel.ContactSelection[0].Should().Be(selectionType);
    }

    [Fact]
    public async Task ThenPersonsDetailsOnGetIsSuccessfull_WithoutViewModel()
    {
        //Arrange
        int callback = 0;
        _mockRequestDistributedCache.Setup(x => x.GetAsync<SubjectAccessRequestViewModel>(It.IsAny<string>()))
        .Callback(() => callback++);

        //Act
        await _personalDetailsModel.OnGet();

        // Assert
        callback.Should().Be(1);
    }

    [Theory]
    [InlineData("email")]
    [InlineData("phone")]
    [InlineData("textphone")]
    [InlineData("nameandpostcode")]
    public async Task ThenPersonsDetailsOnPostIsSuccessfull(string selectionType)
    {
        //Arrange
        _personalDetailsModel.ContactSelection = new List<string> { selectionType };
        _personalDetailsModel.Email = "TestUser@email.com";
        _personalDetailsModel.Telephone = "1234567890";
        _personalDetailsModel.Textphone = "1234567890";
        _personalDetailsModel.Name = "Test User";
        _personalDetailsModel.Postcode = "B60 1PY";
        int callback = 0;
        _mockRequestDistributedCache.Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<SubjectAccessRequestViewModel>()))
            .Callback(() => callback++);
            
        //Act
        var result = await _personalDetailsModel.OnPost() as RedirectToPageResult;

        // Assert
        callback.Should().Be(1);
        _personalDetailsModel.ValidationValid.Should().BeTrue();
        ArgumentNullException.ThrowIfNull(result);
        result.PageName.Should().Be("/La/SubjectAccessResultDetails");
    }

    [Theory]
    [InlineData("email", "some_email")]
    [InlineData("phone", "some_email")]
    [InlineData("textphone", "some_email")]
    [InlineData("nameandpostcode", "some_email")]
    [InlineData("", default!)]
    public async Task ThenPersonsDetailsOnPostFailValidation(string selectionType, string value)
    {
        //Arrange
        _personalDetailsModel.Email = value;
        _personalDetailsModel.ContactSelection = new List<string> { selectionType };
        if (selectionType == string.Empty)
        {
            _personalDetailsModel.ContactSelection.Clear();
        }
        int callback = 0;
        _mockRequestDistributedCache.Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<SubjectAccessRequestViewModel>()))
            .Callback(() => callback++);

        //Act
        await _personalDetailsModel.OnPost();

        // Assert
        callback.Should().Be(0);
        _personalDetailsModel.ValidationValid.Should().BeFalse();
        switch(selectionType) 
        {
            case "email":
                _personalDetailsModel.EmailValid.Should().BeFalse();
                break;

            case "phone":
                _personalDetailsModel.PhoneValid.Should().BeFalse();
                break;

            case "textphone":
                _personalDetailsModel.TextValid.Should().BeFalse();
                break;

            case "nameandpostcode":
                _personalDetailsModel.NameValid.Should().BeFalse();
                _personalDetailsModel.PostcodeValid.Should().BeFalse();
                break;
        }
    }
}
