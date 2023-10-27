using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.SharedKernel.Razor.FamilyHubsUi.Options;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.La;
using Xunit;

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests.La;

public class RequestDetailsTests
{
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
