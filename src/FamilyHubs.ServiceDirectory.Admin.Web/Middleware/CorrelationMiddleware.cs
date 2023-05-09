// ReSharper disable UnusedMember.Global

using FamilyHubs.ServiceDirectory.Admin.Core.Services;

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

        //****************TEMP CODE - DO NOT COMPLETE PR
        foreach(var header in context.Request.Headers)
        {
            _logger.LogInformation($"Header - {header.Key}:{header.Value}");//  Temp Code
        }
        //****************TEMP CODE - DO NOT COMPLETE PR


        using (_logger.BeginScope(new Dictionary<string, object>
               {
                   ["CorrelationId"] = correlationService.CorrelationId
                }))
        {
            await _next(context);
        }            
    }
}