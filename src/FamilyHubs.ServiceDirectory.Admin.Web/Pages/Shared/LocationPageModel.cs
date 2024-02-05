using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Errors;
using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.SharedKernel.Identity.Models;
using FamilyHubs.SharedKernel.Razor.ErrorNext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;

//todo: have generic version of these classes (for service and location)?
//todo: if don't have generic version, use common composable code

public class LocationPageModel : LocationPageModel<object>
{
    protected LocationPageModel(
        LocationJourneyPage page,
        IRequestDistributedCache connectionRequestCache)
        : base(page, connectionRequestCache)
    {
    }
}

[Authorize(Roles = RoleGroups.AdminRole)]
public class LocationPageModel<TInput> : HeaderPageModel where TInput : class?
{
    //todo: make non-nullable any that are guaranteed to be set in get/post?
    public JourneyFlow Flow { get; set; }
    public bool RedirectingToSelf { get; set; }
    public string? BackUrl { get; set; }
    // not set in ctor, but will always be there in Get/Post handlers
    public FamilyHubsUser FamilyHubsUser { get; private set; } = default!;

    public LocationModel<TInput>? LocationModel { get; set; }

    public IErrorState Errors { get; private set; }

    protected readonly LocationJourneyPage CurrentPage;
    protected IRequestDistributedCache Cache { get; }

    protected LocationPageModel(
        LocationJourneyPage page,
        IRequestDistributedCache cache)
    {
        Cache = cache;
        CurrentPage = page;
        Errors = ErrorState.Empty;
    }

    //todo: decompose
    public async Task<IActionResult> OnGetAsync(
        string? flow,
        bool redirectingToSelf = false,
        CancellationToken cancellationToken = default)
    {
        Flow = JourneyFlowExtensions.FromUrlString(flow);

        RedirectingToSelf = redirectingToSelf;

        //todo: could do with a version that just gets the email address
        FamilyHubsUser = HttpContext.GetFamilyHubsUser();

        // default, but can be overridden
        BackUrl = GenerateBackUrl();

        LocationModel = await Cache.GetAsync<LocationModel<TInput>>(FamilyHubsUser.Email);
        if (LocationModel == null)
        {
            // the journey cache entry has expired and we don't have a model to work with
            // likely the user has come back to this page after a long time
            return Redirect(GetLocationPageUrl(LocationJourneyPage.Initiator, Flow));
        }

        //todo: tie in with redirecting to self
        //todo: what if redirecting to self is set in url, and user uses browser back button?

        // handle this scenario:
        // we redirect to self with user input, then the browser shuts down before the get, then later another page is fetched.
        // without this check, we get an instance of TInput with all the properties set to default values
        // (unless the actual TInput in the cache happens to share property names/types with the TInput we're expecting, in which case we'll get some duff data)
        // we could store the wip input in the model's usual properties, but how would we handle error => redirect get => back => next. at this state would want a default page, not an errored page
        if (LocationModel.UserInputType != null
            && LocationModel.UserInputType != typeof(TInput).FullName)
        {
            LocationModel.UserInput = default;
        }

        if (LocationModel.ErrorState?.Page == CurrentPage)
        {
            Errors = ErrorState.Create(PossibleErrors.All, LocationModel.ErrorState.Errors);
        }
        else
        {
            // we don't save the model on Get, but we don't want the page to pick up the error state when the user has gone back
            // (we'll clear the error state in the model on a non-redirect to self post
            LocationModel.ErrorState = null;
            Errors = ErrorState.Empty;
        }

        await OnGetWithModelAsync(cancellationToken);

        return Page();
    }

    //todo: decompose
    public async Task<IActionResult> OnPostAsync(
        string? flow = null,
        CancellationToken cancellationToken = default)
    {
        Flow = JourneyFlowExtensions.FromUrlString(flow);

        // only required if we don't use PRG
        //BackUrl = GenerateBackUrl();

        FamilyHubsUser = HttpContext.GetFamilyHubsUser();

        // we don't need to retrieve UserInput on a post. this effectively clears it
        LocationModel = await Cache.GetAsync<LocationModel<TInput>>(FamilyHubsUser.Email);
        if (LocationModel == null)
        {
            // the journey cache entry has expired and we don't have a model to work with
            // likely the user has come back to this page after a long time
            return Redirect(GetLocationPageUrl(LocationJourneyPage.Initiator, Flow));
        }

        var result = await OnPostWithModelAsync(cancellationToken);

        // if we're not redirecting to self
        //todo: look for redirectingToSelf=True also?
        if (!(result is RedirectResult redirect && redirect.Url.StartsWith(CurrentPage.GetPagePath(Flow))))
        {
            // clear the error state and user input
            LocationModel.ErrorState = null;
            LocationModel.UserInput = null;
        }

        await Cache.SetAsync(FamilyHubsUser.Email, LocationModel);

        return result;
    }

    public string GetLocationPageUrl(
        LocationJourneyPage page,
        JourneyFlow? flow = null,
        bool redirectingToSelf = false)
    {
        flow ??= Flow;

        string redirectingToSelfParam = redirectingToSelf ? "&redirectingToSelf=true" : "";
        return $"{page.GetPagePath(flow.Value)}?flow={flow.Value.ToUrlString()}{redirectingToSelfParam}";
    }

    protected IActionResult RedirectToLocationPage(
        LocationJourneyPage page,
        JourneyFlow flow,
        bool redirectingToSelf = false)
    {
        return Redirect(GetLocationPageUrl(page, flow, redirectingToSelf));
    }

    protected IActionResult NextPage()
    {
        LocationJourneyPage nextPage;
        if (Flow == JourneyFlow.Add)
        {
            nextPage = CurrentPage + 1;
            
            // VCS Managers and Dual Role users skip the Family Hub page
            if (nextPage == LocationJourneyPage.Family_Hub
                && FamilyHubsUser.Role is RoleTypes.VcsManager or RoleTypes.VcsDualRole)
            {
                ++nextPage;
            }
        }
        else
        {
            nextPage = LocationJourneyPage.Location_Details;
        }

        return RedirectToLocationPage(nextPage, Flow == JourneyFlow.AddRedo ? JourneyFlow.Add : Flow);
    }

    protected string GenerateBackUrl()
    {
        LocationJourneyPage backUrlPage;

        if (Flow is JourneyFlow.Add)
        {
            backUrlPage = CurrentPage - 1;
            
            // VCS Managers and Dual Role users skip the Family Hub page
            if (backUrlPage == LocationJourneyPage.Family_Hub
                && FamilyHubsUser.Role is RoleTypes.VcsManager or RoleTypes.VcsDualRole)
            {
                --backUrlPage;
            }
        }
        else
        {
            backUrlPage = LocationJourneyPage.Location_Details;
        }

        return GetLocationPageUrl(backUrlPage, Flow is JourneyFlow.AddRedo ? JourneyFlow.Add : Flow);
    }

    //todo: naming?
    protected virtual void OnGetWithModel()
    {
    }

    protected virtual Task OnGetWithModelAsync(CancellationToken cancellationToken)
    {
        OnGetWithModel();

        return Task.CompletedTask;
    }

    protected virtual IActionResult OnPostWithModel()
    {
        return Page();
    }

    protected virtual Task<IActionResult> OnPostWithModelAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(OnPostWithModel());
    }

    protected IActionResult RedirectToSelf(TInput userInput, params ErrorId[] errors)
    {
        LocationModel!.UserInputType = typeof(TInput).FullName;
        LocationModel.UserInput = userInput;

        return RedirectToSelfInternal(errors);
    }

    protected IActionResult RedirectToSelf(params ErrorId[] errors)
    {
        LocationModel!.UserInputType = null;
        LocationModel.UserInput = null;

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
        LocationModel!.AddErrorState(CurrentPage, errors);

        return RedirectToLocationPage(CurrentPage, Flow, true);
    }
}