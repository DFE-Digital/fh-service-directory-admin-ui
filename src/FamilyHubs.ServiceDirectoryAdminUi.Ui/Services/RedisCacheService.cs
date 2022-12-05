using FamilyHubs.ServiceDirectory.Shared.Helpers;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServices;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using System;
using static FamilyHubs.ServiceDirectoryAdminUi.Ui.Infrastructure.Configuration.TempStorageConfiguration;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;

public class RedisCacheService : IRedisCacheService
{
    private readonly IRedisCache _redisCache;
    private readonly int _timespanMinites;

    public RedisCacheService(IRedisCache redisCache, IConfiguration configuration)
    {
        _redisCache = redisCache;
        _timespanMinites = configuration.GetValue<int>("SessionTimeOutMinutes");
    }
    public OrganisationViewModel? RetrieveOrganisationWithService()
    {
        return _redisCache.GetValue<OrganisationViewModel>(KeyOrgWithService);
    }
    public void StoreOrganisationWithService(OrganisationViewModel? vm)
    {
        if (vm != null)
            _redisCache.SetValue(KeyOrgWithService, vm, _timespanMinites);
    }

    public void ResetOrganisationWithService()
    {
        _redisCache.SetStringValue(KeyOrgWithService, String.Empty, _timespanMinites);
    }

    public string RetrieveLastPageName()
    {
        return _redisCache.GetStringValue(KeyCurrentPage) ?? string.Empty;
    }

    public void StoreCurrentPageName(string? currPage)
    {
        if (currPage != null)
            _redisCache.SetStringValue(KeyCurrentPage, currPage, _timespanMinites);
    }

    public OpenReferralServiceDto? RetrieveService()
    {
        return _redisCache.GetValue<OpenReferralServiceDto>(KeyService);
    }

    public void StoreService(OpenReferralServiceDto serviceDto)
    {
        _redisCache.SetValue<OpenReferralServiceDto>(KeyService, serviceDto, _timespanMinites);
    }

    public void StoreStringValue(string key, string value)
    {
        _redisCache.SetStringValue(key, value);
    }

    public string RetrieveStringValue(string key)
    {
        return _redisCache.GetStringValue(key) ?? string.Empty;
    }

    public void ResetStringValue(string key)
    {
        _redisCache.SetStringValue(key, string.Empty);
    }

    //user flow
    public string RetrieveUserFlow()
    {
        return _redisCache.GetStringValue(KeyUserFlow) ?? string.Empty;
    }

    public void StoreUserFlow(string userFlow)
    {
        _redisCache.SetStringValue(KeyUserFlow, userFlow, _timespanMinites);
    }

    public void ResetLastPageName()
    {
        _redisCache.SetStringValue(KeyCurrentPage, String.Empty, _timespanMinites);
    }
}
