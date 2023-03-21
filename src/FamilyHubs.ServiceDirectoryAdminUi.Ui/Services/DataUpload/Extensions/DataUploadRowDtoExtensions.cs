using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using System.Linq.Expressions;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.DataUpload.Extensions
{
    public static class DataUploadRowDtoExtensions
    {
        public static T GetServiceValue<T>(this List<DataUploadRowDto> rows, Expression<Func<DataUploadRowDto, T>> keySelectorExpression)
        {
            var keySelector = keySelectorExpression.Compile();
            var value = rows.Select(keySelector).First();

            var failingRows = rows.Where(x => !EqualityComparer<T>.Default.Equals(keySelector.Invoke(x), value));

            if (failingRows.Any())
            {
                var propertyName = ((MemberExpression)keySelectorExpression.Body).Member.Name;
                var serviceId = rows.Select(x => x.ServiceOwnerReferenceId).First();

                var rowNumbers = string.Join(", ", failingRows.Select(m => m.ExcelRowId).ToList());
                throw new DataUploadException($"Data mismatch, for serviceId {serviceId} {propertyName} is not the same in row(s) :{rowNumbers}");
            }

            return value;
        }

        public static void UpdateServiceDeliveries(this DataUploadRowDto row, ServiceDto service)
        {
            if (row.DeliveryMethod == ServiceDeliveryType.NotSet)
                return;

            var deliverMethod = new ServiceDeliveryDto
            {
                Name = row.DeliveryMethod
            };

            if (!service.ServiceDeliveries.Where(x => x.Name == deliverMethod.Name).Any())
            {
                service.ServiceDeliveries.Add(deliverMethod);
            }
        }

    }

}
