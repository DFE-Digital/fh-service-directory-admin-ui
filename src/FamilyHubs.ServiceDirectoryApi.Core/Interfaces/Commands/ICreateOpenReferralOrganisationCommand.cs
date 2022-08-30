using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralOrganisations;

namespace fh_service_directory_api.core.Interfaces.Commands
{
    public interface ICreateOpenReferralOrganisationCommand
    {
        OpenReferralOrganisationWithServicesDto OpenReferralOrganisation { get; init; }
    }
}