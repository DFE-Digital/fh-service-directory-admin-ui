using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using Microsoft.AspNetCore.Authorization;
using System.Globalization;
using System.Runtime;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;


[Authorize]
public class ServiceDetailModel : HeaderPageModel
{
    public string? Title { get; set; }
    public List<TaxonomyDto> Taxonomies { get; set; } = new();

    public string Description { get; set; } = "";


    public string Languages { get; set; } = "";

    public List<string> Costs { get; set; } = new();

    public List<ContactDto> Contacts { get; set; } = new List<ContactDto>();

    public string? Eligibility { get; set; } = "";

    public List<string> Shedule { get; set; } = new();
    public string ServiceDeliveries { get; set; } = "";

    private readonly IServiceDirectoryClient _serviceDirectoryClient;

    public ServiceDetailModel(IServiceDirectoryClient serviceDirectoryClient)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
    }

    public async Task OnGetAsync(long id)
    {
        var service = await _serviceDirectoryClient.GetServiceById(id);

        if (service != null)
        {
            Title = service.Name;
            Taxonomies = service.Taxonomies.ToList();
            Description = service.Description ?? "";
            Languages = string.Join(", ", service.Languages.Select(x => x.Name));
            Contacts = service.Contacts.ToList();
            Costs = GetCost(service).ToList();
            Eligibility = GetAgeRange(service.Eligibilities.FirstOrDefault());
            Shedule = GetWhen(service).ToList();
            ServiceDeliveries = string.Join(", ", service.ServiceDeliveries.Select(x => x.Name));
        }

    }

    private static IEnumerable<string> GetWhen(ServiceDto service)
    {
        var when =
            service.RegularSchedules.FirstOrDefault()?.Description?.Split('\n').Select(l => l.Trim())
            ?? Enumerable.Empty<string>();
        return when;
    }

    private static string? GetAgeRange(EligibilityDto? eligibility)
    {
        var forChildAndYoungPeople = "No - ";
        if (eligibility?.EligibilityType == ServiceDirectory.Shared.Enums.EligibilityType.Child
            || eligibility?.EligibilityType == ServiceDirectory.Shared.Enums.EligibilityType.Teen)
        {
            forChildAndYoungPeople = "Yes - ";
        }

        return eligibility == null ? null : $"{forChildAndYoungPeople}{AgeToString(eligibility.MinimumAge)} to {AgeToString(eligibility.MaximumAge)}";
    }

    private static string AgeToString(int age)
    {
        return age == 127 ? "25+ years" : (age == 1 ? age.ToString() + " year" : age.ToString() + " years");
    }

    private static IEnumerable<string> GetCost(ServiceDto service)
    {
        const string Free = "Free";

        if (!service.CostOptions.Any())
        {
            return new[] { Free };
        }

        var cost = new List<string>();
        var firstCost = service.CostOptions.First();

        if (firstCost.Amount is null or decimal.Zero)
        {
            cost.Add(Free);
        }

        if (firstCost.Amount is not null && firstCost.Amount != decimal.Zero)
        {
            var ukNumberFormat = new CultureInfo("en-GB", false).NumberFormat;
            var amount = firstCost.Amount.Value.ToString("C0", ukNumberFormat);
            var message = $"Yes - {amount}";
            if (!string.IsNullOrWhiteSpace(firstCost.Option))
            {
                message += $" every {firstCost.Option?.ToLowerInvariant()}";
            }

            cost.Add(message);
        }

        if (!string.IsNullOrWhiteSpace(firstCost.AmountDescription))
        {
            cost.Add(firstCost.AmountDescription);
        }

        return cost;
    }
}