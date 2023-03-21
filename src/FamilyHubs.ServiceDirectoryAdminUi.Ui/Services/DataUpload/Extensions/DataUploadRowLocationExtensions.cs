using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using System;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.DataUpload.Extensions
{
    public static class DataUploadRowLocationExtensions
    {
        public static async Task UpdateLocations(this DataUploadRowDto row, ServiceDto? existingService, ServiceDto service, IPostcodeLocationClientService postcodeLocationClientService)
        {
            try
            {
                if (string.IsNullOrEmpty(row.Postcode)) return;
                var postCodeData = await postcodeLocationClientService.LookupPostcode(row.Postcode);
                var location = GetLocationFromRow(row, postCodeData);
                if (location == null) return;

                //  Check if location is already in the DB, if it is update its properties
                var existLocationRecord = GetMatchingLocation(row, existingService, postCodeData);
                if (existLocationRecord != null)
                {
                    UpdateExistingLocationRecord(existLocationRecord, location);
                    UpdateLocationContacts(existLocationRecord, row);
                    return;
                }

                //  Check if the location is already in the serviceDto to be uploaded, if it is update its properties
                var previouslyAddedLocation = GetMatchingLocation(row, service, postCodeData);
                if (previouslyAddedLocation != null)
                {
                    UpdateLocationContacts(previouslyAddedLocation, row);
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

        private static void UpdateExistingLocationRecord(LocationDto existinglocation, LocationDto locationFromRow)
        {
            existinglocation.Description = locationFromRow.Description;
            existinglocation.Latitude = locationFromRow.Latitude;
            existinglocation.Longitude = locationFromRow.Longitude;
            existinglocation.Address1 = locationFromRow.Address1;
            existinglocation.Address2 = locationFromRow.Address2;
            existinglocation.City = locationFromRow.City;
            existinglocation.StateProvince = locationFromRow.StateProvince;
            existinglocation.Country = locationFromRow.Country;
        }

        private static void UpdateLocationContacts(LocationDto location, DataUploadRowDto row)
        {
            if (row.DeliveryMethod != ServiceDeliveryType.InPerson)
                return;

            var contact = row.GetContactFromRow();
            if (contact == null) return;

            var contactExists = location.Contacts.Where(x => 
                x.Email == contact.Email &&
                x.Telephone == contact.Telephone &&
                x.Url == contact.Url &&
                x.TextPhone == contact.TextPhone
            ).Any();

            if (!contactExists)
                location.Contacts.Add(contact);
        }

        private static LocationDto? GetMatchingLocation(DataUploadRowDto row, ServiceDto? service, PostcodesIoResponse postCodeIoResponse)
        {
            if(service == null) return null;
            return service.Locations.Where(x => x.Name == row.LocationName && x.PostCode == postCodeIoResponse.Result.Postcode).FirstOrDefault();
        }

        private static LocationDto? GetLocationFromRow(DataUploadRowDto row, PostcodesIoResponse postCodeData)
        {
            if (string.IsNullOrEmpty(row.LocationName) || string.IsNullOrEmpty(row.Postcode) || string.IsNullOrEmpty(row.AddressLineOne) || string.IsNullOrEmpty(row.TownOrCity) || string.IsNullOrEmpty(row.County))
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
                StateProvince = row.County,
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
