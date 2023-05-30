namespace FamilyHubs.ServiceDirectory.Admin.Core.Models;

public class PermissionModel
{
    public string OrganisationType { get; set; } = string.Empty;
    
    public bool LaAdmin { get; set; }
    public bool LaProfessional { get; set; }
    
    public bool LaJourney => LaAdmin || LaProfessional;
    
    public bool VcsAdmin { get; set; }
    public bool VcsProfessional { get; set; }
    public bool VcsJourney => VcsAdmin || VcsProfessional;

    public string EmailAddress { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    
    public long LaOrganisationId { get; set; }
    public long VcsOrganisationId { get; set; }
    
    public string LaOrganisationName { get; set; } = string.Empty;
    
    public string VcsOrganisationName { get; set; } = string.Empty;
}