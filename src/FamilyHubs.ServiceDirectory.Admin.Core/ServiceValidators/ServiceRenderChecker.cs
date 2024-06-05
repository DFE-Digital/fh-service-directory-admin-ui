
namespace FamilyHubs.ServiceDirectory.Admin.Core.ServiceValidators
{
    //todo: options (not mutually exclusive): componentise service search result and render on details page
    // check 200 response
    // render using real service into iframe
    // use selenium (or similar) to check contents of rendered page

    public interface IServiceRenderChecker
    {
        Task<bool> CheckServiceRenderAsync(string url, CancellationToken cancellationToken);
    }

    public class ServiceRenderChecker : IServiceRenderChecker
    {
        internal const string HttpClientName = "render";

        private readonly IHttpClientFactory _httpClientFactory;

        public ServiceRenderChecker(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> CheckServiceRenderAsync(string url, CancellationToken cancellationToken)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(url, cancellationToken);

            return response.IsSuccessStatusCode;
        }
    }
}
