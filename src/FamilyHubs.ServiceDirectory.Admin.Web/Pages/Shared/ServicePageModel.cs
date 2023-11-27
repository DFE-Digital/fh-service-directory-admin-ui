using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.SharedKernel.Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;

[Authorize(Roles = RoleGroups.AdminRole)]
public class ServicePageModel : HeaderPageModel
{
    protected readonly ServiceJourneyPage CurrentPage;
    protected IRequestDistributedCache Cache { get; }
    public long? ServiceId { get; set; }
    public JourneyFlow Flow { get; set; }
    public bool RedirectingToSelf { get; set; }
    public string? BackUrl { get; set; }
    // not set in ctor, but will always be there in Get/Post handlers
    public FamilyHubsUser FamilyHubsUser { get; set; } = default!;

    public ServicePageModel(ServiceJourneyPage page, IRequestDistributedCache cache)
    {
        Cache = cache;
        CurrentPage = page;
    }

    protected virtual Task<IActionResult> OnSafeGetAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult((IActionResult)Page());
    }

    protected virtual Task<IActionResult> OnSafePostAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult((IActionResult)Page());
    }

    public async Task<IActionResult> OnGetAsync(
        string? serviceId,
        string? flow,
        bool redirectingToSelf = false,
        CancellationToken cancellationToken = default)
    {
        if (long.TryParse(serviceId, out long serviceIdLong))
        {
            ServiceId = serviceIdLong;
        }

        Flow = JourneyFlowExtensions.FromUrlString(flow);

        if (ServiceId == null && Flow == JourneyFlow.Edit)
        {
            // someone's been monkeying with the query string and we don't have the service details we need
            // we can't send them back to the start of the journey because we don't know what service they were looking at
            // so we'll just send them to the menu page
            //todo: error or redirect?

            return Redirect(ServiceJourneyPageExtensions.GetInitiatorPagePath(Flow));
        }

        RedirectingToSelf = redirectingToSelf;

        // default, but can be overridden
        BackUrl = GenerateBackUrl();

        //todo: could do with a version that just gets the email address
        FamilyHubsUser = HttpContext.GetFamilyHubsUser();

        return await OnSafeGetAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostAsync(
        string serviceId,
        string? flow = null,
        CancellationToken cancellationToken = default)
    {
        //todo: try parse (in method?)
        ServiceId = long.Parse(serviceId);

        Flow = JourneyFlowExtensions.FromUrlString(flow);

        // default, but can be overridden
        BackUrl = GenerateBackUrl();

        FamilyHubsUser = HttpContext.GetFamilyHubsUser();

        return await OnSafePostAsync(cancellationToken);
    }

    class RouteValues
    {
        public long ServiceId { get; set; }
        public string? Flow { get; set; }
        public bool RedirectingToSelf { get; set; }
    }

    protected IActionResult RedirectToServicePage(
        ServiceJourneyPage page,
        //todo: does it need to be passed?
        JourneyFlow? flow = null,
        bool redirectingToSelf = false)
    {
        flow ??= JourneyFlow.Add;

        //todo: mismatch between page url and page name
        return RedirectToPage($"/{page.GetPageUrl()}", new RouteValues
        {
            ServiceId = ServiceId!.Value,
            //todo: do we need to generate the string ourselves?
            Flow = flow.Value.ToUrlString(),
            RedirectingToSelf = redirectingToSelf
        });
    }

    protected IActionResult NextPage()
    {
        var nextPage = Flow == JourneyFlow.Add ? CurrentPage + 1 : ServiceJourneyPage.Details;

        return RedirectToServicePage(nextPage);
    }

    protected string GenerateBackUrl()
    {
        ServiceJourneyPage? backUrlPage;
        if (Flow is JourneyFlow.Add)
        {
            backUrlPage = CurrentPage - 1;

            if (backUrlPage == ServiceJourneyPage.Initiator)
            {
                return ServiceJourneyPageExtensions.GetInitiatorPagePath(Flow);
            }
        }
        else
        {
            backUrlPage = ServiceJourneyPage.Details;
        }

        //todo: check ServiceId for null
        //todo: need flow too (unless default to Add)
        return $"/{backUrlPage.Value.GetPageUrl()}?serviceId={ServiceId}";
    }
}