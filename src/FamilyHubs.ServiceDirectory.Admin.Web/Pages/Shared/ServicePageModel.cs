using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Errors;
using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.SharedKernel.Identity.Models;
using FamilyHubs.SharedKernel.Razor.ErrorNext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;

public class ServicePageModel : ServicePageModel<object>
{
    protected ServicePageModel(
        ServiceJourneyPage page,
        IRequestDistributedCache connectionRequestCache)
        : base(page, connectionRequestCache)
    {
    }
}

[Authorize(Roles = RoleGroups.AdminRole)]
public class ServicePageModel<TInput> : HeaderPageModel where TInput : class?
{
    //todo: make non-nullable any that are guaranteed to be set in get/post?
    public long? ServiceId { get; set; }
    public JourneyFlow Flow { get; set; }
    public bool RedirectingToSelf { get; set; }
    public string? BackUrl { get; set; }
    // not set in ctor, but will always be there in Get/Post handlers
    public FamilyHubsUser FamilyHubsUser { get; private set; } = default!;
    public ServiceModel<TInput>? ServiceModel { get; set; }
    public IErrorState Errors { get; private set; }

    protected readonly ServiceJourneyPage CurrentPage;
    protected IRequestDistributedCache Cache { get; }

    protected ServicePageModel(
        ServiceJourneyPage page,
        IRequestDistributedCache cache)
    {
        Cache = cache;
        CurrentPage = page;
        Errors = ErrorState.Empty;
    }

    //todo: decompose
    public async Task<IActionResult> OnGetAsync(
        string? serviceId,
        string? flow,
        bool redirectingToSelf = false,
        CancellationToken cancellationToken = default)
    {
        Flow = JourneyFlowExtensions.FromUrlString(flow);

        if (long.TryParse(serviceId, out long serviceIdLong))
        {
            ServiceId = serviceIdLong;
        }

        if (ServiceId == null && Flow == JourneyFlow.Edit)
        {
            // someone's been monkeying with the query string and we don't have the service details we need
            // we can't send them back to the details page because we don't know what service they were looking at
            // so we'll just send them to the menu page
            //todo: error or redirect?

            return Redirect("/Welcome");
        }

        RedirectingToSelf = redirectingToSelf;

        // default, but can be overridden
        BackUrl = GenerateBackUrl();

        //todo: could do with a version that just gets the email address
        FamilyHubsUser = HttpContext.GetFamilyHubsUser();

        if (Flow == JourneyFlow.Edit && !RedirectingToSelf)
        {
            //todo: when in Edit mode, it's only the errorstate that we actually need in the cache
            ServiceModel = await Cache.SetAsync(FamilyHubsUser.Email, new ServiceModel<TInput>());
        }
        else
        {
            ServiceModel = await Cache.GetAsync<ServiceModel<TInput>>(FamilyHubsUser.Email);
            if (ServiceModel == null)
            {
                // the journey cache entry has expired and we don't have a model to work with
                // likely the user has come back to this page after a long time
                return Redirect(GetServicePageUrl(ServiceJourneyPage.Initiator, ServiceId, Flow));
            }

            //todo: tie in with redirecting to self
            //todo: what if redirecting to self is set in url, and user uses browser back button?

            // handle this scenario:
            // we redirect to self with user input, then the browser shuts down before the get, then later another page is fetched.
            // without this check, we get an instance of TInput with all the properties set to default values
            // (unless the actual TInput in the cache happens to share property names/types with the TInput we're expecting, in which case we'll get some duff data)
            // we could store the wip input in the model's usual properties, but how would we handle error => redirect get => back => next. at this state would want a default page, not an errored page
            if (ServiceModel.UserInputType != null
                && ServiceModel.UserInputType != typeof(TInput).FullName)
            {
                ServiceModel.UserInput = default;
            }
        }

        if (ServiceModel.ErrorState?.Page == CurrentPage)
        {
            Errors = ErrorState.Create(PossibleErrors.All, ServiceModel.ErrorState.Errors);

            // some pages (like service name) don't need to keep user input, so we don't want to throw if we don't have any
            //todo: add method that consumer can call to check userinput is present? (when it needs it)?
            //if (Errors.HasErrors && typeof(TInput).Name != "object" && ServiceModel.UserInput == null)
            //{
            //    throw new InvalidOperationException("ServiceModel has errors and expecting user input but no user input");
            //}
        }
        else
        {
            // we don't save the model on Get, but we don't want the page to pick up the error state when the user has gone back
            // (we'll clear the error state in the model on a non-redirect to self post
            ServiceModel.ErrorState = null;
            Errors = ErrorState.Empty;
        }

        await OnGetWithModelAsync(cancellationToken);

        return Page();
    }

    //todo: decompose
    public async Task<IActionResult> OnPostAsync(
        string serviceId,
        string? flow = null,
        CancellationToken cancellationToken = default)
    {
        //todo: move to method?
        if (long.TryParse(serviceId, out long serviceIdLong))
        {
            ServiceId = serviceIdLong;
        }

        if (ServiceId == null && Flow == JourneyFlow.Edit)
        {
            // someone's been monkeying with the query string and we don't have the service details we need
            // we can't send them back to the details page because we don't know what service they were looking at
            // so we'll just send them to the menu page
            //todo: error or redirect?

            return Redirect("/Welcome");
        }

        Flow = JourneyFlowExtensions.FromUrlString(flow);

        // only required if we don't use PRG
        //BackUrl = GenerateBackUrl();

        FamilyHubsUser = HttpContext.GetFamilyHubsUser();

        // we don't need to retrieve UserInput on a post. this effectively clears it
        ServiceModel = await Cache.GetAsync<ServiceModel<TInput>>(FamilyHubsUser.Email);
        if (ServiceModel == null)
        {
            // the journey cache entry has expired and we don't have a model to work with
            // likely the user has come back to this page after a long time
            return Redirect(GetServicePageUrl(ServiceJourneyPage.Initiator, ServiceId, Flow));
        }

        var result = await OnPostWithModelAsync(cancellationToken);

        // if we're not redirecting to self, clear the error state and user input
        //todo: look for redirectingToSelf=True also?
        if (!(result is RedirectResult redirect && redirect.Url.StartsWith(CurrentPage.GetPagePath(Flow))))
        {
            ServiceModel.ErrorState = null;
            ServiceModel.UserInput = null;
        }

        await Cache.SetAsync(FamilyHubsUser.Email, ServiceModel);

        return result;
    }

    protected string GetServicePageUrl(
        ServiceJourneyPage page,
        long? serviceId,
        JourneyFlow flow,
        bool redirectingToSelf = false)
    {
        //todo: flow.ToUrlString needed?
        return $"{page.GetPagePath(flow)}?serviceId={serviceId}&flow={flow.ToUrlString()}&redirectingToSelf={redirectingToSelf}";
    }

    protected IActionResult RedirectToServicePage(
        ServiceJourneyPage page,
        //todo: does it need to be passed? take from class?
        JourneyFlow flow,
        bool redirectingToSelf = false)
    {
        return Redirect(GetServicePageUrl(page, ServiceId, flow, redirectingToSelf));
    }

    protected IActionResult NextPage()
    {
        var nextPage = Flow == JourneyFlow.Add ? CurrentPage + 1 : ServiceJourneyPage.Service_Detail;

        return RedirectToServicePage(nextPage, Flow);
    }

    protected string GenerateBackUrl()
    {
        var backUrlPage = Flow is JourneyFlow.Add
            ? CurrentPage - 1 : ServiceJourneyPage.Service_Detail;

        //todo: check ServiceId for null
        //todo: need flow too (unless default to Add)
        return GetServicePageUrl(backUrlPage, ServiceId, Flow);
    }

    //todo: naming?
    protected virtual void OnGetWithModel(CancellationToken cancellationToken)
    {
    }

    protected virtual Task OnGetWithModelAsync(CancellationToken cancellationToken)
    {
        OnGetWithModel(cancellationToken);

        return Task.CompletedTask;
    }

    protected virtual IActionResult OnPostWithModel(CancellationToken cancellationToken)
    {
        return Page();
    }

    protected virtual Task<IActionResult> OnPostWithModelAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(OnPostWithModel(cancellationToken));
    }

    protected IActionResult RedirectToSelf(TInput userInput, params ErrorId[] errors)
    {
        ServiceModel!.UserInputType = typeof(TInput).FullName;
        ServiceModel.UserInput = userInput;

        return RedirectToSelfInternal(errors);
    }

    protected IActionResult RedirectToSelf(params ErrorId[] errors)
    {
        ServiceModel!.UserInputType = null;
        ServiceModel.UserInput = null;

        return RedirectToSelfInternal(errors);
    }

    private IActionResult RedirectToSelfInternal(params ErrorId[] errors)
    {
        //todo: have this as a helper method
        //// truncate at some large value, to stop a denial of service attack
        //var safeInvalidUserInput = invalidUserInput != null
        //    ? new[] { invalidUserInput[..Math.Min(invalidUserInput.Length, 4500)] }
        //    : null;

        //todo: throw if model null?
        ServiceModel!.ErrorState = new ServiceErrorState(CurrentPage, errors);

        return RedirectToServicePage(CurrentPage, Flow, true);
    }
}