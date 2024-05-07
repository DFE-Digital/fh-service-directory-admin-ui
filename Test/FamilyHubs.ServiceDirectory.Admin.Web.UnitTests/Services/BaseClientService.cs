using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using Moq;
using Moq.Protected;

// ReSharper disable StringLiteralTypo

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests.Services;

public class BaseClientService
{
    protected const long OrganisationId = 1;
    private const long ServiceId = 1;
    private const string OwnerServiceId = "1";

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

    protected OrganisationDetailsDto GetTestCountyCouncilDto()
    {
        var bristolCountyCouncil = new OrganisationDetailsDto
        {
            Id = OrganisationId,
            OrganisationType = OrganisationType.LA,
            Name = "Unit Test County Council",
            Description = "Unit Test County Council",
            Uri = new Uri("https://www.unittest.gov.uk/").ToString(),
            Url = "https://www.unittest.gov.uk/",
            Services = new List<ServiceDto>
            {
                GetTestCountyCouncilServicesDto(OrganisationId)
            },
            AdminAreaCode = "E1234"
        };

        return bristolCountyCouncil;
    }

    protected ServiceDto GetTestCountyCouncilServicesDto(long parentId)
    {
        var location = new LocationDto
        {
            Id = 1,
            Latitude = 52.6312,
            Longitude = -1.66526,
            Address1 = "77 Sheepcote Lane",
            Address2 = ", Stathe, Tamworth, Staffordshire, ",
            PostCode = "B77 3JN",
            Country = "GB",
            City = "Test",
            LocationTypeCategory = LocationTypeCategory.NotSet,
            Name = "Test",
            StateProvince = "Test",
            LocationType = LocationType.Postal,
            Contacts = new List<ContactDto> ()
        };

        var taxonomies = new List<TaxonomyDto>
        {
            new TaxonomyDto
            {
                Id = 1,
                Name = "Organisation",
                TaxonomyType = TaxonomyType.ServiceCategory
            },
            new TaxonomyDto
            {
                Id = 2,
                Name = "Support",
                TaxonomyType = TaxonomyType.ServiceCategory
            },
            new TaxonomyDto
            {
                Id = 3,
                Name = "Children",
                TaxonomyType = TaxonomyType.ServiceCategory
            },
            new TaxonomyDto
            {
                Id = 4,
                Name = "Long Term Health Conditions",
                TaxonomyType = TaxonomyType.ServiceCategory
            }
        };

        var service = new ServiceDto
        {
            Id = ServiceId,
            ServiceType = ServiceType.InformationSharing,
            OrganisationId = parentId,
            Name = "Unit Test Service",
            Description = "Unit Test Service Description",
            Status = ServiceStatusType.Active,
            ServiceDeliveries = new List<ServiceDeliveryDto> { new() { Id = 1, ServiceId = ServiceId, Name = AttendingType.Online } },
            Eligibilities = new List<EligibilityDto> { new() { MinimumAge = 0, MaximumAge = 13, ServiceId = ServiceId, EligibilityType = EligibilityType.Child, Id = 1 } },
            Contacts = new List<ContactDto> { new() { Id = 1, Name = "Contact", Telephone = "01827 65777", TextPhone = "01827 65777", ServiceId = ServiceId, Email = "support@unittestservice.com", Url = "www.unittestservice.com" } },
            CostOptions = new List<CostOptionDto>(),
            Languages = new List<LanguageDto> { new() { Id =1, Name = "English", Code = "en", ServiceId = ServiceId } },
            ServiceAreas = new List<ServiceAreaDto> { new() { Id = 1, ServiceId = ServiceId, Extent = "National", Uri = "http://statistics.data.gov.uk/id/statistical-geography/K02000001" } },
            Locations = new List<LocationDto> { location },
            Taxonomies = taxonomies
        };

        return service;
    }
}
