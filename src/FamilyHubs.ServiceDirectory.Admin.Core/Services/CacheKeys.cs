
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Http;

namespace FamilyHubs.ServiceDirectory.Admin.Core.Services;

public interface ICacheKeys
{
    string KeyOrgWithService { get; }
    string KeyUserPermission { get; }
    string KeyLaOrganisations { get; }
    string KeyCurrentPage { get; }
    string KeyService { get; }
    string KeyUserFlow { get; }
}

public class CacheKeys : ICacheKeys
{
    private readonly string _userId;

    public CacheKeys(IHttpContextAccessor httpContextAccessor)
    {
        _userId = httpContextAccessor.HttpContext!.GetFamilyHubsUser().Email;
    }

    public string KeyOrgWithService => SessionNamespaced("_OrgWithService");
    public string KeyUserPermission => SessionNamespaced("_UserPermission");
    public string KeyLaOrganisations => SessionNamespaced("_LaOrganisations");
    public string KeyCurrentPage => SessionNamespaced("_CurrentPage");
    public string KeyService => SessionNamespaced("_Service");
    public string KeyUserFlow => SessionNamespaced("_UserFlow");
    
    private string SessionNamespaced(string key)
    {
        return $"{_userId}{key}";
    }
}