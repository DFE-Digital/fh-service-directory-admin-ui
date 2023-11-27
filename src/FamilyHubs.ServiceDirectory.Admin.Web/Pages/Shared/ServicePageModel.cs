﻿using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.SharedKernel.Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;

public enum JourneyFlow
{
    Add,
    AddRedo,
    Edit
}

[Authorize(Roles = RoleGroups.AdminRole)]
public class ServicePageModel : HeaderPageModel
{
    protected readonly ServiceJourneyPage CurrentPage;
    protected IRequestDistributedCache Cache { get; }
    // not set in ctor, but will always be there in Get/Set handlers
    public string ServiceId { get; set; } = default!;
    public JourneyFlow Flow { get; set; }
    public string? BackUrl { get; set; }
    //todo: if keep, rename
    // not set in ctor, but will always be there in Get/Post handlers
    public FamilyHubsUser ProfessionalUser { get; set; } = default!;

    public ServicePageModel(ServiceJourneyPage page, IRequestDistributedCache cache)
    {
        Cache = cache;
        CurrentPage = page;
    }

    protected virtual Task<IActionResult> OnSafeGetAsync()
    {
        return Task.FromResult((IActionResult)Page());
    }

    protected virtual Task<IActionResult> OnSafePostAsync()
    {
        return Task.FromResult((IActionResult)Page());
    }

    public async Task<IActionResult> OnGetAsync(string serviceId, string? changing = null)
    {
        //todo: error or redirect?
        if (serviceId == null)
        {
            // someone's been monkeying with the query string and we don't have the service details we need
            // we can't send them back to the start of the journey because we don't know what service they were looking at
            // so we'll just send them to the menu page
            return RedirectToPage("/Welcome");
        }

        ServiceId = serviceId;
        Flow = GetFlow(changing);

        // default, but can be overridden
        BackUrl = GenerateBackUrl();

        //todo: could do with a version that just gets the email address
        ProfessionalUser = HttpContext.GetFamilyHubsUser();

        return await OnSafeGetAsync();
    }

    protected JourneyFlow GetFlow(string? changing)
    {
        return changing switch
        {
            //todo: better names? adding or editing?
            "page" => JourneyFlow.Edit,
            _ => JourneyFlow.Add
        };
    }

    protected string? GetChanging(JourneyFlow flow)
    {
        return flow switch
        {
            JourneyFlow.Edit => "page",
            _ => null
        };
    }

    public async Task<IActionResult> OnPostAsync(string serviceId, string? changing = null)
    {
        ServiceId = serviceId;

        Flow = GetFlow(changing);

        // default, but can be overridden
        BackUrl = GenerateBackUrl();

        ProfessionalUser = HttpContext.GetFamilyHubsUser();

        return await OnSafePostAsync();
    }

    class RouteValues
    {
        public string? ServiceId { get; set; }
        public string? Changing { get; set; }
    }

    //protected IActionResult RedirectToServicePage(string page, string? changing = null)
    //{
    //    return RedirectToPage($"/{page}", new RouteValues
    //    {
    //        ServiceId = ServiceId,
    //        Changing = changing
    //    });
    //}

    private string GetPageName(ServiceJourneyPage page)
    {
        return page.ToString().Replace('_', '-');
    }

    protected IActionResult RedirectToServicePage(ServiceJourneyPage page, string? changing = null)
    {
        return RedirectToPage($"/{GetPageName(page)}", new RouteValues
        {
            ServiceId = ServiceId,
            Changing = changing
        });
    }

    protected IActionResult NextPage()
    {
        ServiceJourneyPage nextPage = Flow == JourneyFlow.Edit ? ServiceJourneyPage.Details : CurrentPage + 1;

        return RedirectToServicePage(nextPage);
    }

    protected string GenerateBackUrl()
    {
        ServiceJourneyPage? backUrlPage = Flow == JourneyFlow.Edit ? ServiceJourneyPage.Details : CurrentPage-1;

        //todo: check ServiceId for null
        return $"/{backUrlPage}?serviceId={ServiceId}";
    }
}