using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace FamilyHubs.ServiceDirectory.Admin.Core.Services;
public interface ICacheService
{
    public OrganisationViewModel? RetrieveOrganisationWithService();
    public void StoreOrganisationWithService(OrganisationViewModel? vm);
    public void ResetOrganisationWithService();
    
    public string RetrieveUserFlow();
    public void StoreUserFlow(string userFlow);
    
    public string RetrieveLastPageName();
    public void StoreCurrentPageName(string? currentPage);
    public void ResetLastPageName();
}

public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _timeSpanMinutes;

    private readonly string _sessionId;

    private const string KeyOrgWithService = "_OrgWithService";
    private const string KeyCurrentPage = "_CurrentPage";
    private const string KeyService = "_Service";
    private const string KeyUserFlow = "_UserFlow";

    public CacheService(IMemoryCache cache, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _cache = cache;
        var timeoutValue = configuration.GetValue<int?>("SessionTimeOutMinutes");
        ArgumentNullException.ThrowIfNull(timeoutValue);
        
        _timeSpanMinutes = TimeSpan.FromMinutes(timeoutValue.Value) ;

        var session = httpContextAccessor.HttpContext!.Session;
        _sessionId = session.Id;
    }
    
    public OrganisationViewModel? RetrieveOrganisationWithService()
    {
        return _cache.Get<OrganisationViewModel>($"{_sessionId}{KeyOrgWithService}");
    }
    
    public void StoreOrganisationWithService(OrganisationViewModel? vm)
    {
        if (vm != null)
            _cache.Set($"{_sessionId}{KeyOrgWithService}", vm, _timeSpanMinutes);
    }

    public void ResetOrganisationWithService()
    {
        _cache.Remove($"{_sessionId}{KeyOrgWithService}");
    }

    
    public string RetrieveLastPageName()
    {
        return _cache.Get($"{_sessionId}{KeyCurrentPage}")?.ToString() ?? string.Empty;
    }

    public void StoreCurrentPageName(string? currentPage)
    {
        if (currentPage != null)
            _cache.Set($"{_sessionId}{KeyCurrentPage}", currentPage, _timeSpanMinutes);
    }

    public void ResetLastPageName()
    {
        _cache.Remove($"{_sessionId}{KeyCurrentPage}");
    }    
    
    
    public void StoreService(ServiceDto serviceDto)
    {
        _cache.Set($"{_sessionId}{KeyService}", serviceDto, _timeSpanMinutes);
    }
    
    public string RetrieveUserFlow()
    {
        return _cache.Get($"{_sessionId}{KeyUserFlow}")?.ToString() ?? string.Empty;
    }

    public void StoreUserFlow(string userFlow)
    {
        _cache.Set($"{_sessionId}{KeyUserFlow}", userFlow, _timeSpanMinutes);
    }
}
