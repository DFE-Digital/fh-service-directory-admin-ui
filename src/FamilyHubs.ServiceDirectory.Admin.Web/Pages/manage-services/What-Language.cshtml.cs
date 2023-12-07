using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public class What_LanguageModel : ServicePageModel
{
    public What_LanguageModel(IRequestDistributedCache connectionRequestCache)
        : base(ServiceJourneyPage.What_Language, connectionRequestCache)
    {
    }
}