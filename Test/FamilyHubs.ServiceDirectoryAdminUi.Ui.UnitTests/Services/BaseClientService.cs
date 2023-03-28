using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
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
    protected const long ORGANISATION_ID = 1;
    private const long SERVICE_ID = 1;
    private const string OWNER_SERVICE_ID = "1";

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
        var bristolCountyCouncil = new OrganisationWithServicesDto
        {
            Id = ORGANISATION_ID,
            OrganisationType = OrganisationType.LA,
            Name = "Unit Test County Council",
            Description = "Unit Test County Council",
            Uri = new Uri("https://www.unittest.gov.uk/").ToString(),
            Url = "https://www.unittest.gov.uk/",
            Services = new List<ServiceDto>
            {
                 GetTestCountyCouncilServicesDto(ORGANISATION_ID)
            },
            AdminAreaCode = "E1234"
        };

        return bristolCountyCouncil;
    }

    protected ServiceDto GetTestCountyCouncilServicesDto(long parentId)
    {
        var contactId = Guid.NewGuid().ToString();

        var location = new LocationDto
        {
            Id = 1,
            Latitude = 52.6312,
            Longitude =-1.66526,
            Address1 ="77 Sheepcote Lane",
            Address2 = ", Stathe, Tamworth, Staffordshire, ",
            PostCode = "B77 3JN",
            Country = "England",
            City = "Test",
            LocationType = LocationType.NotSet,
            Name = "Test",
            StateProvince = "Test",
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
            Id = SERVICE_ID,
            ServiceOwnerReferenceId = OWNER_SERVICE_ID,
            ServiceType = ServiceType.InformationSharing,
            OrganisationId = parentId,
            Name = "Unit Test Service",
            Description = @"Unit Test Service Description",
            Status = ServiceStatusType.Active,
            ServiceDeliveries = new List<ServiceDeliveryDto> { new ServiceDeliveryDto { Id = 1, ServiceId = SERVICE_ID, Name = ServiceDeliveryType.Online } },
            Eligibilities = new List<EligibilityDto> { new EligibilityDto { MinimumAge = 0, MaximumAge = 13, ServiceId = SERVICE_ID, EligibilityType = EligibilityType.Child, Id = 1 } },
            Contacts = new List<ContactDto> { new ContactDto { Id = 1, Name = "Contact", Telephone = "01827 65777", TextPhone = "01827 65777", ServiceId = SERVICE_ID, Email = "support@unittestservice.com", Url = "www.unittestservice.com" } },
            CostOptions = new List<CostOptionDto>(),
            Languages = new List<LanguageDto> { new LanguageDto { Id =1, Name = "English", ServiceId = SERVICE_ID } },
            ServiceAreas= new List<ServiceAreaDto> { new ServiceAreaDto { Id = 1, ServiceId = SERVICE_ID, Extent = "National", Uri = "http://statistics.data.gov.uk/id/statistical-geography/K02000001" } },
            Locations=new List<LocationDto> { location },
            Taxonomies = taxonomies
        };

        return service;
    }
}
