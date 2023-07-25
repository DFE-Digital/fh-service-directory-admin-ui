// ReSharper disable UnusedMember.Global

using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.SharedKernel.Identity;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Middleware;

public class CorrelationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationMiddleware> _logger;

    public CorrelationMiddleware(RequestDelegate next, ILogger<CorrelationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ICorrelationService correlationService)
    {

        var user = context.GetFamilyHubsUser();
        var userIdentifier = "Anonymous";
        if (!string.IsNullOrEmpty(user.Email))
        {
            userIdentifier = user.Email;
        }

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationService.CorrelationId,
            ["UserIdentifier"] = userIdentifier
        }))
        {
            await _next(context);
        }
    }
}