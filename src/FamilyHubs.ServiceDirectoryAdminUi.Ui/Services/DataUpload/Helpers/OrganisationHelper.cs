using FamilyHubs.ServiceDirectory.Shared.Dto;
using System.Data;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.DataUpload.Helpers
{
    internal static class OrganisationHelper
    {
        internal static bool TryResolveOrganisationType(DataRow dtRow, out OrganisationTypeDto organisationTypeDto, out string? organisationName)
        {
            switch (dtRow["Organisation Type"].ToString()?.ToLower())
            {
                case "local authority":
                    organisationTypeDto = new OrganisationTypeDto("1", "LA", "Local Authority");
                    organisationName = dtRow["Local authority"].ToString();
                    break;
                case "voluntary and community sector":
                    organisationTypeDto = new OrganisationTypeDto("2", "VCFS", "Voluntary, Charitable, Faith Sector");
                    organisationName = dtRow["Name of organisation"].ToString();
                    break;
                case "family hub":
                    organisationTypeDto = new OrganisationTypeDto("3", "FamilyHub", "Family Hub");
                    organisationName = dtRow["Local authority"].ToString();
                    break;
                default:
                    organisationTypeDto = new OrganisationTypeDto("4", "Company", "Public / Private Company eg: Child Care Centre");
                    organisationName = dtRow["Name of organisation"].ToString();
                    break;

            }

            if (organisationTypeDto.Name != "LA" && organisationTypeDto.Name != "FamilyHub")
            {
                if (string.IsNullOrWhiteSpace(organisationName))
                {
                    return false;
                }
            }

            return true;
        }

    }
}
