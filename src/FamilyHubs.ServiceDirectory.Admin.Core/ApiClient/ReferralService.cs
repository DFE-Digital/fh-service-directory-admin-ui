using FamilyHubs.ReferralService.Shared.Dto;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using Microsoft.Extensions.Logging;

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
}
