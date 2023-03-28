using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.DataUpload.Extensions
{
    public static class DataUploadRowEligibilitiesExtensions
    {
        public static void UpdateEligibilities(this DataUploadRowDto row, ServiceDto? existingService, ServiceDto service)
        {
            var eligibility = GetEligibilityFromRow(row);

            var existingEligibility = GetMatchingEligibility(eligibility, service);
            if (existingEligibility != null)
                return;

            existingEligibility = GetMatchingEligibility(eligibility, existingService);
            if (existingEligibility != null)
            {
                service.Eligibilities.Add(existingEligibility);
                return;
            }

            service.Eligibilities.Add(eligibility);
        }

        private static EligibilityDto GetEligibilityFromRow(DataUploadRowDto row)
        {
            if (!int.TryParse(row.AgeFrom, out var minimumAge))
            {
                minimumAge = 0;
            }

            if (!int.TryParse(row.AgeTo, out var maximumAge))
            {
                maximumAge = 127;
            }

            var eligibility = new EligibilityDto
            {
                MaximumAge = maximumAge,
                MinimumAge = minimumAge,
                EligibilityType = EligibilityType.Child
            };

            if (eligibility.MinimumAge >= 18)
            {
                eligibility.EligibilityType = EligibilityType.Adult;
            }

            return eligibility;
        }

        private static EligibilityDto? GetMatchingEligibility(EligibilityDto eligibility, ServiceDto? service)
        {
            if (service == null) return null;
            return service.Eligibilities.Where(x => 
                x.MaximumAge == eligibility.MaximumAge &&
                x.MinimumAge == eligibility.MinimumAge &&
                x.EligibilityType == eligibility.EligibilityType).FirstOrDefault();
        }
    }
}
