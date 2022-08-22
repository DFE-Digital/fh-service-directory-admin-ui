using LAHub.Domain.RecordEntities;
using WebUI.Models;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;

public class ApiModelToViewModelHelper
{
    public static OrganisationViewModel CreateViewModel(OpenReferralOrganisationWithServicesRecord apiModel, string serviceId)
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

        OpenReferralServiceRecord? openReferralServiceRecord = apiModel?.Services?.FirstOrDefault(x => x.Id == serviceId);
        if (openReferralServiceRecord != null)
        {
            organisationViewModel.ServiceId = openReferralServiceRecord.Id;
            organisationViewModel.ServiceName = openReferralServiceRecord.Name;
            organisationViewModel.ServiceDescription = openReferralServiceRecord.Description;
            organisationViewModel.InPersonSelection = openReferralServiceRecord?.Deliverable_type?.Split(',').ToList();
            organisationViewModel.Email = openReferralServiceRecord?.Email;
            organisationViewModel.Website = openReferralServiceRecord?.Url;

            if (openReferralServiceRecord?.Contacts != null)
            {
                var contact = openReferralServiceRecord.Contacts.FirstOrDefault();
                if (contact != null)
                {
                    if (contact.Phones != null && contact.Phones.Any())
                    {
                        organisationViewModel.Telephone = contact.Phones.First().Number;
                    }
                }
            }

            organisationViewModel.IsPayedFor = "No";
            if (openReferralServiceRecord?.Cost_options != null && openReferralServiceRecord.Cost_options.Any())
            {
                var cost = openReferralServiceRecord.Cost_options.FirstOrDefault();
                if (cost != null)
                {
                    organisationViewModel.IsPayedFor = "Yes";
                    organisationViewModel.PayUnit = cost.Amount_description;
                    organisationViewModel.Cost = cost.Amount;
                }
            }

            organisationViewModel.Languages = openReferralServiceRecord?.Languages?.Select(x => x.Language).ToList();

            if (openReferralServiceRecord?.Service_at_locations != null)
            {
                var serviceAtLocation = openReferralServiceRecord.Service_at_locations.FirstOrDefault();
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

            if (openReferralServiceRecord?.Service_taxonomys != null)
            {
                organisationViewModel.TaxonomySelection = new List<string>();
                foreach (var item in openReferralServiceRecord.Service_taxonomys)
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
