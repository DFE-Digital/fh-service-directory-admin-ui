using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Extensions;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using static FamilyHubs.ServiceDirectoryAdminUi.Ui.Infrastructure.Configuration.TempStorageConfiguration;


namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;

public class SessionService : ISessionService
{
    public OrganisationViewModel? RetrieveOrganisationWithService(HttpContext httpContext)
    {
        return httpContext.Session.Get<OrganisationViewModel>(KeyOrgWithService);
    }
    public void StoreOrganisationWithService(HttpContext httpContext, OrganisationViewModel? vm)
    {
        if (vm != null)
            httpContext.Session.Set(KeyOrgWithService, vm);
    }

    public void ResetOrganisationWithService(HttpContext httpContext)
    {
        httpContext.Session.ReSet<OrganisationViewModel>(KeyOrgWithService);
    }

    public string RetrieveLastPageName(HttpContext httpContext)
    {
        return httpContext.Session.Get<string>(KeyCurrentPage) ?? string.Empty;
    }

    public void StoreCurrentPageName(HttpContext httpContext, string? currPage)
    {
        httpContext.Session.Set(KeyCurrentPage, currPage);
    }

    public ServiceDto? RetrieveService(HttpContext httpContext)
    {
        return httpContext.Session.Get<ServiceDto>(KeyService);
    }

    public void StoreService(HttpContext httpContext, ServiceDto serviceDto)
    {
        httpContext.Session.Set(KeyService, serviceDto);
    }

    //user flow
    public string RetrieveUserFlow(HttpContext httpContext)
    {
        return httpContext.Session.Get<string>(KeyUserFlow) ?? string.Empty;
    }

    public void StoreUserFlow(HttpContext httpContext, string userFlow)
    {
        httpContext.Session.Set(KeyUserFlow, userFlow);
    }

    public void ResetLastPageName(HttpContext httpContext)
    {
        httpContext.Session.Set(KeyCurrentPage, String.Empty);
    }


}
