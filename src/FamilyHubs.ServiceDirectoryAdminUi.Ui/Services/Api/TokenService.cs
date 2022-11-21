using Microsoft.Extensions.Caching.Memory;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;

public interface ITokenService
{
    string GetToken();
    string GetRefreshToken();
    void SetToken(string tokenValue, DateTime validTo, string refreshToken);
    void ClearTokens();
}

public class TokenService : ITokenService
{
    private readonly IMemoryCache _memoryCache;

    public TokenService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public void SetToken(string tokenValue, DateTime validTo, string refreshToken)
    {
        if (string.IsNullOrEmpty(tokenValue) || string.IsNullOrEmpty(refreshToken))
            return;

        //api seems to be subtracting 60 mins
        TimeZoneInfo tzi = TimeZoneInfo.Local;
        TimeSpan offset = tzi.GetUtcOffset(validTo);
        DateTime validDate = DateTime.Now.Add(offset);

        if (!DateTime.Now.IsDaylightSavingTime())
        {
            validDate = validDate.AddHours(1);
        }

        TimeSpan ts = validDate - DateTime.Now;

        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(ts); // TimeSpan.FromSeconds(3));

        _memoryCache.Set("FamilyHubToken", tokenValue, cacheEntryOptions);
        _memoryCache.Set("FamilyHubRefreshToken", refreshToken, cacheEntryOptions);
    }

    public string GetToken()
    {
        if (_memoryCache.TryGetValue("FamilyHubToken", out string cacheValue))
        {
            return cacheValue;
        }

        return string.Empty;
    }

    public string GetRefreshToken()
    {
        if (_memoryCache.TryGetValue("FamilyHubRefreshToken", out string cacheValue))
        {
            return cacheValue;
        }

        return string.Empty;
    }

    public void ClearTokens()
    {
        _memoryCache.Remove("FamilyHubToken");
        _memoryCache.Remove("FamilyHubRefreshToken");
    }
}
