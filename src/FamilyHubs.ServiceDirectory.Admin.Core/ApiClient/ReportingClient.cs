using FamilyHubs.ServiceDirectory.Admin.Core.Exceptions;
using FamilyHubs.SharedKernel.Exceptions;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;

// TODO: Use from shared kernel
public class FourWeekBreakdownDto
{
    public string WeekOneName { get; set; } = null!;
    public int WeekOneSearchCount { get; set; }

    public string WeekTwoName { get; set; } = null!;
    public int WeekTwoSearchCount { get; set; }

    public string WeekThreeName { get; set; } = null!;
    public int WeekThreeSearchCount { get; set; }

    public string WeekFourName { get; set; } = null!;
    public int WeekFourSearchCount { get; set; }

    public IEnumerable<(string, int)> Iterate()
    {
        yield return (WeekOneName, WeekOneSearchCount);
        yield return (WeekTwoName, WeekTwoSearchCount);
        yield return (WeekThreeName, WeekThreeSearchCount);
        yield return (WeekFourName, WeekFourSearchCount);
    }

    public int TotalSearchCount { get; set; }
}

public interface IReportingClient
{
    Task<long> GetServicesSearchesPast7Days(long? laOrganisationId = null, CancellationToken cancellationToken = default);
    Task<FourWeekBreakdownDto> GetServicesSearches4WeekBreakdown(long? laOrganisationId = null, CancellationToken cancellationToken = default);
    Task<long> GetServicesSearchesTotal(long? laOrganisationId = null, CancellationToken cancellationToken = default);
}

public class ReportingClient : ApiService<ReportingClient>, IReportingClient
{
    public ReportingClient(HttpClient client, ILogger<ReportingClient> logger)
        : base(client, logger)
    {
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

    public async Task<long> GetServicesSearchesPast7Days(long? laOrganisationId = null, CancellationToken cancellationToken = default)
    {
        var uri = laOrganisationId.HasValue
            ? $"report/service-searches-past-7-days/organisation/{laOrganisationId}"
            : "report/service-searches-past-7-days";

        using var response = await Client.GetAsync($"{Client.BaseAddress}{uri}?date={DateTime.Today:yyyy-MM-dd}", cancellationToken);

        await ValidateResponse(response);

        return (await response.Content.ReadFromJsonAsync<long>(cancellationToken: cancellationToken))!;
    }

    public async Task<FourWeekBreakdownDto> GetServicesSearches4WeekBreakdown(long? laOrganisationId = null, CancellationToken cancellationToken = default)
    {
        var uri = laOrganisationId.HasValue
            ? $"report/service-searches-4-week-breakdown/organisation/{laOrganisationId}"
            : "report/service-searches-4-week-breakdown";

        using var response = await Client.GetAsync($"{Client.BaseAddress}{uri}?date={DateTime.Today:yyyy-MM-dd}", cancellationToken);

        await ValidateResponse(response);

        return (await response.Content.ReadFromJsonAsync<FourWeekBreakdownDto>(cancellationToken: cancellationToken))!;
    }

    public async Task<long> GetServicesSearchesTotal(long? laOrganisationId = null, CancellationToken cancellationToken = default)
    {
        var uri = laOrganisationId.HasValue
            ? $"report/service-searches-total/organisation/{laOrganisationId}"
            : "report/service-searches-total";

        using var response = await Client.GetAsync($"{Client.BaseAddress}{uri}", cancellationToken);

        await ValidateResponse(response);

        return (await response.Content.ReadFromJsonAsync<long>(cancellationToken: cancellationToken))!;
    }
}
