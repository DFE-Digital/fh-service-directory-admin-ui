using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServices;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using FamilyHubs.SharedKernel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages;

public class LocalOfferResultsModel : PageModel
{
    private readonly ILocalOfferClientService _localOfferClientService;

    public double CurrentLatitude { get; set; }
    public double CurrentLongitude { get; set; }

    public PaginatedList<OpenReferralServiceDto> SearchResults { get; set; } = default!;

    public string SelectedDistance { get; set; } = "212892";

    [BindProperty(SupportsGet = true)]
    public string MinimumAge { get; set; } = "0";
    [BindProperty(SupportsGet = true)]
    public string MaximumAge { get; set; } = "99";
    [BindProperty(SupportsGet = true)]
    public string? SearchText { get; set; }

    public List<SelectListItem> DistanceSelectionList { get; } = new List<SelectListItem>
    {
        new SelectListItem { Value = "1609.34", Text = "1 mile" },
        new SelectListItem { Value = "3218.69", Text = "2 miles" },
        new SelectListItem { Value = "8046.72", Text = "5 miles" },
        new SelectListItem { Value = "16093.4", Text = "10 miles" },
        new SelectListItem { Value = "24140.2", Text = "15 miles" },
        new SelectListItem { Value = "32186.9", Text = "20 miles" },
    };

    public LocalOfferResultsModel(ILocalOfferClientService localOfferClientService)
    {
        _localOfferClientService = localOfferClientService;
    }

    public async Task OnGetAsync(double latitude, double longitude, double distance, string minimumAge, string maximumAge, string searchText)
    {
        CurrentLatitude = latitude;
        CurrentLongitude = longitude;
        SelectedDistance = distance.ToString();
        if (!int.TryParse(minimumAge, out var minAge))
        {
            minAge = 0;
        }

        if (!int.TryParse(maximumAge, out var maxAge))
        {
            maxAge = 99;
        }

        SearchResults = await _localOfferClientService.GetLocalOffers("Information Sharing", "active", minAge, maxAge, latitude, longitude, distance, 1, 99, SearchText ?? string.Empty, null, null, null);
    }

    public IActionResult OnPost()
    {
        SelectedDistance = Request.Form["SelectedDistance"].ToString();
        if (double.TryParse(Request.Form["CurrentLatitude"], out var currentLatitude))
        {
            CurrentLatitude = currentLatitude;
        }
        if (double.TryParse(Request.Form["CurrentLongitude"], out var currentLongitude))
        {
            CurrentLongitude = currentLongitude;
        }

        return RedirectToPage("LocalOfferResults", new
        {
            latitude = CurrentLatitude,
            longitude = CurrentLongitude,
            distance = SelectedDistance,
            minimumAge = MinimumAge,
            maximumAge = MaximumAge,
            searchText = SearchText
        });

    }
}
