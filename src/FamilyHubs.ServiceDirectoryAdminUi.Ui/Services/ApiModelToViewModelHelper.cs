using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;

public class ApiModelToViewModelHelper
{
    public static OrganisationViewModel CreateViewModel(OrganisationWithServicesDto apiModel, string serviceId)
    {
        OrganisationViewModel organisationViewModel = new()
        {
            Id = new Guid(apiModel.Id),
            Name = apiModel.Name,
            Description = apiModel.Description,
            Logo = apiModel.Logo,
            Uri = apiModel.Uri,
            Url = apiModel.Url,
            Type = apiModel.OrganisationType.Name
        };

        //May be need to include service Id

        var serviceRecord = apiModel.Services?.FirstOrDefault(x => x.Id == serviceId);
        if (serviceRecord != null)
        {
            organisationViewModel.ServiceId = serviceRecord.Id;
            organisationViewModel.ServiceType = (serviceRecord.ServiceType.Id == "1") ? "IS" : "FX";
            organisationViewModel.ServiceName = serviceRecord.Name;
            organisationViewModel.ServiceDescription = serviceRecord.Description;
            organisationViewModel.InPersonSelection = serviceRecord.DeliverableType?.Split(',').ToList();
            organisationViewModel.Familychoice = serviceRecord.CanFamilyChooseDeliveryLocation ? "Yes" : "No";

            GetEligibility(organisationViewModel, serviceRecord.Eligibilities);

            //if (serviceRecord.Contacts != null)
            //{   
            //    GetContacts(organisationViewModel, serviceRecord);
            //}

            organisationViewModel.IsPayedFor = "No";
            if (serviceRecord.CostOptions != null && serviceRecord.CostOptions.Any())
            {
                var cost = serviceRecord.CostOptions.FirstOrDefault();
                if (cost != null)
                {
                    organisationViewModel.IsPayedFor = "Yes";
                    organisationViewModel.PayUnit = cost.AmountDescription;
                    organisationViewModel.Cost = cost.Amount;
                }

                organisationViewModel.CostDescriptions = new List<string>();
                foreach (var option in serviceRecord.CostOptions)
                    organisationViewModel.CostDescriptions?.Add(option.AmountDescription);
            }

            var serviceDeliveryListFromApiServiceRecord = serviceRecord.ServiceDeliveries?
                                                                                    .Select(x => x.Name.ToString())
                                                                                    .ToList();
            
            if (serviceDeliveryListFromApiServiceRecord != null)
                organisationViewModel.ServiceDeliverySelection = ConvertServiceDeliverySelectionFromValueToId(serviceDeliveryListFromApiServiceRecord);

            organisationViewModel.Languages = serviceRecord.Languages?.Select(x => x.Name).ToList();

            if (serviceRecord.ServiceAtLocations != null)
            {
                var serviceAtLocation = serviceRecord.ServiceAtLocations.FirstOrDefault();
                if (serviceAtLocation != null)
                {
                    organisationViewModel.Latitude = serviceAtLocation.Location.Latitude;
                    organisationViewModel.Longtitude = serviceAtLocation.Location.Longitude;
                    organisationViewModel.LocationName = serviceAtLocation.Location.Name;
                    organisationViewModel.LocationDescription = serviceAtLocation.Location.Description;

                    organisationViewModel.RegularSchedules = new List<string>();
                    foreach (var schedule in serviceAtLocation.RegularSchedules!)
                        organisationViewModel.RegularSchedules.Add(schedule.Description);

                    if (serviceAtLocation.Location.PhysicalAddresses != null && serviceAtLocation.Location.PhysicalAddresses.Any())
                    {
                        var address = serviceAtLocation.Location.PhysicalAddresses.FirstOrDefault();
                        if (address != null)
                        {
                            organisationViewModel.Address_1 = address.Address1;
                            organisationViewModel.City = address.City;
                            organisationViewModel.Country = address.Country;
                            organisationViewModel.Postal_code = address.PostCode;
                            organisationViewModel.State_province = address.StateProvince;
                        }
                    }
                }
            }

            if (serviceRecord.ServiceTaxonomies != null)
            {
                organisationViewModel.TaxonomySelection = new List<string>();
                foreach (var item in serviceRecord.ServiceTaxonomies)
                {
                    if (item.Taxonomy != null)
                    {
                        var id = item.Taxonomy?.Id;
                        if (id != null)
                            organisationViewModel.TaxonomySelection.Add(id);
                    }

                }
                
            }

        }

        return organisationViewModel;
    }

    private static void GetContacts(OrganisationViewModel organisationViewModel, ServiceDto serviceRecord)
    {
        //if (serviceRecord.Contacts == null)
        //    return;

        //foreach (var contact in serviceRecord.Contacts)
        //{
        //    //Telephone
        //    organisationViewModel.Telephone = contact.Telephone;
        //    organisationViewModel.Textphone = contact.TextPhone;
        //    organisationViewModel.Email = contact.Email;
        //    organisationViewModel.Website = contact.Url;
        //}

    }

    private static List<string> ConvertServiceDeliverySelectionFromValueToId(List<string> serviceDeliverySelectionValues)
    {
        var myEnumDescriptions = from ServiceDeliveryType n in Enum.GetValues(typeof(ServiceDeliveryType))
                                 select new { Id = (int)n, Name = n.ToString() };

        Dictionary<string, string> dictServiceDelivery = new();
        foreach (var myEnumDescription in myEnumDescriptions)
        {
            if (myEnumDescription.Id == 0)
                continue;
            dictServiceDelivery[myEnumDescription.Name] = myEnumDescription.Id.ToString();
        }

        return serviceDeliverySelectionValues.Select(value => dictServiceDelivery[value]).ToList();
    }

    private static void GetEligibility(OrganisationViewModel organisationViewModel, ICollection<EligibilityDto>? eligibility)
    {
        if (eligibility == null)
            return;

        organisationViewModel.WhoForSelection ??= new List<string>();

        foreach (var e in eligibility)
        {
            if(e.EligibilityDescription == "Children")
                organisationViewModel.Children = "Yes";

            organisationViewModel.MinAge = e.MinimumAge;
            organisationViewModel.MaxAge = e.MaximumAge;
            organisationViewModel.WhoForSelection?.Add(e.EligibilityDescription);
        }
    }
}
