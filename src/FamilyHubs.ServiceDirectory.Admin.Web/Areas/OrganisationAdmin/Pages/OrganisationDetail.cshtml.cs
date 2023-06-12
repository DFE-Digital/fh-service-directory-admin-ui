using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.OrganisationAdmin.Pages;

public class OrganisationDetailModel : PageModel
{
    private readonly IServiceDirectoryClient _serviceDirectoryClient;

    [BindProperty]
    public OrganisationViewModel OrganisationViewModel{ get; set; } = new OrganisationViewModel();

    public OrganisationDetailModel(IServiceDirectoryClient serviceDirectoryClient)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
    }
    public async Task OnGetAsync(long? id)
    {
        if (id != null)
        {
            var organisation = await _serviceDirectoryClient.GetOrganisationById(id.Value);
            OrganisationViewModel = new OrganisationViewModel
            {
                Id = id.Value,
                Name = organisation?.Name,
                Description = organisation?.Description,
                Uri = organisation?.Uri,
                Url = organisation?.Url,
                Logo = organisation?.Logo
            };
        }   
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var strOrganisationViewModel = JsonConvert.SerializeObject(OrganisationViewModel);

        return RedirectToPage("/OrganisationAdmin/Welcome", new
        {
            strOrganisationViewModel
        });
    }
}
