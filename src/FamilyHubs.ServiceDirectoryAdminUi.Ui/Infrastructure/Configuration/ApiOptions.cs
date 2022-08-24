namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Infrastructure.Configuration;

public class ApiOptions
{
    public const string ApplicationServiceApi = "ApplicationServiceApi";

    public string ApiBaseUrl { get; set; } = "https://localhost:7128/";
    public string SubscriptionKey { get; set; } = default!;
    public string ApiVersion { get; set; } = default!;
}
