namespace WebUI.Infrastructure.Configuration;

public class ExternalLinksConfiguration
{
    public const string EmployerIncentivesExternalLinksConfiguration = "ExternalLinks";

    public virtual string ManageApprenticeshipSiteUrl { get; set; } = default!;
    public virtual string CommitmentsSiteUrl { get; set; } = default!;
    public virtual string EmployerRecruitmentSiteUrl { get; set; } = default!;
}
