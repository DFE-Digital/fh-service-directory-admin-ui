using FamilyHubs.ServiceDirectory.Shared.Dto;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.DataUpload.Extensions
{
    public static class DataUploadRowCostsExtensions
    {
        public static void UpdateCosts(this DataUploadRowDto row, ServiceDto? existingService, ServiceDto service)
        {
            var costOption = GetCostFromRow(row);
            if (costOption == null) return;

            var existingCostOption = GetMatchingCostOption(costOption, service);
            if (existingCostOption != null)
                return;

            existingCostOption = GetMatchingCostOption(costOption, existingService);
            if (existingCostOption != null)
            {
                service.CostOptions.Add(existingCostOption);
                return;
            }

            service.CostOptions.Add(costOption);
        }

        private static CostOptionDto? GetCostFromRow(DataUploadRowDto row)
        {
            if (string.IsNullOrEmpty(row.CostInPounds) && string.IsNullOrEmpty(row.CostPer) && string.IsNullOrEmpty(row.CostDescription))
            {
                return null;
            }

            if (!decimal.TryParse(row.CostInPounds, out var amount))
            {
                amount = 0.0M;
            }

            return new CostOptionDto
            {
                Amount = amount,
                AmountDescription= row.CostDescription,
                Option = row.CostPer
            };
        }

        private static CostOptionDto? GetMatchingCostOption(CostOptionDto costOptionDto, ServiceDto? service)
        {
            if (service == null) return null;

            return service.CostOptions.Where(x =>
                x.Amount == costOptionDto.Amount &&
                x.AmountDescription == costOptionDto.AmountDescription &&
                x.Option == costOptionDto.Option).FirstOrDefault();
        }
    }
}
