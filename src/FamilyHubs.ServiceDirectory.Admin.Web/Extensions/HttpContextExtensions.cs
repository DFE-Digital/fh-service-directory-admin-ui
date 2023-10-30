using Microsoft.IdentityModel.Tokens;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Extensions
{
    public static class HttpContextExtensions
    {
        //todo: fix issue with this and have as fallback for when js is disabled?
        public static string GetBackButtonPath(this HttpContext httpContext)
        {

            var host = httpContext.GetHost();
            var path = httpContext.Request.Headers.Referer;

            if (path.IsNullOrEmpty())
            {
                return host;
            }

            return path.ToString().Substring(path.ToString().IndexOf(host) + host.Length);
        }

        private static string GetHost(this HttpContext httpContext)
        {
            var host = httpContext.Request.Host.Value;

            if (httpContext.Request.Headers.ContainsKey("X-Forwarded-Host"))
            {
                var xforwardedhost = httpContext.Request.Headers["X-Forwarded-Host"];
                if (!xforwardedhost.IsNullOrEmpty())
                {
                    host = xforwardedhost.ToString();
                }
            }

            return host;

        }
    }
}
