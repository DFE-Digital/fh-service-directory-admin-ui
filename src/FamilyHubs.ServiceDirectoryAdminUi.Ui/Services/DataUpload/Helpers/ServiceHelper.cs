using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using System.Data;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.DataUpload.Helpers
{
    internal static class ServiceHelper
    {
        internal static string GetServiceDeliveryId(ServiceDto? service, ServiceDeliveryType serviceDelivery)
        {
            var id = Guid.NewGuid().ToString();
            if (service != null && service.ServiceDeliveries != null)
            {
                var serviceDeliveryItem = service.ServiceDeliveries.FirstOrDefault(x => x.Name == serviceDelivery);
                if (serviceDeliveryItem != null)
                {
                    id = serviceDeliveryItem.Id;
                }
            }

            return id;

        }

        internal static List<LanguageDto> GetLanguages(DataRow dtRow, ServiceDto? service)
        {
            var list = (service != null && service.Languages != null) ? service.Languages.ToList() : new List<LanguageDto>();
            var languages = dtRow["Language"].ToString();
            if (!string.IsNullOrEmpty(languages))
            {
                var parts = languages.Split('|');
                foreach (var part in parts)
                {
                    var languageId = Guid.NewGuid().ToString();
                    if (service != null && service.Languages != null)
                    {
                        var originalLanguage = service.Languages.FirstOrDefault(x => x.Name == part);
                        if (originalLanguage != null)
                        {
                            languageId = originalLanguage.Id;
                        }
                    }

                    list.Add(new LanguageDto(languageId, part.Trim()));
                }
            }

            return list;
        }

        internal static ServiceTypeDto GetServiceType(OrganisationTypeDto organisationTypeDto)
        {
            switch (organisationTypeDto.Name)
            {
                case "LA":
                case "FamilyHub":
                    return new ServiceTypeDto("2", "Family Experience", "");


                default:
                    return new ServiceTypeDto("1", "Information Sharing", "");

            }
        }

        internal static List<CostOptionDto> GetCosts(DataRow dtRow, ServiceDto? service)
        {
            var list = service?.CostOptions?.Count > 1 ? service.CostOptions.ToList() : new();

            if (string.IsNullOrEmpty(dtRow["Cost (£ in pounds)"].ToString()) &&
                string.IsNullOrEmpty(dtRow["Cost per"].ToString()) &&
                string.IsNullOrEmpty(dtRow["Cost Description"].ToString()))
            {
                return list;
            }

            if (!decimal.TryParse(dtRow["Cost (£ in pounds)"].ToString(), out var amount))
            {
                amount = 0.0M;
            }

            var costId = Guid.NewGuid().ToString();
            if (service != null && service.CostOptions != null)
            {
                var costOption = (amount != 0.0M && string.IsNullOrEmpty(dtRow["Cost per"].ToString())) ? service.CostOptions.FirstOrDefault(t => t.Amount == amount && t.AmountDescription == dtRow["Cost per"].ToString()) : service.CostOptions.FirstOrDefault(t => (t.Option == dtRow["Cost Description"].ToString()));
                if (costOption != null)
                {
                    costId = costOption.Id;
                }
            }

            list.Add(new CostOptionDto(
                                costId,
                                dtRow["Cost per"].ToString() ?? string.Empty,
                                amount,
                                null,
                                dtRow["Cost Description"].ToString(),
                                null,
                                null
                                ));

            return list;
        }

        internal static List<ServiceDeliveryDto> GetDeliveryTypes(string rowDeliveryTypes, ServiceDto? service)
        {
            List<ServiceDeliveryDto> list = new();
            var parts = rowDeliveryTypes.Split('|');
            foreach (var part in parts)
            {

                if (string.Compare(part, DeliverMethods.IN_PERSON, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    list.Add(new ServiceDeliveryDto(ServiceHelper.GetServiceDeliveryId(service, ServiceDeliveryType.InPerson), ServiceDeliveryType.InPerson));
                }
                else if (string.Compare(part, DeliverMethods.ONLINE, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    list.Add(new ServiceDeliveryDto(ServiceHelper.GetServiceDeliveryId(service, ServiceDeliveryType.Online), ServiceDeliveryType.Online));
                }
                else if (string.Compare(part, DeliverMethods.TELEPHONE, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    list.Add(new ServiceDeliveryDto(ServiceHelper.GetServiceDeliveryId(service, ServiceDeliveryType.Telephone), ServiceDeliveryType.Telephone));
                }
            }

            return list;
        }

        internal static List<EligibilityDto> GetEligibilities(DataRow dtRow, ServiceDto? service)
        {
            var eligibilityId = Guid.NewGuid().ToString();
            var list = (service != null && service.Eligibilities != null) ? service.Eligibilities.ToList() : new();

            if (!int.TryParse(dtRow["Age from"].ToString(), out var minimumAge))
            {
                minimumAge = 0;
            }

            if (!int.TryParse(dtRow["Age to"].ToString(), out var maximumAge))
            {
                maximumAge = 127;
            }

            var eligibility = "Child";
            if (minimumAge >= 18)
            {
                eligibility = "Adult";
            }

            if (service != null && service.Eligibilities != null)
            {
                var eligibleItem = service.Eligibilities?.Count == 1 ? service.Eligibilities?.First() : service.Eligibilities?.FirstOrDefault(x => x.MinimumAge == minimumAge && x.MaximumAge == maximumAge);
                if (eligibleItem != null)
                {
                    eligibilityId = eligibleItem.Id;
                }
            }

            list.Add(new EligibilityDto(eligibilityId, eligibility, maximumAge, minimumAge));

            return list;
        }

    }
}
