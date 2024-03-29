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
        bool redirectingToSelf = false,
        CancellationToken cancellationToken = default)
    {
        Flow = JourneyFlowExtensions.FromUrlString(flow);

        RedirectingToSelf = redirectingToSelf;

        //todo: could do with a version that just gets the email address
        FamilyHubsUser = HttpContext.GetFamilyHubsUser();

        ServiceModel = await Cache.GetAsync<ServiceModel<TInput>>(FamilyHubsUser.Email);
        if (ServiceModel == null)
        {
            // the journey cache entry has expired and we don't have a model to work with
            // likely the user has come back to this page after a long time
            return Redirect(GetServicePageUrl(ServiceJourneyPage.Initiator, Flow));
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
        CancellationToken cancellationToken = default)
    {
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
            return Redirect(GetServicePageUrl(ServiceJourneyPage.Initiator, Flow));
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

    public string GetServicePageUrl(
        ServiceJourneyPage page,
        JourneyFlow? flow = null,
        bool redirectingToSelf = false,
        IDictionary<string, StringValues>? queryCollection = null)
    {
        flow ??= Flow;

        string redirectingToSelfParam = redirectingToSelf ? "&redirectingToSelf=true" : "";

        string extraQueries = queryCollection != null
            ? $"&{(string.Join("&", queryCollection.Select(q => $"{q.Key}={q.Value}")))}"
            : "";

        return $"{page.GetPagePath(flow.Value)}?flow={flow.Value.ToUrlString()}{redirectingToSelfParam}{extraQueries}";
    }

    protected IActionResult RedirectToServicePage(
        ServiceJourneyPage page,
        JourneyFlow flow,
        bool redirectingToSelf = false,
        IDictionary<string, StringValues>? queryCollection = null)
    {
        return Redirect(GetServicePageUrl(page, flow, redirectingToSelf, queryCollection));
    }

    // NextPage should handle skips in a linear journey
    protected IActionResult NextPage()
    {
        ServiceJourneyPage nextPage;
        if (Flow == JourneyFlow.Add)
        {
            nextPage = CurrentPage + 1;
            switch (nextPage)
            {
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
        }
        else
        {
            nextPage = ServiceJourneyPage.Service_Detail;
        }

        return RedirectToServicePage(nextPage, Flow == JourneyFlow.AddRedo ? JourneyFlow.Add : Flow);
    }

    protected string GenerateBackUrl()
    {
        ServiceJourneyPage backUrlPage;
        if (Flow == JourneyFlow.Add)
        {
            backUrlPage = CurrentPage - 1;
            switch (backUrlPage)
            {
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
            }
        }
        else
        {
            backUrlPage = ServiceJourneyPage.Service_Detail;
        }

        return GetServicePageUrl(backUrlPage, Flow == JourneyFlow.AddRedo ? JourneyFlow.Add : Flow);
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

        return RedirectToServicePage(CurrentPage, Flow, true, queryCollection);
    }
}

#pragma warning restore S6605
