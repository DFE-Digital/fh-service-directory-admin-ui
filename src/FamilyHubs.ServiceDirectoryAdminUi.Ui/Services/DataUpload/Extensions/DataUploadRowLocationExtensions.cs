using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.DataUpload.Extensions
{
    public static class DataUploadRowLocationExtensions
    {
        public static async Task UpdateLocations(this DataUploadRowDto row, ServiceDto? existingService, ServiceDto service, IPostcodeLocationClientService postcodeLocationClientService)
        {
            PostcodesIoResponse? postCodeData = null;
            try
            {
                if (string.IsNullOrEmpty(row.Postcode)) return;
                postCodeData = await postcodeLocationClientService.LookupPostcode(row.Postcode);
            }
            catch (Exception exp)
            {
                var msg = $"There was an error while trying to resolve postcode on row {row.ExcelRowId}";
                throw new DataUploadException(msg, exp);
            }

            try
            {

                var location = GetLocationFromRow(row, postCodeData!);
                if (location == null) return;

                //  Check if the location is already in the serviceDto to be uploaded, if it is update its properties
                var previouslyAddedLocation = GetMatchingLocation(row, service, postCodeData);
                if (previouslyAddedLocation != null)
                {
                    UpdateLocationContacts(previouslyAddedLocation, row);
                    return;
                }

                //  Check if location is already in the DB, if it is update its properties
                var existLocationRecord = GetMatchingLocation(row, existingService, postCodeData);
                if (existLocationRecord != null)
                {
                    UpdateLocationWithDbData(existLocationRecord, location);
                    UpdateLocationContacts(location, row, existLocationRecord.Contacts);
                    service.Locations.Add(location);
                    return;
                }

                //  Location not yet added
                UpdateLocationContacts(location, row);
                service.Locations.Add(location);
            }
            catch (Exception ex)
            {
                var msg = $"There was an error while attempting to UpdateLocations on row {row.ExcelRowId}";
                throw new DataUploadException(msg, ex);
            }
        }

        private static void UpdateLocationWithDbData(LocationDto existinglocation, LocationDto locationFromRow)
        {
            locationFromRow.Id = existinglocation.Id;
            locationFromRow.AccessibilityForDisabilities = existinglocation.AccessibilityForDisabilities;
            locationFromRow.HolidaySchedules = existinglocation.HolidaySchedules;
            locationFromRow.LocationType = existinglocation.LocationType;
            locationFromRow.Name = existinglocation.Name;
            locationFromRow.RegularSchedules = existinglocation.RegularSchedules;
        }

        private static void UpdateLocationContacts(LocationDto location, DataUploadRowDto row, ICollection<ContactDto>? existingContactDtos = null)
        {
            if (row.DeliveryMethod != ServiceDeliveryType.InPerson)
                return;

            var contact = row.GetContactFromRow();
            if (contact == null) return;

            //  If the contact is already added exit
            var existingContact = location.Contacts.GetMatchingContact(contact);
            if (existingContact != null)
                return;

            //  If the contact exists in the db, add that record with its Ids to the list and exit
            existingContact = existingContactDtos.GetMatchingContact(contact);
            if (existingContact != null)
            {
                location.Contacts.Add(existingContact);
                return;
            }

            //  Location has not been added yet, add it now
            location.Contacts.Add(contact);
        }

        private static LocationDto? GetMatchingLocation(DataUploadRowDto row, ServiceDto? service, PostcodesIoResponse postCodeIoResponse)
        {
            if(service == null) return null;
            return service.Locations.Where(x => x.Name == row.LocationName && x.PostCode == postCodeIoResponse.Result.Postcode).FirstOrDefault();
        }

        private static LocationDto? GetLocationFromRow(DataUploadRowDto row, PostcodesIoResponse postCodeData)
        {
            if (string.IsNullOrEmpty(row.LocationName) || string.IsNullOrEmpty(row.Postcode) || string.IsNullOrEmpty(row.AddressLineOne) )
            {
                return null;
            }

            var location = new LocationDto
            {
                Name = row.LocationName,
                Description = row.LocationDescription,
                Address1 = row.AddressLineOne,
                Address2 = row.AddressLineTwo,
                City = row.TownOrCity,
                StateProvince = row.County ?? string.Empty,
                Country = postCodeData.Result.Country ?? "England",
                PostCode = postCodeData.Result.Postcode,
                Latitude = postCodeData.Result.Latitude,
                Longitude = postCodeData.Result.Longitude,
                LocationType = LocationType.NotSet,
            };

            return location;
        }
    }
}
