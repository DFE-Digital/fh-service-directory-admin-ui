using FamilyHubs.ServiceDirectory.Shared.Builders;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralContacts;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralCostOptions;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralEligibilitys;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralHolidaySchedule;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralLanguages;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralLocations;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralOrganisations;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralPhones;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralPhysicalAddresses;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralRegularSchedule;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServiceAreas;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServiceAtLocations;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServiceDeliverysEx;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServices;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServiceTaxonomys;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralTaxonomys;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.UnitTests.Services;

public class BaseClientService
{
    protected HttpClient GetMockClient(string content)
    {
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                Content = new StringContent(content),
                StatusCode = HttpStatusCode.OK
            });

        var client = new HttpClient(mockHttpMessageHandler.Object);
        client.BaseAddress = new Uri("Https://Localhost");
        return client;
    }

    protected OpenReferralOrganisationWithServicesDto GetTestCountyCouncilDto()
    {
        var bristolCountyCouncil = new OpenReferralOrganisationWithServicesDto(
            "56e62852-1b0b-40e5-ac97-54a67ea957dc",
            "Unit Test County Council",
            "Unit Test County Council",
            null,
            new Uri("https://www.unittest.gov.uk/").ToString(),
            "https://www.unittest.gov.uk/",
            new List<OpenReferralServiceDto>
            {
                 GetTestCountyCouncilServicesDto()
            }
            );

        return bristolCountyCouncil;
    }

    protected OpenReferralServiceDto GetTestCountyCouncilServicesDto()
    {
        var contactId = Guid.NewGuid().ToString();

        ServicesDtoBuilder builder = new ServicesDtoBuilder();
        OpenReferralServiceDto service = builder.WithMainProperties("3010521b-6e0a-41b0-b610-200edbbeeb14",
                "Unit Test Service",
                @"Unit Test Service Description",
                null,
                null,
                null,
                null,
                null,
                "active",
                "www.unittestservice.com",
                "support@unittestservice.com",
                null)
            .WithServiceDelivery(new List<OpenReferralServiceDeliveryExDto>
                {
                    new OpenReferralServiceDeliveryExDto(Guid.NewGuid().ToString(),ServiceDelivery.Online)
                })
            .WithEligibility(new List<OpenReferralEligibilityDto>
                {
                    new OpenReferralEligibilityDto("Test9111Children","",0,13)
                })
            .WithContact(new List<OpenReferralContactDto>()
            {
                new OpenReferralContactDto(
                    contactId,
                    "Contact",
                    string.Empty,
                    new List<OpenReferralPhoneDto>()
                    {
                        new OpenReferralPhoneDto("1567", "01827 65777")
                    }
                    )
            })
            .WithCostOption(new List<OpenReferralCostOptionDto>())
            .WithLanguages(new List<OpenReferralLanguageDto>()
                {
                    new OpenReferralLanguageDto("1bb6c313-648d-4226-9e96-b7d37eaeb3dd", "English")
                })
            .WithServiceAreas(new List<OpenReferralServiceAreaDto>()
                {
                    new OpenReferralServiceAreaDto(Guid.NewGuid().ToString(), "National", null,"http://statistics.data.gov.uk/id/statistical-geography/K02000001")
                })
            .WithServiceAtLocations(new List<OpenReferralServiceAtLocationDto>()
                {
                    new OpenReferralServiceAtLocationDto(
                        "Test1749",
                        new OpenReferralLocationDto(
                            "6ea31a4f-7dcc-4350-9fba-20525efe092f",
                            "",
                            "",
                            52.6312,
                            -1.66526,
                            new List<OpenReferralPhysicalAddressDto>()
                            {
                                new OpenReferralPhysicalAddressDto(
                                    Guid.NewGuid().ToString(),
                                    "77 Sheepcote Lane",
                                    ", Stathe, Tamworth, Staffordshire, ",
                                    "B77 3JN",
                                    "England",
                                    null
                                    )
                            }
                            //new List<Accessibility_For_Disabilities>()
                            ),
                            new List<OpenReferralRegularScheduleDto>(),
                            new List<OpenReferralHolidayScheduleDto>()
                        )

                })
            .WithServiceTaxonomies(new List<OpenReferralServiceTaxonomyDto>()
                {
                    new OpenReferralServiceTaxonomyDto
                    ("UnitTest9107",
                    new OpenReferralTaxonomyDto(
                        "UnitTest bccsource:Organisation",
                        "Organisation",
                        "Test BCC Data Sources",
                        null
                        )),

                    new OpenReferralServiceTaxonomyDto
                    ("UnitTest9108",
                    new OpenReferralTaxonomyDto(
                        "UnitTest bccprimaryservicetype:38",
                        "Support",
                        "Test BCC Primary Services",
                        null
                        )),

                    new OpenReferralServiceTaxonomyDto
                    ("UnitTest9109",
                    new OpenReferralTaxonomyDto(
                        "UnitTest bccagegroup:37",
                        "Children",
                        "Test BCC Age Groups",
                        null
                        )),

                    new OpenReferralServiceTaxonomyDto
                    ("UnitTest9110",
                    new OpenReferralTaxonomyDto(
                        "UnitTestbccusergroup:56",
                        "Long Term Health Conditions",
                        "Test BCC User Groups",
                        null
                        ))
                })
            .Build();

        return service;
    }
}
