﻿using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.ServiceDirectory.Shared.Extensions;

namespace FamilyHubs.ServiceDirectory.Admin.Core.Services;

public static class ApiModelToViewModelHelper
{
    public static OrganisationViewModel CreateViewModel(OrganisationWithServicesDto apiModel, long serviceId)
    {
        var organisationViewModel = new OrganisationViewModel
        {
            Id = apiModel.Id,
            Name = apiModel.Name,
            Description = apiModel.Description,
            Logo = apiModel.Logo,
            Uri = apiModel.Uri,
            Url = apiModel.Url,
            Type = apiModel.OrganisationType.ToString()
        };

        //May be need to include service Id

        var serviceRecord = apiModel.Services.FirstOrDefault(x => x.Id == serviceId);
        if (serviceRecord != null)
        {
            organisationViewModel.ServiceId = serviceRecord.Id;
            organisationViewModel.ServiceOwnerReferenceId = serviceRecord.ServiceOwnerReferenceId;
            organisationViewModel.ServiceType = serviceRecord.ServiceType == ServiceType.InformationSharing ? "IS" : "FX";
            organisationViewModel.ServiceName = serviceRecord.Name;
            organisationViewModel.ServiceDescription = serviceRecord.Description;
            organisationViewModel.InPersonSelection = serviceRecord.Locations.Select(x=>x.Name).ToList();
            organisationViewModel.FamilyChoice = serviceRecord.CanFamilyChooseDeliveryLocation ? "Yes" : "No";

            GetEligibility(organisationViewModel, serviceRecord.Eligibilities);
            GetContacts(organisationViewModel, serviceRecord);

            organisationViewModel.IsPayedFor = "No";
            if (serviceRecord.CostOptions.Any())
            {
                var cost = serviceRecord.CostOptions.FirstOrDefault();
                if (cost != null)
                {
                    organisationViewModel.IsPayedFor = "Yes";
                    organisationViewModel.PayUnit = cost.Option;
                    organisationViewModel.Cost = cost.Amount;
                }

                organisationViewModel.CostDescriptions = new List<string>();
                foreach (var option in serviceRecord.CostOptions)
                {
                    if (!string.IsNullOrEmpty(option.AmountDescription))
                    {
                        organisationViewModel.CostDescriptions?.Add(option.AmountDescription);
                    }
                }
            }

            var serviceDeliveryListFromApiServiceRecord = serviceRecord.ServiceDeliveries.Select(x => x.Name.ToString()).ToList();
            
            organisationViewModel.ServiceDeliverySelection = ConvertServiceDeliverySelectionFromValueToId(serviceDeliveryListFromApiServiceRecord);

            organisationViewModel.Languages = serviceRecord.Languages.Select(x => x.Name).ToList();

            var location = serviceRecord.Locations.FirstOrDefault();
            if (location != null)
            {
                organisationViewModel.Latitude = location.Latitude;
                organisationViewModel.Longitude = location.Longitude;
                organisationViewModel.LocationName = location.Name;
                organisationViewModel.LocationDescription = location.Description;

                organisationViewModel.RegularSchedules = new List<string>();
                foreach (var schedule in location.RegularSchedules)
                {
                    if (!string.IsNullOrEmpty(schedule.Description))
                    {
                        organisationViewModel.RegularSchedules.Add(schedule.Description);
                    }
                }
                
                organisationViewModel.Address1 = location.Address1;
                organisationViewModel.City = location.City;
                organisationViewModel.Country = location.Country;
                organisationViewModel.PostalCode = location.PostCode;
                organisationViewModel.StateProvince = location.StateProvince;
            }

            organisationViewModel.TaxonomySelection = new List<long>();
            foreach (var item in serviceRecord.Taxonomies)
            {
                var id = item.Id;
                organisationViewModel.TaxonomySelection.Add(id);
            }
        }

        return organisationViewModel;
    }

    private static void GetContacts(OrganisationViewModel organisationViewModel, ServiceDto serviceRecord)
    {
        //  Note currently only resolving one contact per service record as the data upload does not allow for more contacts. 
        //  This implementation will need to change in the future
        var contact = serviceRecord.GetContact();

        if (contact == null) return;

        organisationViewModel.Telephone = contact.Telephone;
        organisationViewModel.TextPhone = contact.TextPhone;
        organisationViewModel.Email = contact.Email;
        organisationViewModel.Website = contact.Url;
    }

    private static List<string> ConvertServiceDeliverySelectionFromValueToId(IEnumerable<string> serviceDeliverySelectionValues)
    {
        var myEnumDescriptions = from ServiceDeliveryType n in Enum.GetValues(typeof(ServiceDeliveryType))
                                 select new { Id = (int)n, Name = n.ToString() };

        var dictServiceDelivery = new Dictionary<string, string>();
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
            if(e.EligibilityType == EligibilityType.Child)
                organisationViewModel.Children = "Yes";

            organisationViewModel.MinAge = e.MinimumAge;
            organisationViewModel.MaxAge = e.MaximumAge;
            organisationViewModel.WhoForSelection?.Add(e.EligibilityType.ToString());
        }
    }
}
