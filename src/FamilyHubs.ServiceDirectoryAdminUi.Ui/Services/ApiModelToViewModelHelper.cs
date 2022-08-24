using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralOrganisations;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServices;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;

public class ApiModelToViewModelHelper
{
    public static OrganisationViewModel CreateViewModel(IOpenReferralOrganisationWithServicesDto apiModel, string serviceId)
    {
        OrganisationViewModel organisationViewModel = new()
        {
            Id = new Guid(apiModel.Id),
            Name = apiModel.Name,
            Description = apiModel.Description,
            Logo = apiModel.Logo,
            Uri = apiModel.Uri,
            Url = apiModel.Url,
        };

        //May be need to include service Id

        IOpenReferralServiceDto? openReferralServiceDto = apiModel?.Services?.FirstOrDefault(x => x.Id == serviceId);
        if (openReferralServiceDto != null)
        {
            organisationViewModel.ServiceId = openReferralServiceDto.Id;
            organisationViewModel.ServiceName = openReferralServiceDto.Name;
            organisationViewModel.ServiceDescription = openReferralServiceDto.Description;
            organisationViewModel.InPersonSelection = openReferralServiceDto?.Deliverable_type?.Split(',').ToList();
            organisationViewModel.Email = openReferralServiceDto?.Email;
            organisationViewModel.Website = openReferralServiceDto?.Url;

            if (openReferralServiceDto?.Contacts != null)
            {
                var contact = openReferralServiceDto.Contacts.FirstOrDefault();
                if (contact != null)
                {
                    if (contact.Phones != null && contact.Phones.Any())
                    {
                        organisationViewModel.Telephone = contact.Phones.First().Number;
                    }
                }
            }

            organisationViewModel.IsPayedFor = "No";
            if (openReferralServiceDto?.Cost_options != null && openReferralServiceDto.Cost_options.Any())
            {
                var cost = openReferralServiceDto.Cost_options.FirstOrDefault();
                if (cost != null)
                {
                    organisationViewModel.IsPayedFor = "Yes";
                    organisationViewModel.PayUnit = cost.Amount_description;
                    organisationViewModel.Cost = cost.Amount;
                }
            }

            organisationViewModel.ServiceDeliverySelection = openReferralServiceDto?.ServiceDelivery?.Select(x => x.ServiceDelivery.ToString()).ToList();

            organisationViewModel.Languages = openReferralServiceDto?.Languages?.Select(x => x.Language).ToList();

            if (openReferralServiceDto?.Service_at_locations != null)
            {
                var serviceAtLocation = openReferralServiceDto.Service_at_locations.FirstOrDefault();
                if (serviceAtLocation != null)
                {
                    organisationViewModel.Latitude = serviceAtLocation.Location.Latitude;
                    organisationViewModel.Longtitude = serviceAtLocation.Location.Longitude;

                    if (serviceAtLocation.Location.Physical_addresses != null && serviceAtLocation.Location.Physical_addresses.Any())
                    {
                        var address = serviceAtLocation.Location.Physical_addresses.FirstOrDefault();
                        if (address != null)
                        {
                            organisationViewModel.Address_1 = address.Address_1?.ToString();
                            organisationViewModel.City = address.City;
                            organisationViewModel.Country = address.Country;
                            organisationViewModel.Postal_code = address.Postal_code;
                            organisationViewModel.State_province = address.State_province;
                        }
                    }
                }
            }

            if (openReferralServiceDto?.Service_taxonomys != null)
            {
                organisationViewModel.TaxonomySelection = new List<string>();
                foreach (var item in openReferralServiceDto.Service_taxonomys)
                {
                    if (item != null && item.Taxonomy != null && item.Taxonomy.Id != null)
                    {
                        var id = item?.Taxonomy?.Id;
                        if (id != null)
                            organisationViewModel.TaxonomySelection.Add(id);
                    }

                }
            }

        }

        return organisationViewModel;
    }
}
