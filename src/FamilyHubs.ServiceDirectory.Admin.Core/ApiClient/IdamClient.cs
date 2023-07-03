using FamilyHubs.ServiceDirectory.Admin.Core.Exceptions;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Models;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace FamilyHubs.ServiceDirectory.Admin.Core.ApiClient
{
    public interface IIdamClient
    {
        public Task AddAccount(AccountDto accountDto);
        public Task<PaginatedList<AccountDto>?> GetAccounts(long organisationId, int pageNumber);
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

        public async Task<PaginatedList<AccountDto>?> GetAccounts(long organisationId, int pageNumber)
        {
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri(Client.BaseAddress + $"api/account/List?pageSize=10&pageNumber={pageNumber}");

            using var response = await Client.SendAsync(request);

            await ValidateResponse(response);

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
