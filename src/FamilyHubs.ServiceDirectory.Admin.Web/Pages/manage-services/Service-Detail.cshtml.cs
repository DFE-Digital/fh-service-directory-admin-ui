using System.Text;
using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

[Authorize(Roles = RoleGroups.AdminRole)]
public class Service_DetailModel : PageModel
{
    public long ServiceId { get; set; }
    public string? Name { get; set; }
    public string? ForChildren { get; set; }
    public HtmlString? Languages { get; set; }

    private readonly IServiceDirectoryClient _serviceDirectoryClient;

    public Service_DetailModel(IServiceDirectoryClient serviceDirectoryClient)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
    }

    public async Task OnGetAsync(long serviceId)
    {
        ServiceId = serviceId;
        var service = await _serviceDirectoryClient.GetServiceById(serviceId);
        Name = service.Name;
        ForChildren = GetForChildren(service);
        Languages = GetLanguages(service);
    }

    private static HtmlString GetLanguages(ServiceDto service)
    {
        StringBuilder languages = new(string.Join(", ", service.Languages.Select(l => l.Name)));

        if (!string.IsNullOrEmpty(service.InterpretationServices))
        {
            var intepretationServices = service.InterpretationServices?.Split(',');
            if (intepretationServices?.Any() == true)
            {
                //todo: should be part of the view
                languages.Append("<br>");

                string languageServices = string.Join(" and ", intepretationServices
                    .Select(s => s.Replace("bsl", "British Sign Language")
                        .Replace("translation", "translation services")));

                if (languageServices.Length > 0)
                {
                    languageServices = char.ToUpperInvariant(languageServices[0]) + languageServices[1..];
                }

                languages.Append($"{languageServices} available on request");
            }
        }

        return new HtmlString(languages.ToString());
    }

    private static string GetForChildren(ServiceDto service)
    {
        var eligibility = service.Eligibilities.FirstOrDefault();
        if (eligibility == null)
        {
            return "No";
        }

        // could be 0 years old (like Find & Connect) or 0 to 12 months, but 0 to 12 months to 1 year, for example looks odd!
        return $"Yes - {AgeToString(eligibility.MinimumAge)} years old to {AgeToString(eligibility.MaximumAge)} years old";
    }

    private static string AgeToString(int age)
    {
        return age == 127 ? "25+" : age.ToString();
    }
}