//using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
//using FamilyHubs.ServiceDirectory.Admin.Core.Models.ServiceJourney;
//using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
//using Microsoft.AspNetCore.Mvc;

//namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

//public class finish_edit_journeyModel : ServicePageModel
//{
//    public finish_edit_journeyModel(IRequestDistributedCache cache)
//        : base(ServiceJourneyPage.Finish_Edit_Journey, cache)
//    {
//    }

//    //todo: could we do this in the base instead? where nextpage???
//    protected override async Task<IActionResult> OnGetWithModelOverrideReturnAsync(CancellationToken cancellationToken)
//    {
//        ServiceModel!.FinishingEdit = true;
//        //todo: method in base to save model
//        await Cache.SetAsync(FamilyHubsUser.Email, ServiceModel);

//        // we need to pass on back too, as the base doesn't pass that on
//        return Redirect(GetServicePageUrl(ServiceJourneyPage.Service_Detail));
//    }
//}