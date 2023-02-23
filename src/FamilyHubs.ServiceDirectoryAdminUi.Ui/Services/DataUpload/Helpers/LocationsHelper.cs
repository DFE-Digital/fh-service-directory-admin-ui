using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using Newtonsoft.Json;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.DataUpload.Helpers
{
    internal static class LocationsHelper
    {
        internal static async Task<ICollection<ServiceAtLocationDto>> GetServiceAtLocations(
            DataUploadRow dtRow,
            ServiceDto? service,
            List<string> errors,
            List<ContactDto> existingContacts,
            List<TaxonomyDto> taxonomies,
            Dictionary<string, PostcodesIoResponse> postCodesCache,
            IPostcodeLocationClientService postcodeLocationClientService)
        {
            ICollection<ServiceAtLocationDto> serviceAtLocations;
            if (service?.ServiceAtLocations is not null)
            {
                serviceAtLocations = service.ServiceAtLocations;
            }
            else
            {
                serviceAtLocations = new List<ServiceAtLocationDto>();
            }

            var postcodeApiModel = await TryGetPostcodesIoResponse(dtRow, postCodesCache, postcodeLocationClientService, errors);
            if (postcodeApiModel is null)
            {
                return serviceAtLocations;
            }

            var existingServiceAtLocation = GetExistingSeviceAtLocationForSameAddress(dtRow, service);
            var serviceAtLocation = CreateServiceAtLocation(dtRow, existingServiceAtLocation, postcodeApiModel, taxonomies, existingContacts, errors);

            if (existingServiceAtLocation is not null)
            {
                //compare and append
                if (!AreEqual(existingServiceAtLocation.LinkContacts, serviceAtLocation.LinkContacts))
                    existingServiceAtLocation.LinkContacts?.ToList()
                        .AddRange(serviceAtLocation.LinkContacts ?? new List<LinkContactDto>());
               
                if (!AreEqual(existingServiceAtLocation.RegularSchedules, serviceAtLocation.RegularSchedules))
                    existingServiceAtLocation.RegularSchedules?.ToList()
                        .AddRange(serviceAtLocation.RegularSchedules ?? new List<RegularScheduleDto>());
               
                if (!AreEqual(existingServiceAtLocation.HolidaySchedules, serviceAtLocation.HolidaySchedules))
                    existingServiceAtLocation.HolidaySchedules?.ToList()
                        .AddRange(serviceAtLocation.HolidaySchedules ?? new List<HolidayScheduleDto>());
               
                if (!AreEqual(existingServiceAtLocation.Location.LinkContacts, serviceAtLocation.Location.LinkContacts))
                    existingServiceAtLocation.Location.LinkContacts?.ToList()
                        .AddRange(serviceAtLocation.Location.LinkContacts ?? new List<LinkContactDto>());
               
                if (!AreEqual(existingServiceAtLocation.Location.LinkTaxonomies, serviceAtLocation.Location.LinkTaxonomies))
                    existingServiceAtLocation.Location.LinkTaxonomies?.ToList()
                        .AddRange(serviceAtLocation.Location.LinkTaxonomies ?? new List<LinkTaxonomyDto>());
               
                if (!AreEqual(existingServiceAtLocation.Location.PhysicalAddresses, serviceAtLocation.Location.PhysicalAddresses))
                    existingServiceAtLocation.Location.PhysicalAddresses?.ToList()
                        .AddRange(serviceAtLocation.Location.PhysicalAddresses ?? new List<PhysicalAddressDto>());
            }
            else
            {
                //create
                serviceAtLocations.Add(serviceAtLocation);
            }
            
            return serviceAtLocations;
        }

        private static ServiceAtLocationDto? GetExistingSeviceAtLocationForSameAddress(DataUploadRow dtRow, ServiceDto? service)
        {
            if (service is null || service.ServiceAtLocations is null)
            {
                return null;
            }

            return service.ServiceAtLocations.FirstOrDefault(x =>
                x.Location.Name == dtRow.LocationName &&
                x.Location.PhysicalAddresses?.FirstOrDefault(l => l.PostCode == dtRow.Postcode) != null);
        }

        private static ServiceAtLocationDto CreateServiceAtLocation(
            DataUploadRow dtRow,
            ServiceAtLocationDto? existingServiceAtLocation,
            PostcodesIoResponse postcodeApiModel,
            List<TaxonomyDto> taxonomies,
            List<ContactDto> existingContacts,
            List<string> errors)
        {
            var regularSchedule = GetRegularScheduleFromDataRow(dtRow, existingServiceAtLocation?.RegularSchedules?.First().Id);
            var address = GetAddressFromDataRow(dtRow, existingServiceAtLocation?.Location.PhysicalAddresses?.First().Id);
            var location = GetLocationFromDataRow(
                dtRow,
                existingServiceAtLocation,
                postcodeApiModel,
                address,
                taxonomies);

            var serviceAtLocationId = existingServiceAtLocation?.Id ?? Guid.NewGuid().ToString();
            var contacts = ContactHelper.GetLinkContacts(
                serviceAtLocationId, LinkContactTypes.SERVICE_AT_LOCATION, dtRow, existingServiceAtLocation?.LinkContacts, existingContacts, errors);

            var serviceAtLocation = new ServiceAtLocationDto(
                serviceAtLocationId,
                location,
                new List<RegularScheduleDto> { regularSchedule },
                new List<HolidayScheduleDto>(),
                contacts
            );

            return serviceAtLocation;

        }

        private static PhysicalAddressDto GetAddressFromDataRow(DataUploadRow dtRow, string? id)
        {
            if (string.IsNullOrEmpty(id))
            {
                id = Guid.NewGuid().ToString();
            }

            var addressLines = dtRow.AddressLineOne;
            if (!string.IsNullOrEmpty(dtRow.AddressLineTwo))
            {
                addressLines += " | " + dtRow.AddressLineTwo;
            }

            return new PhysicalAddressDto(
                                        id,
                                        addressLines ?? string.Empty,
                                        dtRow.TownOrCity,
                                        dtRow.Postcode!,
                                        "England",
                                        dtRow.County
                                        );
        }

        private static LocationDto GetLocationFromDataRow(
            DataUploadRow dtRow,
            ServiceAtLocationDto? existingServiceAtLocation,
            PostcodesIoResponse postcodeApiModel,
            PhysicalAddressDto physicalAddress,
            List<TaxonomyDto> taxonomies)
        {
            var id = existingServiceAtLocation?.Location.Id;
            if (string.IsNullOrEmpty(id))
            {
                id = Guid.NewGuid().ToString();
            }

            var linkTaxonomyList = GetLinkTaxonomyFromDataRow(dtRow, id, existingServiceAtLocation, taxonomies);

            var location = new LocationDto(
                id,
                dtRow.LocationName!,
                dtRow.LocationDescription,
                postcodeApiModel.Result.Latitude,
                postcodeApiModel.Result.Longitude,
                new List<PhysicalAddressDto>() { physicalAddress },
                linkTaxonomyList,
                new List<LinkContactDto>()
            );

            return location;
        }

        private static RegularScheduleDto GetRegularScheduleFromDataRow(DataUploadRow dtRow, string? id)
        {
            if (string.IsNullOrEmpty(id))
            {
                id = Guid.NewGuid().ToString();
            }

            return new RegularScheduleDto(
                id,
                dtRow.OpeningHoursDescription ?? string.Empty,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);
        }

        private static List<LinkTaxonomyDto> GetLinkTaxonomyFromDataRow(
            DataUploadRow dtRow, string locationId, ServiceAtLocationDto? existingServiceAtLocation, List<TaxonomyDto> taxonomies)
        {

            List<LinkTaxonomyDto> linkTaxonomyList = new();
            if (dtRow.OrganisationType?.ToLower() == "family hub")
            {
                var taxonomy = taxonomies.FirstOrDefault(x => x.Name == "FamilyHub");

                if (taxonomy != null)
                {
                    var linkTaxonomyId = existingServiceAtLocation?.Location.LinkTaxonomies?
                        .Where(x => x.Taxonomy?.Id == taxonomy.Id).First().Id ?? Guid.NewGuid().ToString();
                    linkTaxonomyList.Add(new LinkTaxonomyDto(linkTaxonomyId, "Location", locationId, taxonomy));
                }

            }

            return linkTaxonomyList;
        }

        private static async Task<PostcodesIoResponse?> TryGetPostcodesIoResponse(
            DataUploadRow dtRow,
            Dictionary<string, PostcodesIoResponse> postCodesCache,
            IPostcodeLocationClientService postcodeLocationClientService,
            List<string> errors)
        {
            var postcode = dtRow.Postcode;
            if (string.IsNullOrEmpty(postcode))
            {
                var deliveryMethod = dtRow.DeliveryMethod;
                if (deliveryMethod != null && deliveryMethod.Contains("In person"))
                {
                    errors.Add($"Postcode missing row: {dtRow.ExcelRowId}");
                }

                return null;
            }

            try
            {
                if (postCodesCache.ContainsKey(postcode))
                {
                    return postCodesCache[postcode];
                }
                else
                {
                    var postcodeApiModel = await postcodeLocationClientService.LookupPostcode(postcode);
                    postCodesCache[postcode] = postcodeApiModel;
                    return postcodeApiModel;
                }

            }
            catch
            {
                errors.Add($"Failed to find postcode: {postcode} row: {dtRow.ExcelRowId}");
                return null;
            }
        }

        private static bool AreEqual<T>(T expected, T actual)
        {
            var expectedJson = JsonConvert.SerializeObject(expected);
            var actualJson = JsonConvert.SerializeObject(actual);
            return expectedJson == actualJson;
        }
    }
}
