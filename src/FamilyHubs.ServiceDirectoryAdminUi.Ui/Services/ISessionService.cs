using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;

public interface ISessionService
{
    public OrganisationViewModel RetrieveService(HttpContext httpContext);
    public void StoreService(HttpContext httpContext, OrganisationViewModel vm);

    public string RetrieveLastPageName(HttpContext httpContext);
    public void StoreCurrentPageName(HttpContext httpContext, string currPage);
}
