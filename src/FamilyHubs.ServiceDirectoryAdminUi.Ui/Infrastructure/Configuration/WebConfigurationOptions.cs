namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Infrastructure.Configuration;

public class WebConfigurationOptions
{
    public const string WebApplicationConfiguration = "WebApplication";
    public virtual string AllowedHashstringCharacters { get; set; } = default!;
    public virtual string Hashstring { get; set; } = default!;
}
