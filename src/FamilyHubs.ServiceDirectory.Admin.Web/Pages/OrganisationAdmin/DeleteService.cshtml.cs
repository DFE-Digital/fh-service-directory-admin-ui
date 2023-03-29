using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.OrganisationAdmin;
public class DeleteServiceModel : PageModel
{
    private readonly IOrganisationAdminClientService _organisationAdminClientService;

    public bool ValidationValid { get; set; } = true;

    public DeleteServiceModel(
        IOrganisationAdminClientService organisationAdminClientService)
    {
        _organisationAdminClientService = organisationAdminClientService;
    }

    public List<string> SelectOptions { get; set; } = default!;
    [BindProperty] public string SelectedOption { get; set; } = default!;
    private const string OptionYes = "Yes, I want to delete it";
    private const string OptionNo = "No, I want to keep it";
    public ServiceDto Service { get; private set; } = default!;

    public async Task OnGet(long organisationId, long serviceId)
    {
        await Init(serviceId);
    }

    public async Task<IActionResult> OnPost(long serviceId)
    {
        ValidationValid = ModelState.IsValid;
        if (!ModelState.IsValid)
        {
            await Init(serviceId);
            return Page();
        }

        if (SelectedOption == OptionYes)
        {
            var serviceDeleted = await _organisationAdminClientService.DeleteServiceById(serviceId);

            return RedirectToPage(!serviceDeleted 
                ? "/OrganisationAdmin/ServiceNotDeleted" 
                : "/OrganisationAdmin/ServiceDeleted");
        }

        return RedirectToPage("/OrganisationAdmin/ServiceNotDeleted");
    }

    private async Task Init(long serviceId)
    {
        Service = await _organisationAdminClientService.GetServiceById(serviceId);

        //TODO - throw exception;
        SelectOptions = new List<string> { OptionYes, OptionNo };
    }
}
