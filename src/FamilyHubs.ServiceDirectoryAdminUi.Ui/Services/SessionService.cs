using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServices;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Extensions;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using static FamilyHubs.ServiceDirectoryAdminUi.Ui.Infrastructure.Configuration.TempStorageConfiguration;


namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;

public class SessionService : ISessionService
{
    public SessionService()
    {

    }
    public OrganisationViewModel? RetrieveOrganisationWithService(HttpContext httpContext)
    {
        return httpContext.Session.Get<OrganisationViewModel>(KeyOrgWithService);
    }
    public void StoreOrganisationWithService(HttpContext httpContext, OrganisationViewModel? vm)
    {
        if (vm != null)
            httpContext.Session.Set<OrganisationViewModel>(KeyOrgWithService, vm);
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
        httpContext.Session.Set<string?>(KeyCurrentPage, currPage);
    }

    public OpenReferralServiceDto? RetrieveService(HttpContext httpContext)
    {
        return httpContext.Session.Get<OpenReferralServiceDto>(KeyService);
    }

    public void StoreService(HttpContext httpContext, OpenReferralServiceDto serviceDto)
    {
        httpContext.Session.Set<OpenReferralServiceDto>(KeyService, serviceDto);
    }

    //user flow
    public string RetrieveUserFlow(HttpContext httpContext)
    {
        return httpContext.Session.Get<string>(KeyUserFlow) ?? string.Empty;
    }

    public void StoreUserFlow(HttpContext httpContext, string userFlow)
    {
        httpContext.Session.Set<string>(KeyUserFlow, userFlow);
    }

    public void ResetLastPageName(HttpContext httpContext)
    {
        httpContext.Session.Set<string>(KeyCurrentPage, String.Empty);
    }


}
