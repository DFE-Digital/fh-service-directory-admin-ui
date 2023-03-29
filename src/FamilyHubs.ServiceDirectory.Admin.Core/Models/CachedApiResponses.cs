using FamilyHubs.ServiceDirectory.Shared.Dto;

namespace FamilyHubs.ServiceDirectory.Admin.Core.Models;

public class CachedApiResponses
{
    public List<TaxonomyDto> Taxonomies { get; } = new List<TaxonomyDto>();
    public List<OrganisationDto> Organisations { get; set; } = new List<OrganisationDto>();
    public List<OrganisationWithServicesDto> OrganisationsWithServices { get; } = new List<OrganisationWithServicesDto>();
}