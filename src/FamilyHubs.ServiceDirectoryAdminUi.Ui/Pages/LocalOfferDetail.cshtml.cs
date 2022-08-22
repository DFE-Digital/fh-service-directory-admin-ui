using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using LAHub.Domain.RecordEntities;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages
{
    public class LocalOfferDetailModel : PageModel
    {
        private readonly ILocalOfferClientService _localOfferClientService;

        public OpenReferralServiceRecord LocalOffer { get; set; } = default!;

        public LocalOfferDetailModel(ILocalOfferClientService localOfferClientService)
        {
            _localOfferClientService = localOfferClientService;
        }

        public async Task OnGetAsync(string id)
        {
            LocalOffer = await _localOfferClientService.GetLocalOfferById(id);
        }
    }
}
