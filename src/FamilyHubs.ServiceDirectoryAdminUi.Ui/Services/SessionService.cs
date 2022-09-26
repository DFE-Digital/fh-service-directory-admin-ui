using FamilyHubs.ServiceDirectoryAdminUi.Ui.Extensions;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using static FamilyHubs.ServiceDirectoryAdminUi.Ui.Infrastructure.Configuration.SessionConfiguration;


namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;

public class SessionService : ISessionService
{
    public OrganisationViewModel RetrieveService(HttpContext httpContext)
    {
        return httpContext.Session.Get<OrganisationViewModel>(SessionKeyService);
    }
    public void StoreService(HttpContext httpContext, OrganisationViewModel vm)
    {
        httpContext.Session.Set<OrganisationViewModel>(SessionKeyService, vm);
    }

    public string RetrieveLastPageName(HttpContext httpContext)
    {
        return httpContext.Session.Get<string>(SessionKeyCurrentPage);
    }

    public void StoreCurrentPageName(HttpContext httpContext, string currPage)
    {
        httpContext.Session.Set<string>(SessionKeyCurrentPage, currPage);
    }
}
