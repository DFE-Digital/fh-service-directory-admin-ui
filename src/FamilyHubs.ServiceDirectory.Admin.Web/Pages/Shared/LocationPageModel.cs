using System.Runtime.CompilerServices;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Models.LocationJourney;
using FamilyHubs.ServiceDirectory.Admin.Core.Models.ServiceJourney;
using FamilyHubs.ServiceDirectory.Admin.Web.Errors;
using FamilyHubs.ServiceDirectory.Admin.Web.Journeys;
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
public class LocationPageModel<TInput> : HeaderPageModel
    where TInput : class?
{
    //todo: make non-nullable any that are guaranteed to be set in get/post?
    public Journey Journey { get; set; }
    public JourneyFlow Flow { get; set; }
    public JourneyFlow? ParentJourneyFlow { get; set; }
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
        string? journey,
        string? parentJourneyFlow,
        bool redirectingToSelf = false,
        CancellationToken cancellationToken = default)
    {
        Journey = journey != null ? Enum.Parse<Journey>(journey) : Journey.Location;
        Flow = JourneyFlowExtensions.FromUrlString(flow);
        ParentJourneyFlow = JourneyFlowExtensions.FromOptionalUrlString(parentJourneyFlow);

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
            return RedirectOnLocationModelExpiry();
        }

        LocationModel.PopulateUserInput();

        if (LocationModel.ErrorState?.Page == CurrentPage)
        {
            Errors = ErrorState.Create(PossibleErrors.All, LocationModel.ErrorState.Errors);

            await OnGetWithErrorAsync(cancellationToken);
        }
        else
        {
            // we don't save the model on Get, but we don't want the page to pick up the error state when the user has gone back
            // (we'll clear the error state in the model on a non-redirect to self post)
            //todo: call ClearErrors() instead?
            LocationModel.ErrorState = null;
            Errors = ErrorState.Empty;

            await OnGetWithModelAsync(cancellationToken);
        }

        return Page();
    }

    private IActionResult RedirectOnLocationModelExpiry()
    {
        return Journey switch
        {
            Journey.Location => Redirect(GetLocationPageUrl(LocationJourneyPage.Initiator, Journey, Flow)),
            //todo: should do this really. static method on ServicePageModel
            //Journey.Service => Redirect(GetServicePageUrl(ServiceJourneyPage.Initiator, Flow)),
            Journey.Service => Redirect("/Welcome"),
            _ => throw new SwitchExpressionException(Journey)
        };
    }

    //todo: flow = null or not (see get)
    public async Task<IActionResult> OnPostAsync(
        string? journey,
        string? flow = null,
        string? parentJourneyFlow = null,
        CancellationToken cancellationToken = default)
    {
        Journey = journey != null ? Enum.Parse<Journey>(journey) : Journey.Location;
        Flow = JourneyFlowExtensions.FromUrlString(flow);
        ParentJourneyFlow = JourneyFlowExtensions.FromOptionalUrlString(parentJourneyFlow);

        // only required if we don't use PRG
        //BackUrl = GenerateBackUrl();

        FamilyHubsUser = HttpContext.GetFamilyHubsUser();

        // we don't need to retrieve UserInput on a post. this effectively clears it
        LocationModel = await Cache.GetAsync<LocationModel<TInput>>(FamilyHubsUser.Email);
        if (LocationModel == null)
        {
            // the journey cache entry has expired and we don't have a model to work with
            // likely the user has come back to this page after a long time
            return RedirectOnLocationModelExpiry();
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
        Journey journey,
        JourneyFlow? flow = null,
        JourneyFlow? parentJourneyFlow = null,
        bool redirectingToSelf = false)
    {
        flow ??= Flow;

        string redirectingToSelfParam = redirectingToSelf ? "&redirectingToSelf=true" : "";
        string parentJourneyFlowParam = parentJourneyFlow == null ? "" : $"&parentJourneyFlow={parentJourneyFlow}";
        return $"{page.GetPagePath(flow.Value)}?journey={journey}&flow={flow.Value.ToUrlString()}{redirectingToSelfParam}{parentJourneyFlowParam}";
    }

    protected IActionResult RedirectToLocationPage(
        LocationJourneyPage page,
        Journey journey,
        JourneyFlow flow,
        JourneyFlow? parentJourneyFlow = null,
        bool redirectingToSelf = false)
    {
        return Redirect(GetLocationPageUrl(page, journey, flow, parentJourneyFlow, redirectingToSelf));
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

        return RedirectToLocationPage(
            nextPage,
            Journey,
            Flow == JourneyFlow.AddRedo ? JourneyFlow.Add : Flow,
            ParentJourneyFlow);
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
        else if (CurrentPage == LocationJourneyPage.Location_Details && Flow is JourneyFlow.Edit)
        {
            return "/manage-locations";
        }
        else
        {
            backUrlPage = LocationJourneyPage.Location_Details;
        }

        if (Journey == Journey.Service && backUrlPage == LocationJourneyPage.Initiator)
        {
            //todo: check for null?
            //todo: there should be a method that adds the flow param. perhaps GetPagePath itself, as it looks like all callers do it
            return $"{ServiceJourneyPage.Select_Location.GetPagePath(ParentJourneyFlow!.Value)}?flow={ParentJourneyFlow.Value}";
        }

        return GetLocationPageUrl(backUrlPage, Journey, Flow is JourneyFlow.AddRedo ? JourneyFlow.Add : Flow, ParentJourneyFlow);
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

    protected virtual void OnGetWithError()
    {
    }

    protected virtual Task OnGetWithErrorAsync(CancellationToken cancellationToken)
    {
        OnGetWithError();

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
        LocationModel!.SetUserInput(userInput);

        return RedirectToSelfInternal(errors);
    }

    protected IActionResult RedirectToSelf(params ErrorId[] errors)
    {
        LocationModel!.SetUserInput(null!);

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

        return RedirectToLocationPage(CurrentPage, Journey, Flow, ParentJourneyFlow, true);
    }
}