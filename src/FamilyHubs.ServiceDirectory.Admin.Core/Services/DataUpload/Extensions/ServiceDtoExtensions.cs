using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;

namespace FamilyHubs.ServiceDirectory.Admin.Core.Services.DataUpload.Extensions;

public static class ServiceDtoExtensions
{
    // if only one location and online, set contact at only service level
    public static void RationaliseContacts(this ServiceDto service)
    {
        if(service.Locations.Count is 0 or > 1 ) return; // Multiple locations, therefore can have different contact at each

        var canBeDeliveredRemotely = service.ServiceDeliveries.Any(x => x.Name is ServiceDeliveryType.Telephone or ServiceDeliveryType.Online);
            
        if (!canBeDeliveredRemotely) return; // Contacts wont have been added at service level, nothing to rationalise

        service.Locations.First().Contacts = new List<ContactDto>(); // Clear any contacts at the single location as the contact will have been added at service level
    }
}