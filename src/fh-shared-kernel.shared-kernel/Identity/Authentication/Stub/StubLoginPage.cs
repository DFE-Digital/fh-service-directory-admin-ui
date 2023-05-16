using FamilyHubs.SharedKernel.GovLogin.Configuration;
using Microsoft.AspNetCore.Http;
using System.Reflection;
using System.Text;

namespace FamilyHubs.SharedKernel.Identity.Authentication.Stub
{
    /// <summary>
    /// Handles returning a stubbed login page. 
    /// Note- StubLoginPage.html must not be renamed or moved to a different location than this class
    /// </summary>
    internal static class StubLoginPage
    {
        internal static bool ShouldRedirectToStubLoginPage(HttpContext context)
        {
            if (context.Request.Path.HasValue && context.Request.Path.Value.Contains(StubConstants.LoginPagePath))
            {
                return true;
            }

            return false;
        }

        internal static async Task RenderStubLoginPage(HttpContext context, GovUkOidcConfiguration configuration)
        {
            var html = GetLoginHtml();
            html = AddRoleOptions(context, configuration, html);
            var bytes = Encoding.UTF8.GetBytes(html);
            context.Response.ContentType = "text/html";
            await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
        }

        private static string AddRoleOptions(HttpContext context, GovUkOidcConfiguration configuration, string html)
        {
            var stubUsers = configuration.GetStubUsers();
            var redirectUrl = GetRedirectUrlFromPath(context);
            var body = string.Empty;

            foreach (var user in stubUsers)
            {
                var href = $"{StubConstants.RoleSelectedPath}?user={user.User.Email}&redirect={redirectUrl}";
                body += $"<a href=\"{href}\"><button>{user.User.Email}</button></a>";
            }

            return html.Replace("@RenderBody", body);
        }

        private static string GetLoginHtml()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var nameSpace = typeof(StubLoginPage).Namespace;

            using (var stream = assembly.GetManifestResourceStream($"{nameSpace}.StubLoginPage.html"))
            using (StreamReader reader = new StreamReader(stream!))
            {
                string result = reader.ReadToEnd();
                return result;
            }
        }

        /// <summary>
        /// This resolves the url to send the browser to after the stubUser has been selected
        /// The url will be in the path as in this example -> https://localhost//account/stub/loginpage/<ReturnsThis(urlencoded)>
        /// </summary>
        private static string GetRedirectUrlFromPath(HttpContext context)
        {
            var path = context.Request.Path.Value!;
            return path.Substring(path.IndexOf(StubConstants.LoginPagePath) + StubConstants.LoginPagePath.Length);
        }
    }
}
