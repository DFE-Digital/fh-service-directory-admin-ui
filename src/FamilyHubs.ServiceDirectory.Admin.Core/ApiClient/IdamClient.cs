using FamilyHubs.ServiceDirectory.Admin.Core.Exceptions;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Shared.Models;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Text;
using System.Web;

namespace FamilyHubs.ServiceDirectory.Admin.Core.ApiClient
{
    public interface IIdamClient
    {
        public Task AddAccount(AccountDto accountDto);
        public Task<AccountDto?> GetAccount(string email);
        public Task<PaginatedList<AccountDto>?> GetAccounts(
            long organisationId, int pageNumber, string? userName = null, string? email = null, string? organisationName = null, bool? isLaUser = null, bool? isVcsUser = null, string? sortBy = null);
    }

    public class IdamClient : ApiService, IIdamClient
    {
        public IdamClient(HttpClient client) : base(client)
        {

        }

        public async Task AddAccount(AccountDto accountDto)
        {
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri(Client.BaseAddress + "api/account");
            request.Content = new StringContent(JsonConvert.SerializeObject(accountDto), Encoding.UTF8, "application/json");

            using var response = await Client.SendAsync(request);

            await ValidateResponse(response);

            return;
        }

        public async Task<AccountDto?> GetAccount(string email)
        {
            var filter = $"?email={HttpUtility.UrlEncode(email)}";

            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri(Client.BaseAddress + $"api/account{filter}");

            using var response = await Client.SendAsync(request);

            await ValidateResponse(response);

            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return null;
            }

            var account = await response.Content.ReadFromJsonAsync<AccountDto>();
            return account;
        }

        public async Task<PaginatedList<AccountDto>?> GetAccounts(
            long organisationId, 
            int pageNumber, 
            string? userName = null,
            string? email = null,
            string? organisationName = null,
            bool? isLaUser = null, 
            bool? isVcsUser = null,
            string? sortBy = null)
        {
            var filters = $"?pageSize=10&pageNumber={pageNumber}";

            if (!string.IsNullOrEmpty(userName))
                filters += $"&userName={userName}";

            if (!string.IsNullOrEmpty(email))
                filters += $"&email={email}";

            if (!string.IsNullOrEmpty(organisationName))
                filters += $"&organisationName={organisationName}";

            if (isLaUser.HasValue)
                filters += $"&isLaUser={isLaUser}";

            if (isVcsUser.HasValue)
                filters += $"&isVcsUser={isVcsUser}";

            if (!string.IsNullOrEmpty(sortBy))
                filters += $"&sortBy={sortBy}";

            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri(Client.BaseAddress + $"api/account/List{filters}");

            using var response = await Client.SendAsync(request);

            await ValidateResponse(response);

            if(response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return null;
            }

            var accounts = await response.Content.ReadFromJsonAsync<PaginatedList<AccountDto>>();
            return accounts;
        }

        private static async Task ValidateResponse(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                // TODO : handle failures without throwing errors
                var failure = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                if (failure != null)
                {
                    throw new ApiException(failure);
                }
                response.EnsureSuccessStatusCode();
            }
        }

    }
}
