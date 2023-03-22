using FamilyHubs.ServiceDirectory.Shared.Dto;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.DataUpload.Models
{
    public class CachedApiResponses
    {
        public List<TaxonomyDto> Taxonomies { get;} = new ();
        public List<OrganisationDto> Organisations { get; set; } = new();
        public List<OrganisationWithServicesDto> OrganisationsWithServices { get; set; } = new();
    }
}
