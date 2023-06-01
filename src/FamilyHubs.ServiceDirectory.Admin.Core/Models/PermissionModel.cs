namespace FamilyHubs.ServiceDirectory.Admin.Core.Models;

public class PermissionModel
{
    public string OrganisationType { get; set; } = string.Empty;
    
    public bool LaManager { get; set; }
    public bool LaProfessional { get; set; }
    
    public bool LaJourney => LaManager || LaProfessional;
    
    public bool VcsManager { get; set; }
    public bool VcsProfessional { get; set; }
    public bool VcsJourney => VcsManager || VcsProfessional;

    public string EmailAddress { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    
    public long LaOrganisationId { get; set; }
    public long VcsOrganisationId { get; set; }
    
    public string LaOrganisationName { get; set; } = string.Empty;
    
    public string VcsOrganisationName { get; set; } = string.Empty;
}