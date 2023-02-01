using FamilyHubs.ServiceDirectory.Shared.Dto;
using System.Data;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.DataUpload
{
    internal static class ContactHelper
    {
        internal static List<LinkContactDto> GetLinkContacts(string linkId, string linkType, DataRow dtRow, ICollection<LinkContactDto>? existingLinks, List<ContactDto> existingContacts)
        {
            var linkContacts = new List<LinkContactDto>();

            if (ResolveLinkType(dtRow) != linkType)
                return linkContacts;

            var contact = new ContactDto(
                Guid.NewGuid().ToString(),
                string.Empty, // Title
                string.Empty, // Name
                dtRow["Contact phone"].ToString() ?? string.Empty,
                dtRow["Contact sms"].ToString() ?? string.Empty,
                dtRow["Website"].ToString() ?? string.Empty,
                dtRow["Contact email"].ToString() ?? string.Empty);

            var linkAlreadyPresent = existingLinks?.Where(x =>
                x.Contact.Telephone == contact.Telephone &&
                x.Contact.TextPhone == contact.TextPhone &&
                x.Contact.Url == contact.Url &&
                x.Contact.Email == contact.Email
            ).Any();

            if(linkAlreadyPresent.HasValue && linkAlreadyPresent.Value)
                return existingLinks!.ToList(); // Link Record already exists so return existing list

            var matchingContact = existingContacts.Where(x =>
                x.Telephone == contact.Telephone &&
                x.TextPhone == contact.TextPhone &&
                x.Url == contact.Url &&
                x.Email == contact.Email
            ).FirstOrDefault();

            if (matchingContact == null)
            {
                //  Contact does not exist so add a new one
                linkContacts.Add(new LinkContactDto(Guid.NewGuid().ToString(), linkId, linkType, contact));

                // Can be used by other links
                existingContacts.Add(contact);
            }
            else
            {
                //  contact already exists add relationship
                linkContacts.Add(new LinkContactDto(Guid.NewGuid().ToString(), linkId, linkType, matchingContact));
            }

            return linkContacts;
        }

        internal static List<ContactDto> GetAllContactsFromOrganisation(OrganisationWithServicesDto? organisation)
        {
            var contacts = new List<ContactDto>();
            AddContactsToList(organisation?.LinkContacts?.Select(x => x.Contact), contacts);

            if(organisation?.Services == null)
            {
                return contacts;
            }

    
            var serviceContacts = organisation.Services?.Where(x=>x.LinkContacts!=null)
                .SelectMany(x => x.LinkContacts!.Select(x => x.Contact));
            AddContactsToList(serviceContacts, contacts);

            var serviceAtLocations = organisation.Services?.Where(x => x.ServiceAtLocations != null)
                .SelectMany(x => x.ServiceAtLocations!);
            if(serviceAtLocations != null && serviceAtLocations.Any())
            {
                var serviceAtLocationsContacts = serviceAtLocations?.Where(x => x.LinkContacts != null)
                    .SelectMany(x => x.LinkContacts!.Select(x => x.Contact));
                AddContactsToList(serviceAtLocationsContacts, contacts);

                var locationContacts = serviceAtLocations?.Where(x => x.LinkContacts != null)
                    .SelectMany(x => x.Location.LinkContacts!.Select(x => x.Contact));
                AddContactsToList(locationContacts, contacts);

            }

            return contacts;
        }

        internal static string ResolveLinkType(DataRow dtRow)
        {
            var deliveryMethod = dtRow["Delivery method"].ToString();
            if (string.IsNullOrEmpty(deliveryMethod))
                return string.Empty;

            if (deliveryMethod.Contains(DeliverMethods.IN_PERSON))
            {
                return LinkContactTypes.SERVICE_AT_LOCATION;
            }

            if (deliveryMethod.Contains(DeliverMethods.ONLINE) || deliveryMethod.Contains(DeliverMethods.TELEPHONE))
            {
                return LinkContactTypes.SERVICE;
            }

            throw new ArgumentException($"Unexpected value in delivery method :{deliveryMethod}");
        }

        private static void AddContactsToList(IEnumerable<ContactDto>? contacts, List<ContactDto> contactsList)
        {
            if(contacts != null && contacts.Any()) 
            {
                contactsList.AddRange(contacts.ToList());
            }
        }
    }
}
