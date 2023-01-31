using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages;

public class LocalOfferDetailModel : PageModel
{
    private readonly ILocalOfferClientService _localOfferClientService;

    public ServiceDto LocalOffer { get; set; } = default!;

    public LocalOfferDetailModel(ILocalOfferClientService localOfferClientService)
    {
        _localOfferClientService = localOfferClientService;
    }

    public async Task OnGetAsync(string id)
    {
        LocalOffer = await _localOfferClientService.GetLocalOfferById(id);
    }
}
