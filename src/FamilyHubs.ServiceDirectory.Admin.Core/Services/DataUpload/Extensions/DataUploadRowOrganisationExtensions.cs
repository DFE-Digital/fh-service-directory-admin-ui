using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Exceptions;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;

namespace FamilyHubs.ServiceDirectory.Admin.Core.Services.DataUpload.Extensions;

public static class DataUploadRowOrganisationExtensions
{
    public static async Task<OrganisationWithServicesDto> ResolveOrganisation(
        this IGrouping<string, DataUploadRowDto> serviceGroupedData, 
        OrganisationWithServicesDto localAuthority,
        CachedApiResponses cachedApiResponses,
        IOrganisationAdminClientService organisationAdminClientService
    )
    {
        var organisationType = serviceGroupedData.ToList().GetServiceValue(x => x.OrganisationType);

        if (organisationType is OrganisationType.LA or OrganisationType.Company)
            return localAuthority;

        var organisationName = serviceGroupedData.ToList().GetServiceValue(x => x.NameOfOrganisation);

        if (string.IsNullOrWhiteSpace(organisationName))
            throw new DataUploadException($"Name of organisation missing for ServiceOwnerReferenceId:{serviceGroupedData.Key}");

        var organisation = await GetOrganisationByName(organisationAdminClientService, cachedApiResponses, organisationName);

        if (organisation == null)
        {
            //  Non LA organisation does not exist, create it via the API
            organisation = await CreateOrganisation(organisationAdminClientService, localAuthority, organisationName, organisationType);
            cachedApiResponses.OrganisationsWithServices.Add(organisation);
            cachedApiResponses.Organisations.Add(organisation);
        }

        return organisation;

    }

    private static async Task<OrganisationWithServicesDto> CreateOrganisation(
        IOrganisationAdminClientService organisationAdminClientService,
        OrganisationDto localAuthority, 
        string organisationName, 
        OrganisationType organisationType)
    {
        var organisation = new OrganisationWithServicesDto
        {
            AdminAreaCode = localAuthority.AdminAreaCode,
            Name = organisationName,
            OrganisationType = organisationType,
            Description = organisationName,
            AssociatedOrganisationId = localAuthority.Id
        };

        try
        {
            var organisationId = await organisationAdminClientService.CreateOrganisation(organisation);
            organisation.Id = organisationId;
        }
        catch (ApiException ex)
        {
            var msg = $"Failed to create new Organisation :{organisationName} - ";
            foreach (var error in ex.ApiErrorResponse.Errors)
            {
                msg += $"{error.PropertyName}:{error.ErrorMessage} ";
            }

            throw new DataUploadException(msg);
        }

        return organisation;
    }

    private static async Task<OrganisationWithServicesDto?> GetOrganisationByName(
        IOrganisationAdminClientService organisationAdminClientService, 
        CachedApiResponses cachedApiResponses,
        string organisationName)
    {

        var organisation = cachedApiResponses.Organisations.FirstOrDefault(x => string.Equals(x.Name, organisationName, StringComparison.InvariantCultureIgnoreCase));
            
        if (organisation == null)
            return null;

        var organisationWithServices = cachedApiResponses.OrganisationsWithServices.FirstOrDefault(o => o.Id == organisation.Id);

        if (organisationWithServices == null)
            organisationWithServices = await organisationAdminClientService.GetOrganisationById(organisation.Id);

        if (organisationWithServices != null)
            cachedApiResponses.OrganisationsWithServices.Add(organisationWithServices);

        return organisationWithServices;
    }

}