
namespace FamilyHubs.ServiceDirectory.Admin.Core.ServiceValidators
{
    //todo: options (not mutually exclusive): componentise service search result and render on details page
    // check 200 response
    // render using real service into iframe
    // use selenium (or similar) to check contents of rendered page

    public enum RenderCheck
    {
        ConnectSearch,
        ConnectDetails,
        FindSearch
    }

    public record RenderCheckResult(RenderCheck RenderCheck, string Name, bool Passed, string ViewUrl);

    public interface IServiceRenderChecker
    {
        Task<RenderCheckResult> CheckRendering(
            RenderCheck renderCheck,
            long serviceId,
            CancellationToken cancellationToken);
    }

    public class ServiceRenderChecker : IServiceRenderChecker
    {
        internal const string HttpClientName = "render";

        private readonly IHttpClientFactory _httpClientFactory;

        public ServiceRenderChecker(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<RenderCheckResult> CheckRendering(
            RenderCheck renderCheck,
            long serviceId,
            CancellationToken cancellationToken)
        {
            string checkRequestUrl = GetRequestUrl(renderCheck, serviceId);

            return new RenderCheckResult(
                renderCheck,
                GetCheckDisplayName(renderCheck),
                await CheckServiceRenderAsync(checkRequestUrl, cancellationToken),
                checkRequestUrl);
        }

        private async Task<bool> CheckServiceRenderAsync(string checkRequestUrl, CancellationToken cancellationToken)
        {
            //todo: Connect would require either logging in some sort of service (dfe admin?) account

            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(checkRequestUrl, cancellationToken);

            return response.IsSuccessStatusCode;
        }

        private string GetRequestUrl(RenderCheck renderCheck, long serviceId)
        {
            return renderCheck switch
            {
                RenderCheck.ConnectSearch => $"https://connect-search/{serviceId}",
                RenderCheck.ConnectDetails => $"https://localhost:7270/ProfessionalReferral/LocalOfferDetail?serviceid={serviceId}",
                RenderCheck.FindSearch => $"https://localhost:7199/ServiceFilter?serviceId=1{serviceId}",
                _ => throw new ArgumentOutOfRangeException(nameof(renderCheck), renderCheck, null)
            };
        }

        private string GetCheckDisplayName(RenderCheck renderCheck)
        {
            return renderCheck switch
            {
                RenderCheck.ConnectSearch => "Connect search",
                RenderCheck.ConnectDetails => "Connect details",
                RenderCheck.FindSearch => "Find search",
                _ => throw new ArgumentOutOfRangeException(nameof(renderCheck), renderCheck, null)
            };
        }
    }
}
