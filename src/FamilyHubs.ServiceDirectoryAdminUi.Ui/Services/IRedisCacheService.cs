using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServices;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;

public interface IRedisCacheService
{
    //Org with service
    public OrganisationViewModel? RetrieveOrganisationWithService();
    public void StoreOrganisationWithService(OrganisationViewModel? vm);
    public void ResetOrganisationWithService();

    //Service
    public OpenReferralServiceDto? RetrieveService();
    public void StoreService(OpenReferralServiceDto serviceDto);

    //Navigation - user journey (flow)
    public string RetrieveUserFlow();
    public void StoreUserFlow(string userFlow);

    //Navigation - last page name
    public string RetrieveLastPageName();
    public void StoreCurrentPageName(string? currPage);
    public void ResetLastPageName();
}
