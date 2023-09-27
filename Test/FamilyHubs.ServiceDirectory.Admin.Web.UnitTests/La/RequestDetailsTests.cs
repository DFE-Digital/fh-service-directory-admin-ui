using FamilyHubs.ReferralService.Shared.Dto;
using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Web.Areas.SubjectAccessRequest.Pages.La;
using FamilyHubs.SharedKernel.Razor.FamilyHubsUi.Options;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests.La;

public class RequestDetailsTests
{
    /*
    private readonly Mock<IReferralService> _mockReferralService;
    private readonly Mock<IOptions<FamilyHubsUiOptions>> _mockFamilyHubOptions;
    private readonly RequestDetailsModel _requestDetailsModel;

    public RequestDetailsTests()
    {
        var keys = new Dictionary<string, string>()
        {
            { "ConnectWeb", "SomeValue" }
        };

        _mockFamilyHubOptions = new Mock<IOptions<FamilyHubsUiOptions>>();
        _mockFamilyHubOptions.Setup(x => x.Value).Returns(new FamilyHubsUiOptions { Urls = keys });
        _mockReferralService = new Mock<IReferralService>();
        DefaultHttpContext httpContext = new DefaultHttpContext();
        _requestDetailsModel = new RequestDetailsModel(_mockReferralService.Object, _mockFamilyHubOptions.Object);
        _requestDetailsModel.PageContext.HttpContext = httpContext;
        CompiledPageActionDescriptor compiledPageActionDescriptor = new();
        _requestDetailsModel.PageContext.ActionDescriptor = compiledPageActionDescriptor;
        _requestDetailsModel.PageContext.ActionDescriptor.DisplayName = "/La/RequestDetails";
    }

    [Fact]
    public async Task ThenRequestDetailsOnGetIsSuccessfull()
    {
        //Arrange
        _mockReferralService.Setup(x => x.GetReferralById(It.IsAny<long>(), It.IsAny<CancellationToken>())).ReturnsAsync(SubjectAccessResultDetailsTests.GetReferralList()[0]);

        //Act
        await _requestDetailsModel.OnGet(1);

        //Assert
        _requestDetailsModel.Referral.Should().BeEquivalentTo(SubjectAccessResultDetailsTests.GetReferralList()[0]); 
    }
    */

    [Fact]
    public async Task OnGet_ValidId_ReturnsPageResult()
    {
        // Arrange
        int validId = 1;
        var referralServiceMock = new Mock<IReferralService>();
        referralServiceMock.Setup(service => service.GetReferralById(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SubjectAccessResultDetailsTests.GetReferralList()[0]);
        
        var keys = new Dictionary<string, string>()
        {
            { "ConnectWeb", "https://somevalue" }
        };
        var familyHubsUiOptionsMock = new Mock<IOptions<FamilyHubsUiOptions>>();
        familyHubsUiOptionsMock.Setup(options => options.Value)
            .Returns(new FamilyHubsUiOptions { Urls = keys });

        var model = new RequestDetailsModel(referralServiceMock.Object, familyHubsUiOptionsMock.Object);

        // Act
        var result = await model.OnGet(validId);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.NotNull(model.Referral);
    }

    [Fact]
    public async Task OnGet_ForbiddenId_RedirectsToErrorPage()
    {
        // Arrange
        var responseContent = "This is the response content";
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.Forbidden);
        httpResponseMessage.Content = new StringContent(responseContent);
        httpResponseMessage.RequestMessage = new HttpRequestMessage();
        httpResponseMessage.RequestMessage.RequestUri = new System.Uri("http://somevalue");

        int forbiddenId = 2;
        var referralServiceMock = new Mock<IReferralService>();
        referralServiceMock.Setup(service => service.GetReferralById(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ReferralClientServiceException(httpResponseMessage, "error"));

        var keys = new Dictionary<string, string>()
        {
            { "ConnectWeb", "https://somevalue" }
        };
        var familyHubsUiOptionsMock = new Mock<IOptions<FamilyHubsUiOptions>>();
        familyHubsUiOptionsMock.Setup(options => options.Value)
            .Returns(new FamilyHubsUiOptions { Urls = keys });

        var model = new RequestDetailsModel(referralServiceMock.Object, familyHubsUiOptionsMock.Object);

        // Act
        // Act
        Func<Task> act = async () => await model.OnGet(forbiddenId);

        // Assert
        await act.Should().ThrowAsync<System.ArgumentException>();
    }

    [Fact]
    public void GetReferralServiceUrl_ReturnsCorrectUrl()
    {
        // Arrange
        long serviceId = 123;
        var keys = new Dictionary<string, string>()
        {
            { "ConnectWeb", "https://somevalue" }
        };
        var familyHubsUiOptionsMock = new Mock<IOptions<FamilyHubsUiOptions>>();
        familyHubsUiOptionsMock.Setup(options => options.Value)
            .Returns(new FamilyHubsUiOptions { Urls = keys });

        var model = new RequestDetailsModel(Mock.Of<IReferralService>(), familyHubsUiOptionsMock.Object);

        // Act
        var url = model.GetReferralServiceUrl(serviceId);

        // Assert
        url.Should().Be("https://somevalue/ProfessionalReferral/LocalOfferDetail?serviceid=123");

    }
}
