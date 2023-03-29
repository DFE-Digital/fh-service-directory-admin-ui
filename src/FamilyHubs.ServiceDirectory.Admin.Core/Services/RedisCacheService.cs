using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace FamilyHubs.ServiceDirectory.Admin.Core.Services;
public interface IRedisCacheService
{
    public OrganisationViewModel? RetrieveOrganisationWithService();
    public void StoreOrganisationWithService(OrganisationViewModel? vm);
    public void ResetOrganisationWithService();
    public string RetrieveUserFlow();
    public void StoreUserFlow(string userFlow);
    public string RetrieveLastPageName();
    public void StoreCurrentPageName(string? currentPage);
    public void ResetLastPageName();
    void StoreStringValue(string key, string value);
}

public class RedisCacheService : IRedisCacheService
{
    private readonly IRedisCache _redisCache;
    private readonly int _timeSpanMinutes;

    private readonly string _sessionId;

    private const string KeyOrgWithService = "_OrgWithService";
    private const string KeyCurrentPage = "_CurrentPage";
    private const string KeyService = "_Service";
    private const string KeyUserFlow = "_UserFlow";

    public RedisCacheService(IRedisCache redisCache, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _redisCache = redisCache;
        _timeSpanMinutes = configuration.GetValue<int>("SessionTimeOutMinutes");

        var session = httpContextAccessor.HttpContext!.Session;
        _sessionId = session.Id;
    }
    public OrganisationViewModel? RetrieveOrganisationWithService()
    {
        return _redisCache.GetValue<OrganisationViewModel>($"{_sessionId}{KeyOrgWithService}");
    }
    public void StoreOrganisationWithService(OrganisationViewModel? vm)
    {
        if (vm != null)
            _redisCache.SetValue($"{_sessionId}{KeyOrgWithService}", vm, _timeSpanMinutes);
    }

    public void ResetOrganisationWithService()
    {
        _redisCache.SetStringValue($"{_sessionId}{KeyOrgWithService}", string.Empty, _timeSpanMinutes);
    }

    public string RetrieveLastPageName()
    {
        return _redisCache.GetStringValue($"{_sessionId}{KeyCurrentPage}") ?? string.Empty;
    }

    public void StoreCurrentPageName(string? currentPage)
    {
        if (currentPage != null)
            _redisCache.SetStringValue($"{_sessionId}{KeyCurrentPage}", currentPage, _timeSpanMinutes);
    }

    public void StoreService(ServiceDto serviceDto)
    {
        _redisCache.SetValue($"{_sessionId}{KeyService}", serviceDto, _timeSpanMinutes);
    }

    public void StoreStringValue(string key, string value)
    {
        _redisCache.SetStringValue($"{_sessionId}{key}", value);
    }

    public string RetrieveUserFlow()
    {
        return _redisCache.GetStringValue($"{_sessionId}{KeyUserFlow}") ?? string.Empty;
    }

    public void StoreUserFlow(string userFlow)
    {
        _redisCache.SetStringValue($"{_sessionId}{KeyUserFlow}", userFlow, _timeSpanMinutes);
    }

    public void ResetLastPageName()
    {
        _redisCache.SetStringValue($"{_sessionId}{KeyCurrentPage}", string.Empty, _timeSpanMinutes);
    }
}
