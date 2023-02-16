using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Middleware
{
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
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationService.CorrelationId
            }))
            {
                await _next(context);
            }            
        }
    }
}
