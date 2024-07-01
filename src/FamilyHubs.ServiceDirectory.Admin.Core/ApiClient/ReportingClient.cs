using FamilyHubs.ServiceDirectory.Admin.Core.Exceptions;
using FamilyHubs.SharedKernel.Exceptions;
using System.Net.Http.Json;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.SharedKernel.Reports.WeeklyBreakdown;

namespace FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;

public interface IReportingClient
{
    Task<long> GetServicesSearchesPast7Days(ServiceType type, long? laOrganisationId = null, CancellationToken cancellationToken = default);
    Task<WeeklyReportBreakdown> GetServicesSearches4WeekBreakdown(ServiceType type, long? laOrganisationId = null, CancellationToken cancellationToken = default);
    Task<long> GetServicesSearchesTotal(ServiceType type, long? laOrganisationId = null, CancellationToken cancellationToken = default);

    Task<ConnectionRequestMetric> GetConnectionRequestsPast7Days(ServiceType type, long? laOrganisationId = null, CancellationToken cancellationToken = default);
    Task<WeeklyReportBreakdown<ConnectionRequestMetric>> GetConnectionRequests4WeekBreakdown(ServiceType type, long? laOrganisationId = null, CancellationToken cancellationToken = default);
    Task<ConnectionRequestMetric> GetConnectionRequestsTotal(ServiceType type, long? laOrganisationId = null, CancellationToken cancellationToken = default);
}

public class WeeklyReportBreakdown<T>
{
    public IEnumerable<WeeklyReport<T>>? WeeklyReports { get; init; }
    public T? TotalSearchCount { get; init; }
}

public class WeeklyReport<T>
{
    public WeeklyReport(string date, T searchCount)
    {
        Date = date;
        SearchCount = searchCount;
    }

    public string Date { get; init; }
    public T SearchCount { get; init; }
}

public struct ConnectionRequestMetric
{
    public readonly long Made;
    public readonly long Accepted;
    public readonly long Declned;

    public ConnectionRequestMetric(long? made, long? accepted, long? declned)
    {
        Made = made ?? 0;
        Accepted = accepted ?? 0;
        Declned = declned ?? 0;
    }
}

public class ReportingClient : ApiService, IReportingClient
{
    public ReportingClient(HttpClient client)
        : base(client)
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

    public async Task<long> GetServicesSearchesPast7Days(ServiceType type, long? laOrganisationId = null, CancellationToken cancellationToken = default)
    {
        var uri = laOrganisationId.HasValue
            ? $"report/service-searches-past-7-days/organisation/{laOrganisationId}"
            : "report/service-searches-past-7-days";

        return await DoRequest<long>(
            $"{Client.BaseAddress}{uri}?serviceTypeId={(int)type}&date={DateTime.Today:yyyy-MM-dd}",
            cancellationToken
        );
    }

    public async Task<WeeklyReportBreakdown> GetServicesSearches4WeekBreakdown(ServiceType type, long? laOrganisationId = null, CancellationToken cancellationToken = default)
    {
        var uri = laOrganisationId.HasValue
            ? $"report/service-searches-4-week-breakdown/organisation/{laOrganisationId}"
            : "report/service-searches-4-week-breakdown";

        return await DoRequest<WeeklyReportBreakdown>(
            $"{Client.BaseAddress}{uri}?serviceTypeId={(int)type}&date={DateTime.Today:yyyy-MM-dd}",
            cancellationToken
        );
    }

    public async Task<long> GetServicesSearchesTotal(ServiceType type, long? laOrganisationId = null, CancellationToken cancellationToken = default)
    {
        var uri = laOrganisationId.HasValue
            ? $"report/service-searches-total/organisation/{laOrganisationId}"
            : "report/service-searches-total";

        return await DoRequest<long>($"{Client.BaseAddress}{uri}?serviceTypeId={(int)type}", cancellationToken);
    }

    private readonly bool useRealConnectionRequestEndpoints = false;

    public async Task<ConnectionRequestMetric> GetConnectionRequestsPast7Days(ServiceType type, long? laOrganisationId = null, CancellationToken cancellationToken = default)
    {
        if (!useRealConnectionRequestEndpoints)
        {
            return new ConnectionRequestMetric(1, null, null);
        }

        var uri = laOrganisationId.HasValue
            ? $"report/connection-requests-past-7-days/organisation/{laOrganisationId}"
            : "report/connection-requests-past-7-days";

        return await DoRequest<ConnectionRequestMetric>(
            $"{Client.BaseAddress}{uri}?serviceTypeId={(int)type}&date={DateTime.Today:yyyy-MM-dd}",
            cancellationToken
        );
    }

    public async Task<WeeklyReportBreakdown<ConnectionRequestMetric>> GetConnectionRequests4WeekBreakdown(ServiceType type, long? laOrganisationId = null, CancellationToken cancellationToken = default)
    {
        if (!useRealConnectionRequestEndpoints)
        {
            return new WeeklyReportBreakdown<ConnectionRequestMetric>
            {
                WeeklyReports = new[]
                {
                    new WeeklyReport<ConnectionRequestMetric>("17 June to 23 June", new ConnectionRequestMetric(1, null, null)),
                    new WeeklyReport<ConnectionRequestMetric>("10 June to 16 June", new ConnectionRequestMetric(1, null, null)),
                    new WeeklyReport<ConnectionRequestMetric>("3 June to 9 June", new ConnectionRequestMetric(1, null, null)),
                    new WeeklyReport<ConnectionRequestMetric>("27 May to 2 June", new ConnectionRequestMetric(1, null, null))
                },
                TotalSearchCount = new ConnectionRequestMetric(1, null, null)
            };
        }

        var uri = laOrganisationId.HasValue
            ? $"report/connection-requests-4-week-breakdown/organisation/{laOrganisationId}"
            : "report/connection-requests-4-week-breakdown";

        return await DoRequest<WeeklyReportBreakdown<ConnectionRequestMetric>>(
            $"{Client.BaseAddress}{uri}?serviceTypeId={(int)type}&date={DateTime.Today:yyyy-MM-dd}",
            cancellationToken
        );
    }

    public async Task<ConnectionRequestMetric> GetConnectionRequestsTotal(ServiceType type, long? laOrganisationId = null, CancellationToken cancellationToken = default)
    {
        if (!useRealConnectionRequestEndpoints)
        {
            return new ConnectionRequestMetric(1, null, null);
        }

        var uri = laOrganisationId.HasValue
            ? $"report/connection-requests-total/organisation/{laOrganisationId}"
            : "report/connection-requests-total";

        return await DoRequest<ConnectionRequestMetric>($"{Client.BaseAddress}{uri}?serviceTypeId={(int)type}", cancellationToken);
    }

    private async Task<T> DoRequest<T>(string uri, CancellationToken cancellationToken)
    {
        using var response = await Client.GetAsync(uri, cancellationToken);

        await ValidateResponse(response);

        return (await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken))!;
    }
}
