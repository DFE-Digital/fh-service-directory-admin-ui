using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.DataUpload.Extensions
{
    public static class DataUploadRowContactExtensions
    {
        public static void UpdateServiceContacts(this DataUploadRowDto row, ServiceDto? existingService, ServiceDto service)
        {
            if (row.DeliveryMethod != ServiceDeliveryType.Online && row.DeliveryMethod != ServiceDeliveryType.Telephone)
                return;

            try
            {
                var contact = GetContactFromRow(row);
                if (contact == null) return;

                //  Check if the contact is already in the serviceDto to be uploaded, if it is then exit
                var previouslyAddedContact = service.Contacts.GetMatchingContact(contact);
                if (previouslyAddedContact != null)
                {
                    return;
                }

                //  Check if contact is already in the DB, if it is add it to the service dto and exit
                var existContactRecord = existingService?.Contacts.GetMatchingContact(contact);
                if (existContactRecord != null)
                {
                    service.Contacts.Add(existContactRecord);
                    return;
                }

                //  Location not yet added
                service.Contacts.Add(contact);
            }
            catch (Exception ex)
            {
                var msg = $"There was an error while attempting to UpdateContacts on row {row.ExcelRowId}";
                throw new DataUploadException(msg, ex);
            }
        }

        public static ContactDto? GetMatchingContact(this ICollection<ContactDto>? contacts, ContactDto contact)
        {
            if (contacts == null) return null;

            return contacts.Where(x =>
                x.Email == contact.Email &&
                x.Telephone == contact.Telephone &&
                x.Url == contact.Url &&
                x.TextPhone == contact.TextPhone).FirstOrDefault();
        }

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
                TextPhone = row.ContactSms,
                Url = row.Website
            };
        }
    }
}
