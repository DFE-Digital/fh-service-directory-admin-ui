using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralEligibilitys;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralOrganisations;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServices;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Extensions;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;

public class ApiModelToViewModelHelper
{
    public static OrganisationViewModel CreateViewModel(OpenReferralOrganisationWithServicesDto apiModel, string serviceId)
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

        OpenReferralServiceDto? openReferralServiceRecord = apiModel?.Services?.FirstOrDefault(x => x.Id == serviceId);
        if (openReferralServiceRecord != null)
        {
            organisationViewModel.ServiceId = openReferralServiceRecord.Id;
            organisationViewModel.ServiceType = (openReferralServiceRecord.ServiceType.Id == "1") ? "IS" : "FX";
            organisationViewModel.ServiceName = openReferralServiceRecord.Name;
            organisationViewModel.ServiceDescription = openReferralServiceRecord.Description;
            organisationViewModel.InPersonSelection = openReferralServiceRecord?.Deliverable_type?.Split(',').ToList();
            organisationViewModel.Email = openReferralServiceRecord?.Email;
            organisationViewModel.Website = openReferralServiceRecord?.Url;
            organisationViewModel.Familychoice = (openReferralServiceRecord?.CanFamilyChooseDeliveryLocation == true) ? "Yes" : "No";

            GetEligibilities(organisationViewModel, openReferralServiceRecord?.Eligibilities);

            if (openReferralServiceRecord?.Contacts != null)
            {   
                GetContacts(organisationViewModel, openReferralServiceRecord);
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
                    organisationViewModel.CostDescription = cost.Option;

                }
         
            }

            var serviceDeliveryListFromApiServiceRecord = openReferralServiceRecord?.ServiceDelivery?
                                                                                    .Select(x => x.ServiceDelivery.ToString())
                                                                                    .ToList();
            
            if (serviceDeliveryListFromApiServiceRecord != null)
                organisationViewModel.ServiceDeliverySelection = ConvertServiceDeliverySelectionFromValueToId(serviceDeliveryListFromApiServiceRecord);


            var defaultLanguages = openReferralServiceRecord?.Languages?.First().Language;
            organisationViewModel.Languages = new List<string>();

            if (defaultLanguages != null)
            {
                var languages = defaultLanguages.SplitByLineBreaks();
                foreach (var language in languages)
                {
                    organisationViewModel.Languages.Add(language);
                }
            }


            if (openReferralServiceRecord?.Service_at_locations != null)
            {
                var serviceAtLocation = openReferralServiceRecord.Service_at_locations.FirstOrDefault();
                if (serviceAtLocation != null)
                {
                    organisationViewModel.Latitude = serviceAtLocation.Location.Latitude;
                    organisationViewModel.Longtitude = serviceAtLocation.Location.Longitude;
                    organisationViewModel.LocationName = serviceAtLocation.Location.Name;
                    organisationViewModel.LocationDescription = serviceAtLocation.Location.Description;

                    organisationViewModel.RegularSchedules = new List<string>();
                    foreach (var schedule in serviceAtLocation.Regular_schedule!)
                    {
                        if (schedule != null)
                        {
                           var splitSchedules =  schedule.Description.SplitByLineBreaks();
                            foreach (var splitSchedule in splitSchedules)
                            {
                                organisationViewModel.RegularSchedules.Add(splitSchedule);
                            }
                        }
                    }
                        

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

    private static void GetContacts(OrganisationViewModel organisationViewModel, OpenReferralServiceDto openReferralServiceRecord)
    {
        if (openReferralServiceRecord == null || openReferralServiceRecord.Contacts == null)
            return;

        foreach (var contact in openReferralServiceRecord.Contacts)
        {
            if (contact == null)
                continue;

            //Telephone
            if (contact.Name == "Telephone")
            {
                if (contact.Phones != null && contact.Phones.Any())
                {
                    organisationViewModel.Telephone = contact.Phones.First().Number;
                }
            }

            //Textphone
            if (contact.Name == "Textphone")
            {
                if (contact.Phones != null && contact.Phones.Any())
                {
                    organisationViewModel.Textphone = contact.Phones.First().Number;
                }
            }
        }

    }

    private static List<string> ConvertServiceDeliverySelectionFromValueToId(List<string> ServiceDeliverySelectionValues)
    {
        List<string> result = new List<string>();

        var myEnumDescriptions = from ServiceDelivery n in Enum.GetValues(typeof(ServiceDelivery))
                                 select new { Id = (int)n, Name = n.ToString() };

        Dictionary<string, string> dictServiceDelivery = new();
        foreach (var myEnumDescription in myEnumDescriptions)
        {
            if (myEnumDescription.Id == 0)
                continue;
            dictServiceDelivery[myEnumDescription.Name] = myEnumDescription.Id.ToString();
        }

        if (ServiceDeliverySelectionValues != null)
        {
            foreach (var value in ServiceDeliverySelectionValues)
            {
                result.Add(dictServiceDelivery[value]);
            }
        }

        return result;
    }

    private static void GetEligibilities(OrganisationViewModel organisationViewModel, ICollection<OpenReferralEligibilityDto>? eligibilities)
    {
        if (eligibilities == null)
            return;

        if (organisationViewModel.WhoForSelection == null)
            organisationViewModel.WhoForSelection = new List<string>();

        foreach (var e in eligibilities)
        {
            if(e.Eligibility == "Children")
                organisationViewModel.Children = "Yes";

            organisationViewModel.MinAge = e.Minimum_age;
            organisationViewModel.MaxAge = e.Maximum_age;
            organisationViewModel.WhoForSelection?.Add(e.Eligibility);
        }
        
    }
}
