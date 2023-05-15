using Microsoft.AspNetCore.Authorization;

namespace FamilyHubs.ServiceDirectory.Admin.Web;

public class DfeAdminAuthorizationPolicy : AuthorizationPolicy
{
    public DfeAdminAuthorizationPolicy(IEnumerable<IAuthorizationRequirement> requirements, IEnumerable<string> authenticationSchemes) : base(requirements, authenticationSchemes)
    {
        
    }
}