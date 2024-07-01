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

        using var response = await Client.GetAsync($"{Client.BaseAddress}{uri}?serviceTypeId={(int)type}&date={DateTime.Today:yyyy-MM-dd}", cancellationToken);

        await ValidateResponse(response);

        return await response.Content.ReadFromJsonAsync<long>(cancellationToken: cancellationToken);
    }

    public async Task<WeeklyReportBreakdown> GetServicesSearches4WeekBreakdown(ServiceType type, long? laOrganisationId = null, CancellationToken cancellationToken = default)
    {
        var uri = laOrganisationId.HasValue
            ? $"report/service-searches-4-week-breakdown/organisation/{laOrganisationId}"
            : "report/service-searches-4-week-breakdown";

        using var response = await Client.GetAsync($"{Client.BaseAddress}{uri}?serviceTypeId={(int)type}&date={DateTime.Today:yyyy-MM-dd}", cancellationToken);

        await ValidateResponse(response);

        return (await response.Content.ReadFromJsonAsync<WeeklyReportBreakdown>(cancellationToken: cancellationToken))!;
    }

    public async Task<long> GetServicesSearchesTotal(ServiceType type, long? laOrganisationId = null, CancellationToken cancellationToken = default)
    {
        var uri = laOrganisationId.HasValue
            ? $"report/service-searches-total/organisation/{laOrganisationId}"
            : "report/service-searches-total";

        using var response = await Client.GetAsync($"{Client.BaseAddress}{uri}?serviceTypeId={(int)type}", cancellationToken);

        await ValidateResponse(response);

        return await response.Content.ReadFromJsonAsync<long>(cancellationToken: cancellationToken);
    }
}
