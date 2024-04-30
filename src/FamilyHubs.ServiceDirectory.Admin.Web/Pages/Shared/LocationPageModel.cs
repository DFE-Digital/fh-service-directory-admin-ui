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
using System.Diagnostics;

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
    //todo: don't really need this, can just use the presence of ParentJourneyFlow instead
    public Journey Journey { get; set; }
    public JourneyFlow Flow { get; set; }
    public LocationJourneyChangeFlow? ChangeFlow { get; set; }
    public string? ParentJourneyContext { get; set; }
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
        string? change,
        string? parentJourneyContext,
        bool redirectingToSelf = false,
        CancellationToken cancellationToken = default)
    {
        Journey = journey != null ? Enum.Parse<Journey>(journey) : Journey.Location;
        Flow = flow.ToEnum<JourneyFlow>();
        ParentJourneyContext = parentJourneyContext;
        ChangeFlow = change.ToOptionalEnum<LocationJourneyChangeFlow>();

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
            return Redirect(GenerateBackUrlToJourneyInitiatorPage());
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

    protected string GenerateBackUrlToJourneyInitiatorPage()
    {
        if (FamilyHubsUser.Role == RoleTypes.DfeAdmin)
        {
            return "/Welcome";
        }
        return Journey == Journey.Service ? "/manage-services" : "/manage-locations";
    }

    //todo: flow = null or not (see get)
    public async Task<IActionResult> OnPostAsync(
        string? journey,
        string? flow = null,
        string? change = null,
        string? parentJourneyContext = null,
        CancellationToken cancellationToken = default)
    {
        Journey = journey != null ? Enum.Parse<Journey>(journey) : Journey.Location;
        Flow = flow.ToEnum<JourneyFlow>();
        ParentJourneyContext = parentJourneyContext;
        ChangeFlow = change.ToOptionalEnum<LocationJourneyChangeFlow>();

        // only required if we don't use PRG
        //BackUrl = GenerateBackUrl();

        FamilyHubsUser = HttpContext.GetFamilyHubsUser();

        // we don't need to retrieve UserInput on a post. this effectively clears it
        LocationModel = await Cache.GetAsync<LocationModel<TInput>>(FamilyHubsUser.Email);
        if (LocationModel == null)
        {
            // the journey cache entry has expired and we don't have a model to work with
            // likely the user has come back to this page after a long time
            return Redirect(GenerateBackUrlToJourneyInitiatorPage());
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

    //todo: rename ro remove Location
    public string GetLocationPageUrl(
        LocationJourneyPage page,
        LocationJourneyChangeFlow? changeFlow = null,
        LocationJourneyPage? backPage = null)
    {
        changeFlow ??= ChangeFlow;

        string changeFlowParam = changeFlow != null ? $"&change={changeFlow.Value.ToUrlString()}" : "";

        if (backPage == null && page == LocationJourneyPage.Location_Details)
        {
            backPage = CurrentPage;
        }

        string backPageParam = backPage != null ? $"&back={backPage.Value.GetSlug()}" : "";

        //todo: do we need journey? can we just check for the presence of parentJourneyContext instead - should suffice if the only journey to call create location is the add/edit service. might want to leave the journey, in case we call create location from anywhere else
        string parentJourneyFlowContext = ParentJourneyContext == null ? "" : $"&parentJourneyContext={ParentJourneyContext}";
        return $"{page.GetPagePath(Flow)}&journey={Journey}{changeFlowParam}{parentJourneyFlowContext}{backPageParam}";
    }

    protected IActionResult NextPage()
    {
        LocationJourneyPage nextPage;

        //todo: have edit as a change flow?
        // think we can just remove Flow == JourneyFlow.Edit, as when editing ChangeFlow will be set
        if (ChangeFlow != null || Flow == JourneyFlow.Edit)
        {
            nextPage = LocationJourneyPage.Location_Details;
        }
        else
        {
            nextPage = CurrentPage + 1;

            // VCS Managers and Dual Role users skip the Family Hub page
            if (nextPage == LocationJourneyPage.Family_Hub
                && FamilyHubsUser.Role is RoleTypes.VcsManager or RoleTypes.VcsDualRole)
            {
                ++nextPage;
            }
        }

        return Redirect(GetLocationPageUrl(nextPage));
    }

    //todo: naming & make service version match too
    private LocationJourneyPage PreviousPage()
    {
        LocationJourneyPage backUrlPage = CurrentPage - 1;

        // VCS Managers and Dual Role users skip the Family Hub page
        if (backUrlPage == LocationJourneyPage.Family_Hub
            && FamilyHubsUser.Role is RoleTypes.VcsManager or RoleTypes.VcsDualRole)
        {
            --backUrlPage;
        }

        return backUrlPage;
    }

    //todo: have separate class for packing and unpacking parent context
    protected JourneyFlow ParentJourneyFlow
    {
        get
        {
            if (ParentJourneyContext == null)
            {
                throw new InvalidOperationException("ParentJourneyContext not set");
            }

            return ParentJourneyContext.Split('-')[0].ToEnum<JourneyFlow>();
        }
    }

    protected ServiceJourneyChangeFlow? ParentServiceJourneyChangeFlow
    {
        get
        {
            if (ParentJourneyContext == null)
            {
                throw new InvalidOperationException("ParentJourneyContext not set");
            }

            var contextComponents = ParentJourneyContext.Split('-');
            if (contextComponents.Length > 1 && contextComponents[1] != "")
                return contextComponents[1].ToEnum<ServiceJourneyChangeFlow>();
            return null;
        }
    }

    protected virtual string GenerateBackUrl()
    {
        LocationJourneyPage? backUrlPage = null;

        //todo: back when come back to the details page after changing a single page

        if (ChangeFlow != null)
        {
            backUrlPage = LocationJourneyPage.Location_Details;
        }
        else if (Flow is JourneyFlow.Add)
        {
            backUrlPage = PreviousPage();

            if (backUrlPage == LocationJourneyPage.Initiator)
            {
                if (Journey == Journey.Location)
                {
                    return GenerateBackUrlToJourneyInitiatorPage();
                }

                return $"{ServiceJourneyPage.Select_Location.GetPagePath(ParentJourneyFlow)}&changeFlow={ParentServiceJourneyChangeFlow}";
            }
        }
        //else if (Flow is JourneyFlow.Edit)
        //{
        //    // the only time when we're in the Edit flow with no change flow, is when we first hit the details page
        //    Debug.Assert(CurrentPage == LocationJourneyPage.Location_Details);
        //    return GenerateBackUrlToJourneyInitiatorPage();
        //}
        ////todo: need this for servicepagemodel too? do in override?
        //else if (CurrentPage == LocationJourneyPage.Location_Details && Flow is JourneyFlow.Edit)
        //{
        //    return "/manage-locations";
        //}
        //else
        //{
        //    backUrlPage = LocationJourneyPage.Location_Details;
        //}

        //todo: alternative, is to always pass it but for details page to ignore it
        //var changeFlow = backUrlPage == LocationJourneyPage.Location_Details ? null : ChangeFlow;

        if (backUrlPage == null)
        {
            throw new InvalidOperationException("Back page not set");
        }

        //return GetLocationPageUrl(backUrlPage, changeFlow);
        return GetLocationPageUrl(backUrlPage.Value);
    }

    //protected string GenerateBackUrl()
    //{
    //    LocationJourneyPage backUrlPage;

    //    if (Flow is JourneyFlow.Add)
    //    {
    //        backUrlPage = CurrentPage - 1;

    //        // VCS Managers and Dual Role users skip the Family Hub page
    //        if (backUrlPage == LocationJourneyPage.Family_Hub
    //            && FamilyHubsUser.Role is RoleTypes.VcsManager or RoleTypes.VcsDualRole)
    //        {
    //            --backUrlPage;
    //        }
    //    }
    //    else if (CurrentPage == LocationJourneyPage.Location_Details && Flow is JourneyFlow.Edit)
    //    {
    //        return GenerateBackUrlToJourneyInitiatorPage();
    //    }
    //    else
    //    {
    //        backUrlPage = LocationJourneyPage.Location_Details;
    //    }

    //    if (Journey == Journey.Service && backUrlPage == LocationJourneyPage.Initiator)
    //    {
    //        //todo: check for null?
    //        //todo: there should be a method that adds the flow param. perhaps GetPagePath itself, as it looks like all callers do it
    //        return $"{ServiceJourneyPage.Select_Location.GetPagePath(ParentJourneyFlow!.Value)}";
    //    }

    //    //todo: alternative, is to always pass it but for details page to ignore it
    //    //var changeFlow = backUrlPage == LocationJourneyPage.Location_Details ? null : ChangeFlow;

    //    //return GetLocationPageUrl(backUrlPage, changeFlow);
    //    return GetLocationPageUrl(backUrlPage);
    //}

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

        return Redirect($"{GetLocationPageUrl(CurrentPage)}&redirectingToSelf=true");
    }

    protected LocationJourneyPage? BackParam
    {
        get
        {
            var backValues = Request.Query["back"];
            if (backValues.Count == 1)
            {
                return LocationJourneyPageExtensions.FromSlug(backValues[0]!);
            }

            return null;
        }
    }
}