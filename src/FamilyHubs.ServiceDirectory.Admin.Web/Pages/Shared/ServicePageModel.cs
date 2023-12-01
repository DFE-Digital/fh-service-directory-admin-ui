//using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
//using FamilyHubs.ServiceDirectory.Admin.Core.Models;
//using FamilyHubs.SharedKernel.Identity;
//using FamilyHubs.SharedKernel.Identity.Models;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;

//namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;

//[Authorize(Roles = RoleGroups.AdminRole)]
//public class ServicePageModel : HeaderPageModel
//{
//    protected readonly ServiceJourneyPage CurrentPage;
//    protected IRequestDistributedCache Cache { get; }
//    public long? ServiceId { get; set; }
//    public JourneyFlow Flow { get; set; }
//    public bool RedirectingToSelf { get; set; }
//    public string? BackUrl { get; set; }
//    // not set in ctor, but will always be there in Get/Post handlers
//    public FamilyHubsUser FamilyHubsUser { get; set; } = default!;

//    public ServicePageModel(ServiceJourneyPage page, IRequestDistributedCache cache)
//    {
//        Cache = cache;
//        CurrentPage = page;
//    }

//    protected virtual Task<IActionResult> OnSafeGetAsync(CancellationToken cancellationToken)
//    {
//        return Task.FromResult((IActionResult)Page());
//    }

//    protected virtual Task<IActionResult> OnSafePostAsync(CancellationToken cancellationToken)
//    {
//        return Task.FromResult((IActionResult)Page());
//    }

//    public async Task<IActionResult> OnGetAsync(
//        string? serviceId,
//        string? flow,
//        bool redirectingToSelf = false,
//        CancellationToken cancellationToken = default)
//    {
//        Flow = JourneyFlowExtensions.FromUrlString(flow);

//        if (long.TryParse(serviceId, out long serviceIdLong))
//        {
//            ServiceId = serviceIdLong;
//        }

//        if (ServiceId == null && Flow == JourneyFlow.Edit)
//        {
//            // someone's been monkeying with the query string and we don't have the service details we need
//            // we can't send them back to the details page because we don't know what service they were looking at
//            // so we'll just send them to the menu page
//            //todo: error or redirect?

//            return Redirect("/Welcome");
//        }

//        RedirectingToSelf = redirectingToSelf;

//        // default, but can be overridden
//        BackUrl = GenerateBackUrl();

//        //todo: could do with a version that just gets the email address
//        FamilyHubsUser = HttpContext.GetFamilyHubsUser();

//        return await OnSafeGetAsync(cancellationToken);
//    }

//    public async Task<IActionResult> OnPostAsync(
//        string serviceId,
//        string? flow = null,
//        CancellationToken cancellationToken = default)
//    {
//        //todo: move to method?
//        if (long.TryParse(serviceId, out long serviceIdLong))
//        {
//            ServiceId = serviceIdLong;
//        }

//        if (ServiceId == null && Flow == JourneyFlow.Edit)
//        {
//            // someone's been monkeying with the query string and we don't have the service details we need
//            // we can't send them back to the details page because we don't know what service they were looking at
//            // so we'll just send them to the menu page
//            //todo: error or redirect?

//            return Redirect("/Welcome");
//        }

//        Flow = JourneyFlowExtensions.FromUrlString(flow);

//        // only required if we don't use PRG
//        //BackUrl = GenerateBackUrl();

//        FamilyHubsUser = HttpContext.GetFamilyHubsUser();

//        return await OnSafePostAsync(cancellationToken);
//    }

//    protected string GetServicePageUrl(
//        ServiceJourneyPage page,
//        long? serviceId,
//        JourneyFlow flow,
//        bool redirectingToSelf = false)
//    {
//        //todo: flow.ToUrlString needed?
//        return $"{page.GetPagePath(flow)}?serviceId={serviceId}&flow={flow.ToUrlString()}&redirectingToSelf={redirectingToSelf}";
//    }

//    protected IActionResult RedirectToServicePage(
//        ServiceJourneyPage page,
//        //todo: does it need to be passed? take from class?
//        JourneyFlow flow,
//        bool redirectingToSelf = false)
//    {
//        return Redirect(GetServicePageUrl(page, ServiceId, flow, redirectingToSelf));
//    }

//    protected IActionResult NextPage()
//    {
//        var nextPage = Flow == JourneyFlow.Add ? CurrentPage + 1 : ServiceJourneyPage.Service_Detail;

//        return RedirectToServicePage(nextPage, Flow);
//    }

//    protected string GenerateBackUrl()
//    {
//        var backUrlPage = Flow is JourneyFlow.Add
//            ? CurrentPage - 1 : ServiceJourneyPage.Service_Detail;

//        //todo: check ServiceId for null
//        //todo: need flow too (unless default to Add)
//        return GetServicePageUrl(backUrlPage, ServiceId, Flow);
//    }
//}