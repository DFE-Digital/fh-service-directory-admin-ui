using AutoMapper;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralContacts;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralCostOptions;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralEligibilitys;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralLanguages;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralLocations;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralOrganisations;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralPhones;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralPhysicalAddresses;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServiceAreas;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServiceAtLocations;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServiceDeliverysEx;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServices;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServiceTaxonomys;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralTaxonomys;
using fh_service_directory_api.core.Entities;

namespace fh_service_directory_api.core;

public class AutoMappingProfiles : Profile
{
    public AutoMappingProfiles()
    {
        CreateMap<OpenReferralContactDto, OpenReferralContact>();
        CreateMap<OpenReferralCostOptionDto, OpenReferralCost_Option>();
        CreateMap<OpenReferralEligibilityDto, OpenReferralEligibility>();
        CreateMap<OpenReferralLanguageDto, OpenReferralLanguage>();
        CreateMap<OpenReferralLocationDto, OpenReferralLocation>();
        CreateMap<OpenReferralOrganisationWithServicesDto, OpenReferralOrganisation>();
        CreateMap<OpenReferralPhoneDto, OpenReferralPhone>();
        CreateMap<OpenReferralPhysicalAddressDto, OpenReferralPhysical_Address>();
        CreateMap<OpenReferralServiceAreaDto, OpenReferralService_Area>();
        CreateMap<OpenReferralServiceTaxonomyDto, OpenReferralService_Taxonomy>();
        CreateMap<OpenReferralServiceAtLocationDto, OpenReferralServiceAtLocation>();
        CreateMap<OpenReferralServiceDeliveryExDto, OpenReferralServiceDelivery>();
        CreateMap<OpenReferralServiceDto, OpenReferralService>();
        CreateMap<OpenReferralTaxonomyDto, OpenReferralTaxonomy>();

    }
}
