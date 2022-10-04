using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServices;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;

public interface ISessionService
{
    //Org with service
    public OrganisationViewModel RetrieveOrganisationWithService(HttpContext httpContext);
    public void StoreOrganisationWithService(HttpContext httpContext, OrganisationViewModel vm);

    //Service
    public OpenReferralServiceDto RetrieveService(HttpContext httpContext);
    public void StoreService(HttpContext httpContext, OpenReferralServiceDto serviceDto);

    //Navigation - user journey (flow)
    public string RetrieveUserFlow(HttpContext httpContext);
    public void StoreUserFlow(HttpContext httpContext, string userFlow);
    
    //Navigation - last page name
    public string RetrieveLastPageName(HttpContext httpContext);
    public void StoreCurrentPageName(HttpContext httpContext, string currPage);
    public void ResetLastPageName(HttpContext httpContext);

}