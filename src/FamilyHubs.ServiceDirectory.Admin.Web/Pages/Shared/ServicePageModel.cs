﻿using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Models.ServiceJourney;
using FamilyHubs.ServiceDirectory.Admin.Web.Errors;
using FamilyHubs.ServiceDirectory.Admin.Web.Journeys;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.SharedKernel.Identity.Models;
using FamilyHubs.SharedKernel.Razor.ErrorNext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
// Use Array.Exists, rather than Any() : makes refactoring harder, and doesn't look as object-oriented
#pragma warning disable S6605

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
public class ServicePageModel<TInput> : HeaderPageModel
    where TInput : class?
{
    //todo: make non-nullable any that are guaranteed to be set in get/post?
    public JourneyFlow Flow { get; set; }
    public ServiceJourneyChangeFlow? ChangeFlow { get; set; }
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

    public async Task<IActionResult> OnGetAsync(
        string? flow,
        string? change,
        bool redirectingToSelf = false,
        CancellationToken cancellationToken = default)
    {
        Flow = flow.ToEnum<JourneyFlow>();
        ChangeFlow = change.ToOptionalEnum<ServiceJourneyChangeFlow>();

        RedirectingToSelf = redirectingToSelf;

        //todo: could do with a version that just gets the email address
        FamilyHubsUser = HttpContext.GetFamilyHubsUser();

        ServiceModel = await Cache.GetAsync<ServiceModel<TInput>>(FamilyHubsUser.Email);
        if (ServiceModel == null)
        {
            // the journey cache entry has expired and we don't have a model to work with
            // likely the user has come back to this page after a long time
            return Redirect(GenerateBackUrlToJourneyInitiatorPage(null));
        }

        ServiceModel.PopulateUserInput();

        // default, but can be overridden
        BackUrl = GenerateBackUrl();

        if (ServiceModel.ErrorState?.Page == CurrentPage)
        {
            Errors = ErrorState.Create(PossibleErrors.All, ServiceModel.ErrorState.Errors);

            await OnGetWithErrorAsync(cancellationToken);
        }
        else
        {
            // we don't save the model on Get, but we don't want the page to pick up the error state when the user has gone back
            // (we'll clear the error state in the model on a non-redirect to self post
            ServiceModel.ErrorState = null;
            Errors = ErrorState.Empty;

            await OnGetWithModelAsync(cancellationToken);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(
        string? flow = null,
        string? change = null,
        CancellationToken cancellationToken = default)
    {
        Flow = flow.ToEnum<JourneyFlow>();
        ChangeFlow = change.ToOptionalEnum<ServiceJourneyChangeFlow>();

        // only required if we don't use PRG
        //BackUrl = GenerateBackUrl();

        FamilyHubsUser = HttpContext.GetFamilyHubsUser();

        // we don't need to retrieve UserInput on a post. this effectively clears it
        ServiceModel = await Cache.GetAsync<ServiceModel<TInput>>(FamilyHubsUser.Email);
        if (ServiceModel == null)
        {
            // the journey cache entry has expired and we don't have a model to work with
            // likely the user has come back to this page after a long time
            return Redirect(GenerateBackUrlToJourneyInitiatorPage(null));
        }

        var result = await OnPostWithModelAsync(cancellationToken);

        // if we're not redirecting to self, clear the error state and user input
        //todo: look for redirectingToSelf=True also?
        if (!(result is RedirectResult redirect && redirect.Url.StartsWith(CurrentPage.GetPagePath(Flow))))
        {
            ServiceModel.ErrorState = null;
            ServiceModel.UserInput = null;
        }

        if (result is RedirectResult redirect2 && redirect2.Url.StartsWith(ServiceJourneyPage.Service_Detail.GetPagePath(Flow)))
        {
            ServiceModel.FinishingJourney = true;
        }

        await Cache.SetAsync(FamilyHubsUser.Email, ServiceModel);

        return result;
    }

    public string GetServicePageUrl(
        ServiceJourneyPage page,
        ServiceJourneyChangeFlow? changeFlow = null,
        ServiceJourneyPage? backPage = null)
    {
        if (backPage == null && page == ServiceJourneyPage.Service_Detail)
        {
            backPage = CurrentPage;
        }

        return page.GetPagePath(Flow, changeFlow ?? ChangeFlow, backPage);
    }

    private ServiceJourneyPage NextPageCore()
    {
        var nextPage = CurrentPage + 1;
        switch (nextPage)
        {
            case ServiceJourneyPage.Vcs_Organisation
                when ServiceModel!.ServiceType == ServiceTypeArg.La:
                ++nextPage;
                break;

            case ServiceJourneyPage.Add_Location
                when !ServiceModel!.HowUse.Contains(AttendingType.InPerson):
            case ServiceJourneyPage.Select_Location
                when ServiceModel!.AddingLocations == false:

                nextPage = ServiceJourneyPage.Times;
                break;

            case ServiceJourneyPage.Times
                when !ServiceModel!.HowUse.Any(hu => hu is AttendingType.Online or AttendingType.Telephone):

                nextPage = ServiceJourneyPage.Contact;
                break;
        }

        return nextPage;
    }

    /// <summary>
    /// Gets a redirect action to the next page in the journey.
    /// Not expected to be called for Service_Detail or Remove_Location.
    /// </summary>
    /// <param name="addBack">Whether to add a 'back' query parameter to the current page.</param>
    /// <returns>The redirect action to the next page in the journey.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the code doesn't come up with the next page.
    /// Either because the code has missed a case or if it's called by a page that isn't supported.
    /// </exception>
    protected IActionResult NextPage(bool addBack = false)
    {
        ServiceJourneyPage? nextPage = null;
        if (ChangeFlow != null)
        {
            if (ChangeFlow == ServiceJourneyChangeFlow.SinglePage)
            {
                nextPage = ServiceJourneyPage.Service_Detail;
            }
            else
            {
                nextPage = NextPageCore();

                if (ChangeFlow == ServiceJourneyChangeFlow.LocalAuthority &&
                    nextPage > ServiceJourneyPage.Vcs_Organisation)
                {
                    nextPage = ServiceJourneyPage.Service_Detail;
                }
                else
                {
                    // if we're about to ask the user to enter the service's schedule, but we don't need one
                    if (ChangeFlow == ServiceJourneyChangeFlow.HowUse && nextPage >= ServiceJourneyPage.Times
                                                                      && ServiceModel!.HowUse.Length == 1 &&
                                                                      ServiceModel.HowUse.Contains(AttendingType
                                                                          .InPerson)
                                                                      && ServiceModel.AllLocations.Any())
                    {
                        nextPage = ServiceJourneyPage.Service_Detail;
                    }

                    // if we're at the end of the location or 'how use' mini-journey
                    if ((ChangeFlow == ServiceJourneyChangeFlow.Location && nextPage >= ServiceJourneyPage.Times)
                        || (ChangeFlow == ServiceJourneyChangeFlow.HowUse && nextPage >= ServiceJourneyPage.Contact))
                    {
                        nextPage = ServiceJourneyPage.Service_Detail;
                    }
                }
            }
        }
        else if (Flow == JourneyFlow.Add)
        {
            nextPage = NextPageCore();
        }

        if (nextPage == null)
        {
            throw new InvalidOperationException("Next page not set");
        }

        string nextPageUrl = GetServicePageUrl(nextPage.Value, backPage: addBack ? CurrentPage : null);

        return Redirect(nextPageUrl);
    }

    private ServiceJourneyPage PreviousPageAddFlow()
    {
        var backUrlPage = CurrentPage - 1;
        switch (backUrlPage)
        {
            case ServiceJourneyPage.Vcs_Organisation:
                if (FamilyHubsUser.Role != RoleTypes.DfeAdmin)
                {
                    backUrlPage = ServiceJourneyPage.Initiator;
                }
                else if (ServiceModel!.ServiceType == ServiceTypeArg.La)
                {
                    --backUrlPage;
                }
                break;

            case ServiceJourneyPage.Time_Details_At_Location:
                backUrlPage = ServiceJourneyPage.Select_Location;
                break;

            case ServiceJourneyPage.Time_Details
                when !ServiceModel!.HowUse.Any(hu => hu is AttendingType.Online or AttendingType.Telephone)
                && ServiceModel.AllLocations.Any():

                backUrlPage = ServiceJourneyPage.Locations_For_Service;
                break;

            case ServiceJourneyPage.Locations_For_Service:
                if (!ServiceModel!.HowUse.Contains(AttendingType.InPerson))
                {
                    backUrlPage = ServiceJourneyPage.How_Use;
                }
                else if (ServiceModel.AddingLocations == false)
                {
                    backUrlPage = ServiceJourneyPage.Add_Location;
                }
                break;

            // case ServiceJourneyPage.Add_Location is handled in the overridden version of this method in select-location
        }

        return backUrlPage;
    }

    /// <summary>
    /// Returns an url that points to the previous page in the journey.
    /// </summary>
    /// <returns>The url that points to the previous page in the journey.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the code doesn't come up with the back page.
    /// Either because the code has missed a case or if it's called by a page that isn't supported.</exception>
    protected virtual string GenerateBackUrl()
    {
        ServiceJourneyPage? backUrlPage = null;

        if (ChangeFlow != null)
        {
            if (ChangeFlow == ServiceJourneyChangeFlow.SinglePage)
            {
                backUrlPage = ServiceJourneyPage.Service_Detail;
            }
            else
            {
                if (ChangeFlow == ServiceJourneyChangeFlow.LocalAuthority
                    && CurrentPage == ServiceJourneyPage.Local_Authority)
                {
                    backUrlPage = ServiceJourneyPage.Service_Detail;
                }
                else
                {
                    backUrlPage = PreviousPageAddFlow();

                    //todo: this is a bit dense. split it out a bit?
                    //todo: there's still a scenario where the user doesn't go back to the service details page
                    // when they're changing 'how use'
                    if ((ChangeFlow == ServiceJourneyChangeFlow.Location &&
                         (CurrentPage == ServiceJourneyPage.Locations_For_Service ||
                          backUrlPage <= ServiceJourneyPage.How_Use))
                        || (ChangeFlow == ServiceJourneyChangeFlow.HowUse && backUrlPage < ServiceJourneyPage.How_Use))
                    {
                        backUrlPage = ServiceJourneyPage.Service_Detail;
                    }
                }
            }
        }
        else if (Flow == JourneyFlow.Add)
        {
            backUrlPage = PreviousPageAddFlow();
            if (backUrlPage == ServiceJourneyPage.Initiator)
            {
                return GenerateBackUrlToJourneyInitiatorPage(ServiceModel!.ServiceType);
            }
        }

        if (backUrlPage == null)
        {
            throw new InvalidOperationException("Back page not set");
        }

        return GetServicePageUrl(backUrlPage.Value);
    }

    // we don't default serviceType to null even though we handle null, as the only times it should be null is when the cache has expired, which we handle here

    protected string GenerateBackUrlToJourneyInitiatorPage(ServiceTypeArg serviceType)
    {
        return GenerateBackUrlToJourneyInitiatorPage((ServiceTypeArg?)serviceType);
    }

    private string GenerateBackUrlToJourneyInitiatorPage(ServiceTypeArg? serviceType)
    {
        // when user is a dfe admin, the manage-services pages needs the service type, so that the add service link creates a service of the right type
        // (we won't have the service type if the cache has expired)
        if (FamilyHubsUser.Role == RoleTypes.DfeAdmin && serviceType == null)
        {
            return "/Welcome";
        }

        //todo: if the user is a dfe admin, they could have initially come from the welcome or services list pages, so ideally we should send them back to where they came from
        return $"/manage-services{(serviceType != null ? $"?serviceType={serviceType}" : "")}";
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

    //todo: or use QueryCollection?
    //todo: version with queryCollection and userinput - when it's needed
    protected IActionResult RedirectToSelf(IDictionary<string, StringValues> queryCollection, params ErrorId[] errors)
    {
        ServiceModel!.SetUserInput(null!);

        return RedirectToSelfInternal(queryCollection, errors);
    }

    protected IActionResult RedirectToSelf(TInput userInput, params ErrorId[] errors)
    {
        ServiceModel!.SetUserInput(userInput);

        return RedirectToSelfInternal(null, errors);
    }

    protected IActionResult RedirectToSelf(params ErrorId[] errors)
    {
        ServiceModel!.SetUserInput(null!);

        return RedirectToSelfInternal(null, errors);
    }

    private IActionResult RedirectToSelfInternal(IDictionary<string, StringValues>? queryCollection, params ErrorId[] errors)
    {
        //todo: have this as a helper method
        //// truncate at some large value, to stop a denial of service attack
        //var safeInvalidUserInput = invalidUserInput != null
        //    ? new[] { invalidUserInput[..Math.Min(invalidUserInput.Length, 4500)] }
        //    : null;

        //todo: throw if model null?
        ServiceModel!.AddErrorState(CurrentPage, errors);

        string extraQueries = queryCollection != null
            ? $"&{(string.Join("&", queryCollection.Select(q => $"{q.Key}={q.Value}")))}"
            : "";

        // if service-details ends up calling RedirectToSelf, we'd have to make sure back param doesn't get added
        return Redirect($"{GetServicePageUrl(CurrentPage)}{extraQueries}&redirectingToSelf=true");
    }

    protected ServiceJourneyPage? BackParam
    {
        get
        {
            var backValues = Request.Query["back"];
            if (backValues.Count == 1)
            {
                return ServiceJourneyPageExtensions.FromSlug(backValues[0]!);
            }

            return null;
        }
    }
}

#pragma warning restore S6605
