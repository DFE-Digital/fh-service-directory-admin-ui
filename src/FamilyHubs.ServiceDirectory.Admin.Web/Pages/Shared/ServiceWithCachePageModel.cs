using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Errors;
using FamilyHubs.SharedKernel.Razor.ErrorNext;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;

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

    //todo: don't pass model. naming?
    protected virtual void OnGetWithModel(ServiceModel model)
    {
    }

    protected virtual Task OnGetWithModelAsync(ServiceModel model)
    {
        OnGetWithModel(model);

        return Task.CompletedTask;
    }

    protected virtual IActionResult OnPostWithModel(ServiceModel model)
    {
        return Page();
    }

    protected virtual Task<IActionResult> OnPostWithModelAsync(ServiceModel model)
    {
        return Task.FromResult(OnPostWithModel(model));
    }

    protected override async Task<IActionResult> OnSafeGetAsync(CancellationToken cancellationToken)
    {
        ServiceModel = await Cache.GetServiceAsync(ProfessionalUser.Email);
        if (ServiceModel == null)
        {
            // the journey cache entry has expired and we don't have a model to work with
            // likely the user has come back to this page after a long time
            // (even in edit mode, we'd still potentially need the error state)
            //todo: where we send them will change depending on if we're managing or adding
            return RedirectToServicePage(ServiceJourneyPage.Details);
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

        await OnGetWithModelAsync(ServiceModel);

        return Page();
    }

    protected override async Task<IActionResult> OnSafePostAsync(CancellationToken cancellationToken)
    {
        ServiceModel = await Cache.GetServiceAsync(ProfessionalUser.Email);
        if (ServiceModel == null)
        {
            // the journey cache entry has expired and we don't have a model to work with
            // likely the user has come back to this page after a long time
            //todo: where we send them will change depending on if we're managing or adding
            return RedirectToServicePage(ServiceJourneyPage.Details);
        }

        var result = await OnPostWithModelAsync(ServiceModel);

        if (!_redirectingToSelf)
        {
            ServiceModel.ErrorState = null;
        }

        await Cache.SetServiceAsync(ProfessionalUser.Email, ServiceModel);

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

        return RedirectToServicePage(CurrentPage, Flow);
    }
}