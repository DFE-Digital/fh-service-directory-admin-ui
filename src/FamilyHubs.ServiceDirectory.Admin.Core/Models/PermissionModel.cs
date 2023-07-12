namespace FamilyHubs.ServiceDirectory.Admin.Core.Models;

public class PermissionModel
{
    public string OrganisationType { get; set; } = string.Empty;

    public bool LaDualRole => LaManager && LaProfessional;
    public bool LaManager { get; set; }
    public bool LaProfessional { get; set; }
    public bool LaJourney => LaManager || LaProfessional;
    
    public bool VcsDualRole => VcsManager && VcsProfessional;
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

public class AccountDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public List<AccountClaimDto> Claims { get; set; } = new List<AccountClaimDto>();
}

public class AccountClaimDto
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class UpdateAccountDto
{
    public required long AccountId { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
}

public class UpdateClaimDto
{
    public required long AccountId { get; set; }
    public required string Name { get; set; }
    public required string Value { get; set; }
}