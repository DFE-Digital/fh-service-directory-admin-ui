using FamilyHubs.ServiceDirectory.Admin.Core.Models;

namespace FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;

public interface IRequestDistributedCache
{
    //todo: rename to GetOrganisationAsync
    Task<OrganisationViewModel?> GetAsync(string emailAddress);
    Task SetAsync(string emailAddress, OrganisationViewModel model);

    Task<ServiceModel?> GetServiceAsync(string emailAddress);
    Task SetServiceAsync(string emailAddress, ServiceModel model);

    Task<SubjectAccessRequestViewModel?> GetSarAsync(string emailAddress);
    Task SetSarAsync(string emailAddress, SubjectAccessRequestViewModel model);
}