using FamilyHubs.ServiceDirectory.Shared.Helpers;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServices;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using static FamilyHubs.ServiceDirectoryAdminUi.Ui.Infrastructure.Configuration.TempStorageConfiguration;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;

public class RedisCacheService : IRedisCacheService
{
    private readonly IRedisCache _redisCache;
    private readonly int _timespanMinites;

    private readonly string _sessionId;
    private readonly IHttpContextAccessor? _httpContextAccessor;
    private readonly ISession _session;
    public RedisCacheService(IRedisCache redisCache, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _redisCache = redisCache;
        _timespanMinites = configuration.GetValue<int>("SessionTimeOutMinutes");

        _httpContextAccessor = httpContextAccessor;
        _session = _httpContextAccessor.HttpContext!.Session;
        _sessionId = _session.Id;
    }
    public OrganisationViewModel? RetrieveOrganisationWithService()
    {
        return _redisCache.GetValue<OrganisationViewModel>($"{_sessionId}{KeyOrgWithService}");
    }
    public void StoreOrganisationWithService(OrganisationViewModel? vm)
    {
        if (vm != null)
            _redisCache.SetValue($"{_sessionId}{KeyOrgWithService}", vm, _timespanMinites);
    }

    public void ResetOrganisationWithService()
    {
        _redisCache.SetStringValue($"{_sessionId}{KeyOrgWithService}", String.Empty, _timespanMinites);
    }

    public string RetrieveLastPageName()
    {
        return _redisCache.GetStringValue($"{_sessionId}{KeyCurrentPage}") ?? string.Empty;
    }

    public void StoreCurrentPageName(string? currPage)
    {
        if (currPage != null)
            _redisCache.SetStringValue($"{_sessionId}{KeyCurrentPage}", currPage, _timespanMinites);
    }

    public OpenReferralServiceDto? RetrieveService()
    {
        return _redisCache.GetValue<OpenReferralServiceDto>($"{_sessionId}{KeyService}");
    }

    public void StoreService(OpenReferralServiceDto serviceDto)
    {
        _redisCache.SetValue($"{_sessionId}{KeyService}", serviceDto, _timespanMinites);
    }

    public void StoreStringValue(string key, string value)
    {
        _redisCache.SetStringValue($"{_sessionId}{key}", value);
    }

    public string RetrieveStringValue(string key)
    {
        return _redisCache.GetStringValue($"{_sessionId}{key}") ?? string.Empty;
    }

    public void ResetStringValue(string key)
    {
        _redisCache.SetStringValue($"{_sessionId}{key}", string.Empty);
    }

    //user flow
    public string RetrieveUserFlow()
    {
        return _redisCache.GetStringValue($"{_sessionId}{KeyUserFlow}") ?? string.Empty;
    }

    public void StoreUserFlow(string userFlow)
    {
        _redisCache.SetStringValue($"{_sessionId}{KeyUserFlow}", userFlow, _timespanMinites);
    }

    public void ResetLastPageName()
    {
        _redisCache.SetStringValue($"{_sessionId}{KeyCurrentPage}", String.Empty, _timespanMinites);
    }
}
