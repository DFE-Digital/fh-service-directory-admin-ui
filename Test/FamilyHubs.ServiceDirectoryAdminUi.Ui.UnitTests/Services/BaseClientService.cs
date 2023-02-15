using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FamilyHubs.ServiceDirectory.Shared.Builders;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using Moq;
using Moq.Protected;

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

    protected OrganisationWithServicesDto GetTestCountyCouncilDto()
    {
        var bristolCountyCouncil = new OrganisationWithServicesDto(
            "56e62852-1b0b-40e5-ac97-54a67ea957dc",
            new OrganisationTypeDto("1", "LA", "Local Authority"),
            "Unit Test County Council",
            "Unit Test County Council",
            null,
            new Uri("https://www.unittest.gov.uk/").ToString(),
            "https://www.unittest.gov.uk/",
            new List<ServiceDto>
            {
                 GetTestCountyCouncilServicesDto("56e62852-1b0b-40e5-ac97-54a67ea957dc")
            }
            );

        return bristolCountyCouncil;
    }

    protected ServiceDto GetTestCountyCouncilServicesDto(string parentId)
    {
        var contactId = Guid.NewGuid().ToString();
        var serviceId = "3010521b-6e0a-41b0-b610-200edbbeeb14";

        var builder = new ServicesDtoBuilder();
        var service = builder.WithMainProperties(serviceId,
                new ServiceTypeDto("1", "Information Sharing", ""),
                parentId,
                "Unit Test Service",
                @"Unit Test Service Description",
                null,
                null,
                null,
                null,
                null,
                "active",
                null,
                false)
            .WithServiceDelivery(new List<ServiceDeliveryDto>
                {
                    new ServiceDeliveryDto(Guid.NewGuid().ToString(),ServiceDeliveryType.Online)
                })
            .WithEligibility(new List<EligibilityDto>
                {
                    new EligibilityDto("Test9111Children","",0,13)
                })
            .WithLinkContact(new List<LinkContactDto>
            {
                new LinkContactDto(
                    Guid.NewGuid().ToString(),
                    "online",
                    serviceId,
                    new ContactDto(
                        contactId,
                        "Contact",
                        string.Empty,
                        "01827 65777",
                        "01827 65777",
                        "www.unittestservice.com",
                        "support@unittestservice.com"
                    )
                )

            })
            .WithCostOption(new List<CostOptionDto>())
            .WithLanguages(new List<LanguageDto>
            {
                    new LanguageDto("1bb6c313-648d-4226-9e96-b7d37eaeb3dd", "English")
                })
            .WithServiceAreas(new List<ServiceAreaDto>
            {
                    new ServiceAreaDto(Guid.NewGuid().ToString(), "National", null,"http://statistics.data.gov.uk/id/statistical-geography/K02000001")
                })
            .WithServiceAtLocations(new List<ServiceAtLocationDto>
            {
                    new ServiceAtLocationDto(
                        "Test1749",
                        new LocationDto(
                            "6ea31a4f-7dcc-4350-9fba-20525efe092f",
                            "",
                            "",
                            52.6312,
                            -1.66526,
                            new List<PhysicalAddressDto>
                            {
                                new PhysicalAddressDto(
                                    Guid.NewGuid().ToString(),
                                    "77 Sheepcote Lane",
                                    ", Stathe, Tamworth, Staffordshire, ",
                                    "B77 3JN",
                                    "England",
                                    null
                                    )
                            },
                            null,
                            new List<LinkContactDto>()
                            ),
                            new List<RegularScheduleDto>(),
                            new List<HolidayScheduleDto>(),
                            new List<LinkContactDto>()
                        )

            })
            .WithServiceTaxonomies(new List<ServiceTaxonomyDto>
            {
                    new ServiceTaxonomyDto
                    ("UnitTest9107",
                    new TaxonomyDto(
                        "UnitTest bccsource:Organisation",
                        "Organisation",
                        TaxonomyType.ServiceCategory,
                        null
                        )),

                    new ServiceTaxonomyDto
                    ("UnitTest9108",
                    new TaxonomyDto(
                        "UnitTest bccprimaryservicetype:38",
                        "Support",
                        TaxonomyType.ServiceCategory,
                        null
                        )),

                    new ServiceTaxonomyDto
                    ("UnitTest9109",
                    new TaxonomyDto(
                        "UnitTest bccagegroup:37",
                        "Children",
                        TaxonomyType.ServiceCategory,
                        null
                        )),

                    new ServiceTaxonomyDto
                    ("UnitTest9110",
                    new TaxonomyDto(
                        "UnitTestbccusergroup:56",
                        "Long Term Health Conditions",
                        TaxonomyType.ServiceCategory,
                        null
                        ))
                })
            .Build();

        return service;
    }
}
