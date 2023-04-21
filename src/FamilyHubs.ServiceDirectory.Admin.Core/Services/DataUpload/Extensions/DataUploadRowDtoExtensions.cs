using System.Linq.Expressions;
using FamilyHubs.ServiceDirectory.Admin.Core.Exceptions;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;

namespace FamilyHubs.ServiceDirectory.Admin.Core.Services.DataUpload.Extensions;

public static class DataUploadRowDtoExtensions
{
    public static T GetServiceValue<T>(this List<DataUploadRowDto> rows, Expression<Func<DataUploadRowDto, T>> keySelectorExpression)
    {
        var keySelector = keySelectorExpression.Compile();
        var firstRow = rows.First().ExcelRowId;
        var value = rows.Select(keySelector).First();

        var failingRows = rows.Where(x => !EqualityComparer<T>.Default.Equals(keySelector.Invoke(x), value)).ToList();

        if (failingRows.Any())
        {
            var propertyName = ((MemberExpression) keySelectorExpression.Body).Member.Name;
            var serviceId = rows.Select(x => x.ServiceOwnerReferenceId).First();

            var rowNumbers = string.Join(", ", failingRows.Select(m => m.ExcelRowId));
            throw new DataUploadException($"Data mismatch, for serviceId {serviceId} {propertyName} row {firstRow} is not the same in row(s) :{rowNumbers}");
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

        if (service.ServiceDeliveries.All(x => x.Name != deliverMethod.Name))
        {
            service.ServiceDeliveries.Add(deliverMethod);
        }
    }

    public static void UpdateLanguages(this DataUploadRowDto row, ServiceDto? existingService, ServiceDto service)
    {
        if (string.IsNullOrWhiteSpace(row.Language))
            return;

        var splitLanguages = row.Language.Split("|");

        //if all the languages already exists return
        var existingLanguage = service.Languages.Where(x => splitLanguages.Contains(x.Name));
        if (existingLanguage.Count() == splitLanguages.Length)
            return;

        //add missing languages
        if (existingService?.Languages != null)
        {
            foreach (var language in existingService.Languages)
            {
                if (service.Languages.Any(ln => ln.Name == language.Name))
                    service.Languages.Remove(language);

                service.Languages.Add(language);
            }
        }

        splitLanguages.Where(lName => service.Languages.All(lg => lg.Name != lName)).ToList()
            .ForEach(name => service.Languages.Add(new LanguageDto { Name = name }));
    }

    public static void UpdateRegularSchedules(this DataUploadRowDto row, ServiceDto? existingService, ServiceDto service)
    {
        if (string.IsNullOrWhiteSpace(row.OpeningHoursDescription))
            return;

        var existingRegularSchedule = service.RegularSchedules.FirstOrDefault(x => x.Description == row.OpeningHoursDescription);
        if (existingRegularSchedule != null)
            return;

        existingRegularSchedule = existingService?.RegularSchedules.FirstOrDefault(x => x.Description == row.OpeningHoursDescription);
        if (existingRegularSchedule != null)
        {
            service.RegularSchedules.Add(existingRegularSchedule);
            return;
        }

        service.RegularSchedules.Add(new RegularScheduleDto { Description = row.OpeningHoursDescription });
    }

    public static void UpdateTaxonomies(this DataUploadRowDto row, ServiceDto service, CachedApiResponses cachedApiResponses)
    {
        var categories = row.SubCategory;
        if (string.IsNullOrEmpty(categories)) return;

        var parts = categories.Split('|').Select(s => s.Trim());
        foreach (var part in parts)
        {
            var taxonomy = cachedApiResponses.Taxonomies.FirstOrDefault(x => x.Name == part);
            var taxonomyAlreadyAdded = service.Taxonomies.Any(x => x.Name == part);

            if (taxonomy != null && !taxonomyAlreadyAdded)
            {
                service.Taxonomies.Add(taxonomy);
            }
        }
    }
}