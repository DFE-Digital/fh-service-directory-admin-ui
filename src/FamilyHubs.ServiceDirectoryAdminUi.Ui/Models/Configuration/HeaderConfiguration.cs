namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Models.Configuration;

public class HeaderConfiguration : IHeaderConfiguration
{
    public string ManageApprenticeshipsBaseUrl { get; set; } = default!;
    public string ApplicationBaseUrl { get; set; } = default!;
    public string EmployerCommitmentsV2BaseUrl { get; set; } = default!;
    public string EmployerCommitmentsBaseUrl { get; set; } = default!;
    public string EmployerFinanceBaseUrl { get; set; } = default!;
    public string AuthenticationAuthorityUrl { get; set; } = default!;
    public string ClientId { get; set; } = default!;
    public string EmployerRecruitBaseUrl { get; set; } = default!;
    public Uri SignOutUrl { get; set; } = default!;
    public Uri ChangeEmailReturnUrl { get; set; } = default!;
    public Uri ChangePasswordReturnUrl { get; set; } = default!;
}
