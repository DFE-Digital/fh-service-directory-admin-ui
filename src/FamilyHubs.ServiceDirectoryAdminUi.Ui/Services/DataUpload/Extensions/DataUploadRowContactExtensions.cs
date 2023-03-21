using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.DataUpload.Extensions
{
    public static class DataUploadRowContactExtensions
    {
        //public static void UpdateContacts(this DataUploadRowDto row, ServiceDto? existingService, ServiceDto service)
        //{
        //    try
        //    {
        //        var contact = GetContactFromRow(row);
        //        if (contact == null) return;

        //        //  Check if contact is already in the DB
        //        var existLocationRecord = GetMatchingContact(existingService, contact);
        //        if (existLocationRecord != null)
        //        {
        //            UpdateExistingLocationRecord(existLocationRecord, location);
        //            UpdateLocationContacts(existLocationRecord, row);
        //            return;
        //        }

        //        //  Check if the location is already in the serviceDto to be uploaded, if it is update its properties
        //        var previouslyAddedLocation = GetLocationMatchingLocation(row, service, postCodeData);
        //        if (previouslyAddedLocation != null)
        //        {
        //            UpdateLocationContacts(previouslyAddedLocation, row);
        //            return;
        //        }

        //        //  Location not yet added
        //        UpdateLocationContacts(location, row);
        //        service.Locations.Add(location);
        //    }
        //    catch (Exception ex)
        //    {
        //        var msg = $"There was an error while attempting to UpdateContacts on row {row.ExcelRowId}";
        //        throw new DataUploadException(msg, ex);
        //    }
        //}

        //private static ContactDto? GetMatchingContact(ServiceDto? service, ContactDto contact)
        //{
        //    if (service == null) return null;

        //    return service.Contacts.Where(x =>
        //        x.Email == contact.Email &&
        //        x.Telephone == contact.Telephone &&
        //        x.Url == contact.Url &&
        //        x.TextPhone == contact.TextPhone).FirstOrDefault();
        //}

        //private static void UpdateServiceContacts(LocationDto location, DataUploadRowDto row)
        //{
        //    if (row.DeliveryMethod != ServiceDeliveryType.InPerson)
        //        return;

        //    var contact = row.GetContactFromRow();
        //    if (contact == null) return;

        //    var contactExists = location.Contacts.Where(x =>
        //        x.Email == contact.Email &&
        //        x.Telephone == contact.Telephone &&
        //        x.Url == contact.Url &&
        //        x.TextPhone == contact.TextPhone
        //    ).Any();

        //    if (!contactExists)
        //        location.Contacts.Add(contact);
        //}

        public static ContactDto? GetContactFromRow(this DataUploadRowDto row)
        {
            if (string.IsNullOrEmpty(row.ContactEmail) &&
                string.IsNullOrEmpty(row.ContactPhone) &&
                string.IsNullOrEmpty(row.ContactSms) &&
                string.IsNullOrEmpty(row.Website))
            {
                return null;
            }

            return new ContactDto
            {
                Telephone = row.ContactPhone!,
                Email = row.ContactEmail,
                TextPhone = row.ContactPhone,
                Url = row.Website
            };
        }
    }
}
