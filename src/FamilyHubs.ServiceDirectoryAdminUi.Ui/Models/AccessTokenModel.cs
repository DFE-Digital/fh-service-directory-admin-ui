namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;

public class AccessTokenModel
{
    public string Token { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
    public DateTime Expiration { get; set; } = default!;
}
