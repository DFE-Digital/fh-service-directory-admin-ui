using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServices;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using System.ComponentModel.DataAnnotations;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;
public class DeleteServiceModel : PageModel
{
    private readonly ILocalOfferClientService _localOfferClientService;
    private readonly ISessionService _session;
    private readonly IRedisCacheService _redis;

    public DeleteServiceModel(ILocalOfferClientService localOfferClientService,
                              ISessionService sessionService,
                              IRedisCacheService redisCacheService)
    {
        _localOfferClientService = localOfferClientService;
        _session = sessionService;
        _redis = redisCacheService;
    }

    public List<string> SelectOptions { get; set; } = default!;
    [BindProperty] public string SelectedOption { get; set; } = default!;
    private const string OptionYes = "Yes, I want to delete it";
    private const string OptionNo = "No, I want to keep it";
    public OpenReferralServiceDto Service { get; private set; } = default!;

    public async Task OnGet(string organisationid, string serviceid)
    {
        if (!string.IsNullOrEmpty(serviceid))
        {
            Service = await _localOfferClientService.GetLocalOfferById(serviceid);
        }
        else
        {
            //TODO - throw exception;
        }
        SelectOptions = new List<string>() { OptionYes, OptionNo };
    }

    public async Task<IActionResult> OnPost(string serviceId)
    {
        if (SelectedOption == OptionYes)
        {
            bool serviceDeleted = await _localOfferClientService.DeleteServiceById(serviceId);

            if (!serviceDeleted)
            {
                return RedirectToPage("/OrganisationAdmin/ServiceNotDeleted");
            }
            else
            {
                return RedirectToPage("/OrganisationAdmin/ServiceDeleted");
            }
        }

        return RedirectToPage("/OrganisationAdmin/ServiceNotDeleted");
    }
}
