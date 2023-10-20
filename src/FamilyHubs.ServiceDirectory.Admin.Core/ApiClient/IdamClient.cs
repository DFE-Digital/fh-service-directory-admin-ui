using FamilyHubs.ServiceDirectory.Admin.Core.Exceptions;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Shared.Models;
using FamilyHubs.SharedKernel.Exceptions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Text;
using System.Web;

namespace FamilyHubs.ServiceDirectory.Admin.Core.ApiClient
{
    public interface IIdamClient
    {
        Task<Outcome<ErrorCodes>> AddAccount(AccountDto accountDto);
        Task<AccountDto?> GetAccountBEmail(string email);
        Task<AccountDto?> GetAccountById(long id);
        Task<PaginatedList<AccountDto>?> GetAccounts(
            long organisationId, int pageNumber, string? userName = null, string? email = null, string? organisationName = null, bool? isLaUser = null, bool? isVcsUser = null, string? sortBy = null);

        Task<long> UpdateAccount(UpdateAccountDto accountDto, CancellationToken cancellationToken = default);

        Task UpdateClaim(UpdateClaimDto updateClaimDto);
        Task DeleteAccount(long id);
        Task DeleteOrganisationAccounts(long organisationId);
    }

    public class IdamClient : ApiService<IdamClient>, IIdamClient
    {
        public IdamClient(HttpClient client, ILogger<IdamClient> logger) : base(client, logger)
        {
        }

        public async Task<Outcome<ErrorCodes>> AddAccount(AccountDto accountDto)
        {
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri(Client.BaseAddress + "api/account");
            request.Content = new StringContent(JsonConvert.SerializeObject(accountDto), Encoding.UTF8, "application/json");

            using var response = await Client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return new Outcome<ErrorCodes>(true);
            }

            var failure = await response.Content.ReadFromJsonAsync<ApiExceptionResponse<ValidationError>>();
            if (failure != null)
            {
                Logger.LogWarning("Failed to add Account {@apiExceptionResponse}", failure);
                return new Outcome<ErrorCodes>(failure.ErrorCode.ParseToErrorCode() ,false);
            }

            Logger.LogError("Response from api failed with an unknown response body {statusCode}", response.StatusCode);
            return new Outcome<ErrorCodes>(ErrorCodes.UnhandledException, false);
        }

        public async Task<AccountDto?> GetAccountBEmail(string email)
        {
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri(Client.BaseAddress + $"api/account?email={HttpUtility.UrlEncode(email)}");

            using var response = await Client.SendAsync(request);

            await ValidateResponse(response);

            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return null;
            }

            var account = await response.Content.ReadFromJsonAsync<AccountDto>();
            return account;
        }

        public async Task<AccountDto?> GetAccountById(long id)
        {
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri(Client.BaseAddress + $"api/account/{id}");

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
                filters += $"&userName={HttpUtility.UrlEncode(userName)}";

            if (!string.IsNullOrEmpty(email))
                filters += $"&email={HttpUtility.UrlEncode(email)}";

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

        //todo: single shared clients
        public async Task<long> UpdateAccount(UpdateAccountDto accountDto, CancellationToken cancellationToken = default)
        {
            using var response = await Client.PutAsJsonAsync(Client.BaseAddress + "api/account", accountDto, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new IdamsClientServiceException(response, await response.Content.ReadAsStringAsync(cancellationToken));
            }

            long? id = await response.Content.ReadFromJsonAsync<long>(cancellationToken: cancellationToken);

            if (id is null)
            {
                // the only time it'll be null, is if the API returns "null"
                // (see https://stackoverflow.com/questions/71162382/why-are-the-return-types-of-nets-system-text-json-jsonserializer-deserialize-m)
                // unlikely, but possibly (pass new MemoryStream(Encoding.UTF8.GetBytes("null")) to see it actually return null)
                // note we hard-code passing "null", rather than messing about trying to rewind the stream, as this is such a corner case and we want to let the deserializer take advantage of the async stream (in the happy case)
                throw new IdamsClientServiceException(response, "null");
            }

            return id.Value;
        }

        private static async Task ValidateResponse(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                // TODO : handle failures without throwing errors
                var failure = await response.Content.ReadFromJsonAsync<ApiExceptionResponse<ValidationError>>();
                if (failure != null)
                {
                    throw new ApiException(failure);
                }
                response.EnsureSuccessStatusCode();
            }
        }

        public async Task UpdateClaim(UpdateClaimDto updateClaimDto)
        {
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Put;
            request.RequestUri = new Uri(Client.BaseAddress + "api/AccountClaims/UpdateClaim");
            request.Content = new StringContent(JsonConvert.SerializeObject(updateClaimDto), Encoding.UTF8, "application/json");

            using var response = await Client.SendAsync(request);

            await ValidateResponse(response);
        }

        public async Task DeleteAccount(long id)
        {
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Delete;
            request.RequestUri = new Uri(Client.BaseAddress + "api/Account");
            
            request.Content = new StringContent(JsonConvert.SerializeObject(new { AccountId = id }), Encoding.UTF8, "application/json");

            using var response = await Client.SendAsync(request);

            await ValidateResponse(response);
        }

        public async Task DeleteOrganisationAccounts(long organisationId)
        {
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Delete;
            request.RequestUri = new Uri(Client.BaseAddress + "api/Account/DeleteOrganisationAccounts");

            request.Content = new StringContent(JsonConvert.SerializeObject(new { OrganisationId = organisationId }), Encoding.UTF8, "application/json");

            using var response = await Client.SendAsync(request);

            await ValidateResponse(response);
        }
    }
}
