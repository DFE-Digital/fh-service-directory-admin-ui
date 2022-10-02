using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServices;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;

public interface ISessionService
{
    public OrganisationViewModel RetrieveOrganisationWithService(HttpContext httpContext);
    public void StoreOrganisationWithService(HttpContext httpContext, OrganisationViewModel vm);

    public string RetrieveLastPageName(HttpContext httpContext);
    public void StoreCurrentPageName(HttpContext httpContext, string currPage);

    public OpenReferralServiceDto RetrieveService(HttpContext httpContext);
    public void StoreService(HttpContext httpContext, OpenReferralServiceDto serviceDto);
}
