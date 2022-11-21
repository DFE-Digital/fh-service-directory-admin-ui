//using SFA.DAS.Http.Configuration;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Infrastructure.Configuration;

public class ApiOptions //: IApimClientConfiguration
{
    public const string ApplicationServiceApi = "ApplicationServiceApi";

    public string ApiBaseUrl { get; set; } = "https://localhost:7022/";
    public string SubscriptionKey { get; set; } = default!;
    public string ApiVersion { get; set; } = default!;
}
