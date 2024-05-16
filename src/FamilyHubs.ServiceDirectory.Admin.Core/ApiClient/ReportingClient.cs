using FamilyHubs.ServiceDirectory.Admin.Core.Exceptions;
using FamilyHubs.SharedKernel.Exceptions;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;

// TODO: Use from shared kernel
public class WeeklyReportBreakdownDto
{
    public IEnumerable<WeeklyReport> WeeklyReports { get; init; } = null!;

    public int TotalSearchCount { get; init; }
}

public class WeeklyReport
{
    public string Date { get; init; } = null!;

    public int SearchCount { get; init; }
}

public enum ServiceSearchType
{
    Find = 1, Connect = 2
}

public interface IReportingClient
{
    Task<long> GetServicesSearchesPast7Days(ServiceSearchType type, long? laOrganisationId = null, CancellationToken cancellationToken = default);
    Task<WeeklyReportBreakdownDto> GetServicesSearches4WeekBreakdown(ServiceSearchType type, long? laOrganisationId = null, CancellationToken cancellationToken = default);
    Task<long> GetServicesSearchesTotal(ServiceSearchType type, long? laOrganisationId = null, CancellationToken cancellationToken = default);
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

    public async Task<long> GetServicesSearchesPast7Days(ServiceSearchType type, long? laOrganisationId = null, CancellationToken cancellationToken = default)
    {
        var uri = laOrganisationId.HasValue
            ? $"report/service-searches-past-7-days/organisation/{laOrganisationId}"
            : "report/service-searches-past-7-days";

        using var response = await Client.GetAsync($"{Client.BaseAddress}{uri}?serviceTypeId={(int)type}&date={DateTime.Today:yyyy-MM-dd}", cancellationToken);

        await ValidateResponse(response);

        return (await response.Content.ReadFromJsonAsync<long>(cancellationToken: cancellationToken))!;
    }

    public async Task<WeeklyReportBreakdownDto> GetServicesSearches4WeekBreakdown(ServiceSearchType type, long? laOrganisationId = null, CancellationToken cancellationToken = default)
    {
        var uri = laOrganisationId.HasValue
            ? $"report/service-searches-4-week-breakdown/organisation/{laOrganisationId}"
            : "report/service-searches-4-week-breakdown";

        using var response = await Client.GetAsync($"{Client.BaseAddress}{uri}?serviceTypeId={(int)type}&date={DateTime.Today:yyyy-MM-dd}", cancellationToken);

        await ValidateResponse(response);

        return (await response.Content.ReadFromJsonAsync<WeeklyReportBreakdownDto>(cancellationToken: cancellationToken))!;
    }

    public async Task<long> GetServicesSearchesTotal(ServiceSearchType type, long? laOrganisationId = null, CancellationToken cancellationToken = default)
    {
        var uri = laOrganisationId.HasValue
            ? $"report/service-searches-total/organisation/{laOrganisationId}"
            : "report/service-searches-total";

        using var response = await Client.GetAsync($"{Client.BaseAddress}{uri}?serviceTypeId={(int)type}", cancellationToken);

        await ValidateResponse(response);

        return (await response.Content.ReadFromJsonAsync<long>(cancellationToken: cancellationToken))!;
    }
}
