using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

public class OrganisationDetailModel : PageModel
{
    private readonly IOpenReferralOrganisationAdminClientService _openReferralOrganisationAdminClientService;


    [BindProperty]
    public OrganisationViewModel OrganisationViewModel { get; set; } = new OrganisationViewModel();

    public OrganisationDetailModel(IOpenReferralOrganisationAdminClientService openReferralOrganisationAdminClientService)
    {
        _openReferralOrganisationAdminClientService = openReferralOrganisationAdminClientService;
    }
    public async Task OnGetAsync(Guid? id)
    {
        if (id != null)
        {
            var organisation = await _openReferralOrganisationAdminClientService.GetOpenReferralOrganisationById(id.Value.ToString());
            OrganisationViewModel = new OrganisationViewModel
            {
                Id = id.Value,
                Name = organisation.Name,
                Description = organisation.Description,
                Uri = organisation.Uri,
                Url = organisation.Url,
                Logo = organisation.Logo

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

    /*
        public async Task<IActionResult> OnPost()
        {
    //        string[] KeysToIgnore = { "Tenant", "Services", "OrganisationType" };
    //        foreach (var key in KeysToIgnore)
    //        {            
    //#pragma warning disable CS8602 // Dereference of a possibly null reference.
    //            if (ModelState[key] is not null && ModelState[key].Errors.Any())
    //                ModelState.Remove(key);
    //#pragma warning restore CS8602 // Dereference of a possibly null reference.
    //        }

    //        var tenant = await GetTenantById(TenantId);
    //        Organisation.Tenant = _mapper.Map<TenantDto>(tenant);
    //        var organisationType = await GetOrganisationTypeById(new Guid(SelectedOrganisationType));
    //        Organisation.OrganisationType = _mapper.Map<OrganisationTypeDto>(organisationType);

            //if (!ModelState.IsValid)
            //{
            //    await PopulateOrganisationTypeList(Organisation?.OrganisationType?.Name);
            //    return Page();
            //}



            //Organisation.Id = OrganisationId;
            //Organisation.OrganisationType.Id = OrganisationTypeId;
            //Organisation.Tenant.Id = TenantId;

            //Guid retVal;

            //if (Organisation.Id == Guid.Empty)
            //{
            //    var organisation = _mapper.Map<Organisation>(Organisation);
            //    retVal = await _organisationAdminClientService.CreateOrganisation(
            //        organisation.Tenant.Id,
            //        organisation.Name,
            //        organisation.Description,
            //        organisation.LogoUrl,
            //        organisation.LogoAltText,
            //        organisation.OrganisationType.Id,
            //        ContactName,
            //        ContactEmail,
            //        new List<Service>()
            //        );
            //}
            //else
            //{
            //    var orginalOrganisation = await _organisationAdminClientService.GetOrganisationById(Organisation.Id);
            //    var organisation = _mapper.Map<Organisation>(Organisation);
            //    organisation.Id = Organisation.Id;
            //    retVal = await _organisationAdminClientService.UpdateOrganisation(
            //        organisation.Id,
            //        organisation.Tenant.Id,
            //        organisation.Name,
            //        organisation.Description,
            //        organisation.LogoUrl,
            //        organisation.LogoAltText,
            //        organisation.OrganisationType.Id,
            //        (orginalOrganisation.Contact != null) ? orginalOrganisation.Contact.Id : Guid.Empty,
            //        ContactName,
            //        ContactEmail,
            //        orginalOrganisation.Services);
            //}

            //return RedirectToPage("/OrganisationAdmin/CheckOrganisationDetailAnswers", new
            //{
            //    id = retVal
            //});
        }
    */
}
