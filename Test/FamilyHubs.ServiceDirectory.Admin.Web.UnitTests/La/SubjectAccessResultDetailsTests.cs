using FamilyHubs.ReferralService.Shared.Dto;
using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.La;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests.La;

public class SubjectAccessResultDetailsTests
{
    private readonly Mock<IRequestDistributedCache> _mockRequestDistributedCache;
    private readonly Mock<IReferralService> _mockReferralService;
    private readonly SubjectAccessResultDetailsModel _subjectAccessResultDetailsModel;

    public SubjectAccessResultDetailsTests()
    {
        var settings = new Dictionary<string, string>
            {
                { "GovUkOidcConfiguration:AppHost", "https://localhost:7216" }
            };
        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection((IEnumerable<KeyValuePair<string, string?>>)settings).Build();

        _mockRequestDistributedCache = new Mock<IRequestDistributedCache>();
        _mockReferralService = new Mock<IReferralService>();
        DefaultHttpContext httpContext = new DefaultHttpContext();
        _subjectAccessResultDetailsModel = new SubjectAccessResultDetailsModel(_mockRequestDistributedCache.Object, _mockReferralService.Object, configuration);
        _subjectAccessResultDetailsModel.PageContext.HttpContext = httpContext;
        CompiledPageActionDescriptor compiledPageActionDescriptor = new();
        _subjectAccessResultDetailsModel.PageContext.ActionDescriptor = compiledPageActionDescriptor;
        _subjectAccessResultDetailsModel.PageContext.ActionDescriptor.DisplayName = "/La/SubjectAccessResultDetails";
    }

    [Fact]
    public async Task ThenSubjectAccessResultDetailsOnGetIsSuccessfull()
    {
        //Arrange
        int callback = 0;
        _mockRequestDistributedCache.Setup(x => x.GetAsync<SubjectAccessRequestViewModel>(It.IsAny<string>()))
            .Callback(() => callback++)
            .ReturnsAsync(new SubjectAccessRequestViewModel
            {
                SelectionType = "email", Value1 = "TestUser@email.com"
            });
        _mockReferralService.Setup(x => x.GetReferralsByRecipient(It.IsAny<Core.Models.SubjectAccessRequestViewModel>())).ReturnsAsync(GetReferralList());

        //Act
        await _subjectAccessResultDetailsModel.OnGet("", SharedKernel.Razor.Dashboard.SortOrder.none);

        // Assert
        callback.Should().Be(1);
        _subjectAccessResultDetailsModel.ReferralDtos.Should().BeEquivalentTo(GetReferralList());
    }

    [Fact]
    public async Task ThenSubjectAccessResultDetailsOnGetJustRetruns()
    {
        //Arrange
        int callback = 0;
        _mockRequestDistributedCache.Setup(x => x.GetAsync<SubjectAccessRequestViewModel>(It.IsAny<string>()))
            .Callback(() => callback++);

        //Act
        await _subjectAccessResultDetailsModel.OnGet("", SharedKernel.Razor.Dashboard.SortOrder.none);

        // Assert
        callback.Should().Be(1);
        _subjectAccessResultDetailsModel.ReferralDtos.Should().BeEquivalentTo(new List<ReferralDto>());
    }

    public static List<ReferralDto> GetReferralList()
    {
        List<ReferralDto> listReferrals = new()
        {
            new ReferralDto
            {
                ReferrerTelephone = "0121 555 7777",
                ReasonForSupport = "Reason For Support",
                EngageWithFamily = "Engage With Family",
                RecipientDto = new RecipientDto
                {
                    Name = "Test User",
                    Email = "TestUser@email.com",
                    Telephone = "078873456",
                    TextPhone = "078873456",
                    AddressLine1 = "Address Line 1",
                    AddressLine2 = "Address Line 2",
                    TownOrCity = "Birmingham",
                    County = "Country",
                    PostCode = "B30 2TV"
                },
                ReferralUserAccountDto = new UserAccountDto
                {
                    Id = 5,
                    EmailAddress = "Joe.Professional@email.com",
                    Name = "Joe Professional",
                    PhoneNumber = "011 222 3333",
                    Team = "Social Work team North"
                },
                Status = new ReferralStatusDto
                {
                    Id = 1,
                    Name = "New",
                    SortOrder = 1,
                    SecondrySortOrder = 0,
                },
                ReferralServiceDto = new ReferralServiceDto
                {
                    Id = 1,
                    Name = "Test Service",
                    Description = "Test Service Description",
                    Url = "www.TestService.com",
                    OrganisationDto = new OrganisationDto
                    {
                        Id = 1,
                        ReferralServiceId = 1,
                        Name = "Test Organisation",
                        Description = "Test Organisation Description",
                    }
                }
            },
        };

        return listReferrals;
    }
}
