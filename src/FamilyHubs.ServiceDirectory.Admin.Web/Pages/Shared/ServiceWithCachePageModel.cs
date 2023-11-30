using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Errors;
using FamilyHubs.SharedKernel.Razor.ErrorNext;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;

public class ServiceWithCachePageModel<TInput> : ServiceWithCachePageModel
    where TInput : class
{
    public new ServiceModel<TInput>? ServiceModel { get; set; }

    //protected TInput? UserInput { get; private set; }

    protected ServiceWithCachePageModel(
        ServiceJourneyPage page,
        IRequestDistributedCache connectionRequestCache)
        : base(page, connectionRequestCache)
    {
    }

    protected IActionResult RedirectToSelf(TInput userInput, params ErrorId[] errors)
    {
        base.ServiceModel!.UserInputType = typeof(TInput).FullName;
        base.ServiceModel.UserInput = userInput;

        return RedirectToSelf(errors);
    }

    protected override async Task GetAndKeepServiceModelWithUserInputAsync()
    {
        //todo: cleaner way than 'new' ServiceModel?
        base.ServiceModel = ServiceModel = await Cache.GetAsync<ServiceModel<TInput>>(FamilyHubsUser.Email);

        // handle this scenario:
        // we redirect to self with user input, then the browser shuts down before the get, then later another page is fetched.
        // without this check, we get an instance of TInput with all the properties set to default values
        // (unless the actual TInput in the cache happens to share property names/types with the TInput we're expecting, in which case we'll get some duff data)
        // we could store the wip input in the model's usual properties, but how would we handle error => redirect get => back => next. at this state would want a default page, not an errored page
        if (ServiceModel?.UserInputType != null
            && ServiceModel.UserInputType != typeof(TInput).FullName)
        {
            // setting it on the base ServiceModel is not strictly necessary, but it follows the least surprise principle
            base.ServiceModel!.UserInput = null;
            ServiceModel.UserInput = null;
        }

        //todo: could store and check UserInput in here
    }

    //protected override async Task<IActionResult> OnSafeGetAsync(CancellationToken cancellationToken)
    //{
    //    var result = await SetupGet();
    //    if (result != null)
    //    {
    //        return result;
    //    }
    //    //var result = base.OnSafeGetAsync(cancellationToken);

    //    //todo: what happens if we redirect with user input, but browser shuts down before the get, then another page is got next time?
    //    // need to gracefully handle this, and not throw an exception i.e. if different type in cache set it to null
    //    // will it do that automatically if try to deserialise to wrong type?
    //    // or do we need to add a type discriminator to the cache entry? nameof(TInput) or something? <= probably the safest option
    //    // what it does: creates an instance of what is actually the wrong type, with all the properties set to default values (unless they happen to share a property name and type)
    //    // so we have to have a discriminator, check it, and null it out if wrong type (or not deserialize it in the first place if possible)
    //    //todo: poc only!!!!
    //    ServiceModel = await Cache.GetAsync<ServiceModel<TInput>>(FamilyHubsUser.Email);

    //    //todo: set up UserInput like this, or new ServiceModel in this derived class, and let the consumer get from the model?
    //    //todo: better to have base call overridable method?
    //    UserInput = ServiceModel?.UserInput;

    //    await OnGetWithModelAsync(cancellationToken);

    //    return Page();
    //}
}

//todo: we don't have a non-form page at the start of the journey, so we can probably merge ServiceWithCachePageModel and ServicePageModel
public class ServiceWithCachePageModel : ServicePageModel
{
    public ServiceModel? ServiceModel { get; set; }
    public IErrorState Errors { get; private set; }
    //todo: rename
    protected bool _redirectingToSelf;

    protected ServiceWithCachePageModel(
        ServiceJourneyPage page,
        IRequestDistributedCache connectionRequestCache)
        : base(page, connectionRequestCache)
    {
        Errors = ErrorState.Empty;
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

    protected virtual async Task GetAndKeepServiceModelWithUserInputAsync()
    {
        ServiceModel = await Cache.GetAsync<ServiceModel>(FamilyHubsUser.Email);
    }

    protected async Task<IActionResult?> SetupGet()
    {
        if (Flow == JourneyFlow.Edit && !RedirectingToSelf)
        {
            //todo: when in Edit mode, it's only the errorstate that we actually need in the cache
            ServiceModel = await Cache.SetAsync(FamilyHubsUser.Email, new ServiceModel());
        }
        else
        {
            //ServiceModel = await Cache.GetAsync<ServiceModel>(FamilyHubsUser.Email);
            await GetAndKeepServiceModelWithUserInputAsync();
            if (ServiceModel == null)
            {
                // the journey cache entry has expired and we don't have a model to work with
                // likely the user has come back to this page after a long time
                return Redirect(GetServicePageUrl(ServiceJourneyPage.Initiator, ServiceId, Flow));
            }
        }

        if (ServiceModel.ErrorState?.Page == CurrentPage)
        {
            Errors = ErrorState.Create(PossibleErrors.All, ServiceModel.ErrorState.Errors);
        }
        else
        {
            // we don't save the model on Get, but we don't want the page to pick up the error state when the user has gone back
            // (we'll clear the error state in the model on a non-redirect to self post
            ServiceModel.ErrorState = null;
            Errors = ErrorState.Empty;
        }

        return null;
    }

    protected override async Task<IActionResult> OnSafeGetAsync(CancellationToken cancellationToken)
    {
        var result = await SetupGet();
        if (result != null)
        {
            return result;
        }

        //if (Flow == JourneyFlow.Edit && !RedirectingToSelf)
        //{
        //    //todo: when in Edit mode, it's only the errorstate that we actually need in the cache
        //    ServiceModel = await Cache.SetAsync(FamilyHubsUser.Email, new ServiceModel());
        //}
        //else
        //{
        //    ServiceModel = await Cache.GetAsync<ServiceModel>(FamilyHubsUser.Email);
        //    if (ServiceModel == null)
        //    {
        //        // the journey cache entry has expired and we don't have a model to work with
        //        // likely the user has come back to this page after a long time
        //        return Redirect(GetServicePageUrl(ServiceJourneyPage.Initiator, ServiceId, Flow));
        //    }
        //}

        //if (ServiceModel.ErrorState?.Page == CurrentPage)
        //{
        //    Errors = ErrorState.Create(PossibleErrors.All, ServiceModel.ErrorState.Errors);
        //}
        //else
        //{
        //    // we don't save the model on Get, but we don't want the page to pick up the error state when the user has gone back
        //    // (we'll clear the error state in the model on a non-redirect to self post
        //    ServiceModel.ErrorState = null;
        //    Errors = ErrorState.Empty;
        //}

        await OnGetWithModelAsync(cancellationToken);

        return Page();
    }

    protected override async Task<IActionResult> OnSafePostAsync(CancellationToken cancellationToken)
    {
        // we don't need to retrieve UserInput on a post. this effectively clears it
        ServiceModel = await Cache.GetAsync<ServiceModel>(FamilyHubsUser.Email);
        if (ServiceModel == null)
        {
            // the journey cache entry has expired and we don't have a model to work with
            // likely the user has come back to this page after a long time
            return Redirect(GetServicePageUrl(ServiceJourneyPage.Initiator, ServiceId, Flow));
        }

        var result = await OnPostWithModelAsync(cancellationToken);

        if (!_redirectingToSelf)
        {
            ServiceModel.ErrorState = null;
        }

        await Cache.SetAsync(FamilyHubsUser.Email, ServiceModel);

        return result;
    }

    protected IActionResult RedirectToSelf(params ErrorId[] errors)
    {
        //todo: throw if none? is that something this should be used for?
        if (errors.Any())
        {
            //// truncate at some large value, to stop a denial of service attack
            //var safeInvalidUserInput = invalidUserInput != null
            //    ? new[] { invalidUserInput[..Math.Min(invalidUserInput.Length, 4500)] }
            //    : null;

            //todo: throw if model null?
            ServiceModel!.ErrorState = new ServiceErrorState(CurrentPage, errors);
        }

        _redirectingToSelf = true;

        return RedirectToServicePage(CurrentPage, Flow, true);
    }
}