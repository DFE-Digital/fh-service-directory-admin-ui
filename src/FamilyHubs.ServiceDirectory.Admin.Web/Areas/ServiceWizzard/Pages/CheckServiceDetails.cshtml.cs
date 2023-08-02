using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.ServiceDirectory.Shared.Models;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Text;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.ServiceWizzard.Pages;

public class CheckServiceDetailsModel : PageModel
{
    public List<string> ServiceDeliverySelection { get; set; } = new List<string>();
    public List<TaxonomyDto> SelectedTaxonomy { get; set; } = new List<TaxonomyDto>();
    public OrganisationViewModel OrganisationViewModel { get; set; } = default!;
    public string UserFlow { get; set; } = default!;
    public string Address_1 { get; set; } = default!;
    public string Address_2 { get; set; } = default!;
    public string? Cost { get; set; }

    public string? Times { get; set; }

    public bool ShowSpreadsheetData { get; private set; } = false;

    private readonly IServiceDirectoryClient _serviceDirectoryClient;
    private readonly IViewModelToApiModelHelper _viewModelToApiModelHelper;
    private readonly IRequestDistributedCache _requestCache;

    public CheckServiceDetailsModel(IServiceDirectoryClient serviceDirectoryClient, IViewModelToApiModelHelper viewModelToApiModelHelper, IConfiguration configuration, IRequestDistributedCache requestCache)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
        _viewModelToApiModelHelper = viewModelToApiModelHelper;
        _requestCache = requestCache;
        ShowSpreadsheetData = configuration.GetValue<bool>("ShowSpreadsheetData");
    }

    public async Task<IActionResult> OnGet()
    {

        //UserFlow = _redis.RetrieveUserFlow();


        //if (_redis.RetrieveLastPageName() == ServiceAddedPageName)
        //{
        //    return RedirectToPage("/OrganisationAdmin/ErrorService");
        //}

        await InitPage();

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        var user = HttpContext.GetFamilyHubsUser();
        OrganisationViewModel? viewModel = await _requestCache.GetAsync(user.Email);
        
        if (viewModel != null)
        {
            long result = -1L;
            var OrganisationWithServicesRecord = await _viewModelToApiModelHelper.GenerateUpdateServiceDto(viewModel);
            if (OrganisationWithServicesRecord != null)
            {
                viewModel.ServiceId = OrganisationWithServicesRecord.Id;
            }

            if (OrganisationWithServicesRecord != null)
            {
                if (viewModel.Id <= 0)
                {
                    result = await _serviceDirectoryClient.CreateService(OrganisationWithServicesRecord);
                }
                else
                {
                    result = await _serviceDirectoryClient.UpdateService(OrganisationWithServicesRecord);
                }
            }

            if (result > 0)
                await _requestCache.SetAsync(user.Email, viewModel);
        }

        //_redis.StoreCurrentPageName(null);
        //_redis.StoreOrganisationWithService(null); 

        //UserFlow = _redis.RetrieveUserFlow();
        switch (UserFlow)
        {
            case "ManageService":
                return RedirectToPage("/OrganisationAdmin/DetailsSaved");

            default:
                return RedirectToPage("/OrganisationAdmin/ServiceAdded");

        }

    }

    private async Task InitPage()
    {
        var user = HttpContext.GetFamilyHubsUser();
        OrganisationViewModel? viewModel = await _requestCache.GetAsync(user.Email);
        if (viewModel == null)
        {
            return;
        }
        OrganisationViewModel = viewModel;
        SplitAddressFields();
        Cost = string.Format("{0:0.00}", OrganisationViewModel?.Cost);
        PaginatedList<TaxonomyDto> taxonomies = await _serviceDirectoryClient.GetTaxonomyList(1, 9999);

        if (taxonomies != null && OrganisationViewModel != null && OrganisationViewModel.TaxonomySelection != null)
        {
            foreach (var taxonomyKey in OrganisationViewModel.TaxonomySelection)
            {
                var taxonomy = taxonomies.Items.Find(x => x.Id == taxonomyKey);
                if (taxonomy != null)
                {
                    SelectedTaxonomy.Add(taxonomy);
                }
            }
        }

        var myEnumDescriptions = from ServiceDeliveryType n in Enum.GetValues(typeof(ServiceDeliveryType))
                                 select new { Id = (int)n, Name = Utility.GetEnumDescription(n) };

        Dictionary<int, string> dictServiceDelivery = new();
        foreach (var myEnumDescription in myEnumDescriptions)
        {
            if (myEnumDescription.Id == 0)
                continue;
            dictServiceDelivery[myEnumDescription.Id] = myEnumDescription.Name;
        }

        if (OrganisationViewModel != null && OrganisationViewModel.ServiceDeliverySelection != null)
        {
            foreach (var item in OrganisationViewModel.ServiceDeliverySelection)
            {
                if (int.TryParse(item, out var value))
                {
                    ServiceDeliverySelection.Add(dictServiceDelivery[value]);
                }
            }
        }

        if (OrganisationViewModel != null && OrganisationViewModel.OpeningHours != null && OrganisationViewModel.OpeningHours.Any())
        {

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < OrganisationViewModel.OpeningHours.Count; i++)
            {
                
                if (OrganisationViewModel.IsSameTimeOnEachDay == null || !OrganisationViewModel.IsSameTimeOnEachDay.Value)
                {
                    sb.Append($" {OrganisationViewModel.OpeningHours[i].Day} ");
                }
                sb.Append($"{OrganisationViewModel.OpeningHours[i].Starts} {OrganisationViewModel.OpeningHours[i].StartsTimeOfDay} to  {OrganisationViewModel.OpeningHours[i].Finishes} {OrganisationViewModel.OpeningHours[i].FinishesTimeOfDay}");

                if ((i + 1) < OrganisationViewModel.OpeningHours.Count)
                {
                    sb.Append($" and ");
                }
                
            }
            Times = sb.ToString();
        }
    }

    public IActionResult OnGetRedirectToViewServicesPage(string orgId)
    {
        return RedirectToPage("/OrganisationAdmin/ViewServices",
                                new
                                {
                                    orgId
                                });
    }

    private void SplitAddressFields()
    {
        if (string.IsNullOrEmpty(OrganisationViewModel.Address1))
            return;

        var modelAddress1 = OrganisationViewModel.Address1;

        Address_1 = modelAddress1;
        Address_2 = String.Empty;

        if (modelAddress1.Contains("|"))
        {
            Address_1 = modelAddress1.Substring(0, modelAddress1.IndexOf("|"));
            Address_2 = modelAddress1.Substring(modelAddress1.IndexOf("|") + 1);
        }

    }
}
