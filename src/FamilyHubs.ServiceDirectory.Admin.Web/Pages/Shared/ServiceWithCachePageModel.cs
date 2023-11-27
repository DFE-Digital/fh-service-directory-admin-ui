using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Errors;
using FamilyHubs.SharedKernel.Razor.ErrorNext;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;

//todo: we don't have a non-form page at the start of the journey, so we can probably merge ServiceWithCachePageModel and ServicePageModel
public class ServiceWithCachePageModel : ServicePageModel
{
    //todo: we could stop passing this to get/set
    public ServiceModel? ServiceModel { get; set; }
    public IErrorState Errors { get; private set; }
    private bool _redirectingToSelf;

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

    protected override async Task<IActionResult> OnSafeGetAsync(CancellationToken cancellationToken)
    {
        //todo: start page won't have when enter, but will need if PRG. do we add a param to say self redirect?

        ServiceModel = await Cache.GetServiceAsync(FamilyHubsUser.Email);
        if (ServiceModel == null)
        {
            if (CurrentPage == ServiceJourneyPageExtensions.GetAddFlowStartPage()
                && !RedirectingToSelf)
            {
                // the user's just starting the journey
                ServiceModel = new ServiceModel();
                await Cache.SetServiceAsync(FamilyHubsUser.Email, ServiceModel);
            }
            else
            {
                // the journey cache entry has expired and we don't have a model to work with
                // likely the user has come back to this page after a long time
                return Redirect(ServiceJourneyPageExtensions.GetInitiatorPagePath(Flow));
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
        ServiceModel = await Cache.GetServiceAsync(FamilyHubsUser.Email);
        if (ServiceModel == null)
        {
            // the journey cache entry has expired and we don't have a model to work with
            // likely the user has come back to this page after a long time
            return Redirect(ServiceJourneyPageExtensions.GetInitiatorPagePath(Flow));
        }

        var result = await OnPostWithModelAsync(cancellationToken);

        if (!_redirectingToSelf)
        {
            ServiceModel.ErrorState = null;
        }

        await Cache.SetServiceAsync(FamilyHubsUser.Email, ServiceModel);

        return result;
    }

    //todo: version that accepts array of user input, or dictionary?
    protected IActionResult RedirectToSelf(string? invalidUserInput, params ErrorId[] errors)
    {
        //todo: throw if none? is that something this should be used for?
        if (errors.Any())
        {
            // truncate at some large value, to stop a denial of service attack
            var safeInvalidUserInput = invalidUserInput != null
                ? new[] { invalidUserInput[..Math.Min(invalidUserInput.Length, 4500)] }
                : null;

            //todo: throw if model null?
            ServiceModel!.ErrorState =
                new ServiceErrorState(CurrentPage, errors, safeInvalidUserInput);
        }

        _redirectingToSelf = true;

        return RedirectToServicePage(CurrentPage, Flow, true);
    }
}