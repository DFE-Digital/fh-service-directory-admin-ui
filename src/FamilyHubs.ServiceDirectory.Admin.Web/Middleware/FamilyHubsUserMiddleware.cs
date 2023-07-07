// ReSharper disable UnusedMember.Global

using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Http;
using System.Net.Http;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Middleware;

public class FamilyHubsUserMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationMiddleware> _logger;

    public FamilyHubsUserMiddleware(RequestDelegate next, ILogger<CorrelationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ICacheService cacheService)
    {
        
        if (context.User != null && context.User.Identity.IsAuthenticated)
        {
            var cachedUserDetails = await cacheService.RetrieveFamilyHubsUser();
            if (cachedUserDetails == null)
            {

                var userDetails = context.GetFamilyHubsUser();
                if (!string.IsNullOrEmpty(userDetails.Email))
                {
                    await cacheService.StoreFamilyHubsUser(userDetails);
                }
            }
        }
        await _next(context);

    }
}