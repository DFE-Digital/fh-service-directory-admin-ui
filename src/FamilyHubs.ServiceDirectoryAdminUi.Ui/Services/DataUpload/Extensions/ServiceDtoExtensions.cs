using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.DataUpload.Extensions
{
    public static class ServiceDtoExtensions
    {
        // if only one location and online, set contact at only service level
        public static void RationaliseContacts(this ServiceDto service)
        {
            if(service.Locations.Count > 1) return; // Multiple locations, therefore can have different contact at each

            if (service.Locations.Count == 0) return; // No Locations, nothing to rationalise

            var canBeDeliveredRemotely = service.ServiceDeliveries.Where(x => 
                x.Name == ServiceDeliveryType.Telephone || x.Name == ServiceDeliveryType.Online).Any();
            
            if (!canBeDeliveredRemotely) return; // Contacts wont have been added at service level, nothing to rationalise

            service.Locations.First().Contacts = new List<ContactDto>(); // Clear any contacts at the single location as the contact will have been added at service level
        }
    }
}
