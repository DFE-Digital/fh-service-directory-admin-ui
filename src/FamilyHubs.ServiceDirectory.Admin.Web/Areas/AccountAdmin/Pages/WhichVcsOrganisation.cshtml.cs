﻿using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages;

public class WhichVcsOrganisation : AccountAdminViewModel
{
    private readonly ICacheService _cacheService;
    private readonly IOrganisationAdminClientService _serviceDirectoryClient;

    public WhichVcsOrganisation(ICacheService cacheService, IOrganisationAdminClientService serviceDirectoryClient)
    {
        PageHeading = "Which organisation do they work for?";
        ErrorMessage = "Select an organisation";
        BackLink = "/WhichLocalAuthority";
        _cacheService = cacheService;
        _serviceDirectoryClient = serviceDirectoryClient;
    }

    [BindProperty]
    public required string VcsOrganisationName { get; set; } = string.Empty;

    public required List<string> VcsOrganisations { get; set; } = new List<string>();

    public async Task OnGet()
    {
        var vcsOrganisations = await TryGetVcsOrganisationsFromCache();
        VcsOrganisations = vcsOrganisations.Select(l => l.Name).ToList();
        
        var permissionModel = await _cacheService.GetPermissionModel();
        
        if (permissionModel is not null)
        {
            VcsOrganisationName = permissionModel.VcsOrganisationName;
        }
    }

    public async Task<IActionResult> OnPost()
    {
        var vcsOrganisations = await TryGetVcsOrganisationsFromCache();

        if (ModelState.IsValid && !string.IsNullOrWhiteSpace(VcsOrganisationName) && VcsOrganisationName.Length <= 255)
        {
            var permissionModel = await _cacheService.GetPermissionModel();
            ArgumentNullException.ThrowIfNull(permissionModel);
            
            permissionModel.OrganisationId = vcsOrganisations.Single(l => l.Name == VcsOrganisationName).Id;
            permissionModel.VcsOrganisationName = VcsOrganisationName;

            await _cacheService.StorePermissionModel(permissionModel);

            return RedirectToPage("/UserEmail");
        }
        
        HasValidationError = true;
        
        VcsOrganisations = vcsOrganisations.Select(l => l.Name).ToList();

        return Page();
    }

    private async Task<List<OrganisationDto>> TryGetVcsOrganisationsFromCache(CancellationToken cancellationToken = default)
    {
        var semaphore = new SemaphoreSlim(1, 1);
        var vcsOrganisations = await _cacheService.GetVcsOrganisations();
        if (vcsOrganisations is not null)
            return vcsOrganisations;

        try
        {
            await semaphore.WaitAsync(cancellationToken);

            // recheck to make sure it didn't populate before entering semaphore
            vcsOrganisations = await _cacheService.GetVcsOrganisations();
            if (vcsOrganisations is not null)
                return vcsOrganisations;

            var organisations = await _serviceDirectoryClient.GetListOrganisations();
            vcsOrganisations = organisations.Where(x => x.OrganisationType == OrganisationType.VCFS).ToList();

            await _cacheService.StoreVcsOrganisations(vcsOrganisations);
        }
        finally
        {
            semaphore.Release();
        }
        return vcsOrganisations;
    }
}