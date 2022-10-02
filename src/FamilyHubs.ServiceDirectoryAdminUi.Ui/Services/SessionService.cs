using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServices;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Extensions;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using static FamilyHubs.ServiceDirectoryAdminUi.Ui.Infrastructure.Configuration.SessionConfiguration;


namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;

public class SessionService : ISessionService
{
    public OrganisationViewModel RetrieveOrganisationWithService(HttpContext httpContext)
    {
        return httpContext.Session.Get<OrganisationViewModel>(SessionKeyOrgWithService);
    }
    public void StoreOrganisationWithService(HttpContext httpContext, OrganisationViewModel vm)
    {
        httpContext.Session.Set<OrganisationViewModel>(SessionKeyOrgWithService, vm);
    }

    public string RetrieveLastPageName(HttpContext httpContext)
    {
        return httpContext.Session.Get<string>(SessionKeyCurrentPage);
    }

    public void StoreCurrentPageName(HttpContext httpContext, string currPage)
    {
        httpContext.Session.Set<string>(SessionKeyCurrentPage, currPage);
    }

    public OpenReferralServiceDto RetrieveService(HttpContext httpContext)
    {
        return httpContext.Session.Get<OpenReferralServiceDto>(SessionKeyService);
    }

    public void StoreService(HttpContext httpContext, OpenReferralServiceDto serviceDto)
    {
        httpContext.Session.Set<OpenReferralServiceDto>(SessionKeyService, serviceDto);
    }
}
