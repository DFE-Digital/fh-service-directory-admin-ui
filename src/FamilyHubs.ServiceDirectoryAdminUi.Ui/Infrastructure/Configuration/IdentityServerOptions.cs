namespace WebUI.Infrastructure.Configuration;

public class IdentityServerOptions
{
    public const string IdentityServerConfiguration = "Identity";
    public string ClientId { get; set; } = default!;
    public string ClientSecret { get; set; } = default!;
    public string BaseAddress { get; set; } = default!;
    public string Scopes { get; set; } = default!;
    public bool UsePkce { get; set; }
    public string ChangeEmailUrl { get; set; } = default!;
    public string ChangeEmailLinkFormatted()
    {
        return BaseAddress.Replace("/identity", "") + string.Format(ChangeEmailUrl, ClientId);
    }
    public string ChangePasswordUrl { get; set; } = default!;
    public string ChangePasswordLinkFormatted()
    {
        return BaseAddress.Replace("/identity", "") + string.Format(ChangePasswordUrl, ClientId);
    }
}
