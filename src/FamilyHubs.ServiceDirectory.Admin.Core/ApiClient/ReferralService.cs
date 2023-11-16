using FamilyHubs.ReferralService.Shared.Dto;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient.Exceptions;

namespace FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;

public enum ReferralStatus
{
    New = 1,
    Opened,
    Accepted,
    Declined
}

public interface IReferralService
{
    Task<List<ReferralDto>> GetReferralsByRecipient(SubjectAccessRequestViewModel model);
    Task<ReferralDto> GetReferralById(long referralId, CancellationToken cancellationToken = default);
}

public class ReferralService : ApiService<ReferralService>, IReferralService
{
    public ReferralService(HttpClient client, ILogger<ReferralService> logger)
    : base(client, logger)
    {

    }

    public async Task<List<ReferralDto>> GetReferralsByRecipient(SubjectAccessRequestViewModel model)
    {
        var request = new HttpRequestMessage();
        request.Method = HttpMethod.Get;

        string urlParam = string.Empty;

        switch (model.SelectionType)
        {
            case "email":
                urlParam = $"api/referral/recipient?email={model.Value1}";
                break;

            case "phone":
                urlParam = $"api/referral/recipient?telephone={model.Value1}";
                break;

            case "textphone":
                urlParam = $"api/referral/recipient?textphone={model.Value1}";
                break;

            case "nameandpostcode":
                urlParam = $"api/referral/recipient?name={model.Value1}&postcode={model.Value2}";
                break;
        }

        request.RequestUri = new Uri(Client.BaseAddress + urlParam);

        using var response = await Client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        Logger.LogInformation($"{nameof(ReferralService)} Returning Referrals");
        return await DeserializeResponse<List<ReferralDto>>(response) ?? new List<ReferralDto>();
    }

    public async Task<ReferralDto> GetReferralById(long referralId, CancellationToken cancellationToken = default)
    {
        var url = $"api/referral/{referralId}";

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(Client.BaseAddress + url),
        };

        using var response = await Client.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new ReferralClientServiceException(response, await response.Content.ReadAsStringAsync(cancellationToken));
        }

        var referral = await JsonSerializer.DeserializeAsync<ReferralDto>(
            await response.Content.ReadAsStreamAsync(cancellationToken),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
            cancellationToken);

        if (referral is null)
        {
            // the only time it'll be null, is if the API returns "null"
            // (see https://stackoverflow.com/questions/71162382/why-are-the-return-types-of-nets-system-text-json-jsonserializer-deserialize-m)
            // unlikely, but possibly (pass new MemoryStream(Encoding.UTF8.GetBytes("null")) to see it actually return null)
            // note we hard-code passing "null", rather than messing about trying to rewind the stream, as this is such a corner case and we want to let the deserializer take advantage of the async stream (in the happy case)
            throw new ReferralClientServiceException(response, "null");
        }

        return referral;
    }
}
