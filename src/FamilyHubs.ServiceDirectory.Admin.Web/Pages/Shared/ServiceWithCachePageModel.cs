using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Errors;
using FamilyHubs.SharedKernel.Razor.ErrorNext;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;

//public class ServiceWithCachePageModel<TInput> : ServiceWithCachePageModel
//    where TInput : class
//{
//    public new ServiceModel<TInput>? ServiceModel { get; set; }

//    //protected TInput? UserInput { get; private set; }

//    protected ServiceWithCachePageModel(
//        ServiceJourneyPage page,
//        IRequestDistributedCache connectionRequestCache)
//        : base(page, connectionRequestCache)
//    {
//    }

//    protected IActionResult RedirectToSelf(TInput userInput, params ErrorId[] errors)
//    {
//        base.ServiceModel!.UserInputType = typeof(TInput).FullName;
//        base.ServiceModel.UserInput = userInput;

//        return RedirectToSelf(errors);
//    }

//    protected override async Task GetAndKeepServiceModelWithUserInputAsync()
//    {
//        //todo: cleaner way than 'new' ServiceModel?
//        base.ServiceModel = ServiceModel = await Cache.GetAsync<ServiceModel<TInput>>(FamilyHubsUser.Email);

//        //todo: tie in with redirecting to self
//        //todo: what if redirecting to self is set in url, and user uses browser back button?

//        // handle this scenario:
//        // we redirect to self with user input, then the browser shuts down before the get, then later another page is fetched.
//        // without this check, we get an instance of TInput with all the properties set to default values
//        // (unless the actual TInput in the cache happens to share property names/types with the TInput we're expecting, in which case we'll get some duff data)
//        // we could store the wip input in the model's usual properties, but how would we handle error => redirect get => back => next. at this state would want a default page, not an errored page
//        if (ServiceModel?.UserInputType != null
//            && ServiceModel.UserInputType != typeof(TInput).FullName)
//        {
//            // setting it on the base ServiceModel is not strictly necessary, but it follows the least surprise principle
//            base.ServiceModel!.UserInput = null;
//            ServiceModel.UserInput = null;
//        }

//        //todo: could store and check UserInput in here
//    }
//}

//todo: we don't have a non-form page at the start of the journey, so we can probably merge ServiceWithCachePageModel and ServicePageModel
public class ServiceWithCachePageModel<TInput> : ServicePageModel where TInput : class
{
    public ServiceModel<TInput>? ServiceModel { get; set; }
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

    //todo: mignt not have to break this out
    protected virtual async Task GetAndKeepServiceModelWithUserInputAsync()
    {
        ServiceModel = await Cache.GetAsync<ServiceModel<TInput>>(FamilyHubsUser.Email);

        //todo: tie in with redirecting to self
        //todo: what if redirecting to self is set in url, and user uses browser back button?

        // handle this scenario:
        // we redirect to self with user input, then the browser shuts down before the get, then later another page is fetched.
        // without this check, we get an instance of TInput with all the properties set to default values
        // (unless the actual TInput in the cache happens to share property names/types with the TInput we're expecting, in which case we'll get some duff data)
        // we could store the wip input in the model's usual properties, but how would we handle error => redirect get => back => next. at this state would want a default page, not an errored page
        if (ServiceModel?.UserInputType != null
            && ServiceModel.UserInputType != typeof(TInput).FullName)
        {
            ServiceModel.UserInput = default;
        }

        //todo: could store and check UserInput in here
    }

    //protected virtual async Task GetAndKeepServiceModelWithUserInputAsync()
    //{
    //    ServiceModel = await Cache.GetAsync<ServiceModel>(FamilyHubsUser.Email);
    //}

    protected override async Task<IActionResult> OnSafeGetAsync(CancellationToken cancellationToken)
    {
        if (Flow == JourneyFlow.Edit && !RedirectingToSelf)
        {
            //todo: when in Edit mode, it's only the errorstate that we actually need in the cache
            ServiceModel = await Cache.SetAsync(FamilyHubsUser.Email, new ServiceModel<TInput>());
        }
        else
        {
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

        await OnGetWithModelAsync(cancellationToken);

        return Page();
    }

    protected override async Task<IActionResult> OnSafePostAsync(CancellationToken cancellationToken)
    {
        // we don't need to retrieve UserInput on a post. this effectively clears it
        ServiceModel = await Cache.GetAsync<ServiceModel<TInput>>(FamilyHubsUser.Email);
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

    protected IActionResult RedirectToSelf(TInput userInput, params ErrorId[] errors)
    {
        ServiceModel!.UserInputType = typeof(TInput).FullName;
        ServiceModel.UserInput = userInput;

        return RedirectToSelf(errors);
    }

    protected IActionResult RedirectToSelf(params ErrorId[] errors)
    {
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