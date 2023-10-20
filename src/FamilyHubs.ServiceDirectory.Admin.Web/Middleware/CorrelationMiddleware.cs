// ReSharper disable UnusedMember.Global

using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages;
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
        var userEmail = "Anonymous";
        if (!string.IsNullOrEmpty(user.Email))
        {
            userEmail = ObfuscateEmail(user.Email);
        }

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationService.CorrelationId,
            ["UserIdentifier"] = userEmail,
            ["AccountId"] = user.AccountId,
            ["UserRole"] = user.Role,
            ["UserOrganisationId"] = user.OrganisationId,
            ["CorrelationTime"] = DateTime.UtcNow.Ticks
        }))
        {
            await _next(context);
        }
    }

    public static string ObfuscateEmail(string email)
    {
        int atIndex = email.IndexOf('@');
        if (atIndex < 0)
        {
            return "InvalidEmail";
        }

        // Extract the first two characters and everything after the '@' symbol
        string firstTwoChars = email.Substring(0, Math.Min(2, atIndex));
        string afterAt = email.Substring(atIndex);

        return $"{firstTwoChars}***{afterAt}";
    }
}